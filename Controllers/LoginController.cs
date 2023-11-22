using APIProject.DTOs;
using APIProject.Helpers;
using APIProject.DCD;
using APIProject.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;


namespace APIProject.Controllers{

    
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase{

        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSQL _reusableSql;
        private readonly IMapper _mapper;
        public LoginController(IConfiguration config){
            //receives the version of DataContextDapper which has connection string necessary to
            //access the database, config is the connection string, DCD requires it
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSQL(config);
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserRegistrationDTO, User>();
            }));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserRegistrationDTO userRegistration){
            //verify that the users inputs are the same
            if(userRegistration.Password == userRegistration.ConfirmPassword){
                string sqlCheckForUser = "SELECT Email FROM ProjectSchema.ProjectTable WHERE Email = '" 
                    + userRegistration.Email + "'";
                //load in all users and then perform the check
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckForUser);
                //if the user does not exist then get the email and password from the registration
                //dto and set them to a variable
                if(existingUsers.Count() == 0){
                    UserLoginDTO userLogin = new UserLoginDTO(){
                        Email = userRegistration.Email,
                        Password = userRegistration.Password
                    };
                if(_authHelper.SetPassword(userLogin)){
                    User user = _mapper.Map<User>(userRegistration);

                    if(_reusableSql.UpsertUser(user)){
                        return Ok();
                    }
                    throw new Exception("Failed to add user");
                }
                throw new Exception("Failed to register user");
                }
                throw new Exception("User with this email already exists");
            }
            throw new Exception("Passwords do not match");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserLoginDTO userForLogin){
            string sqlForHashAndSalt = @"EXEC ProjectSchema.LoginConfirmation_Get
                @Email = @EmailParam";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            UserLoginConfirmationDTO userForConfirmation = _dapper.LoadDataSingleWithParameters<UserLoginConfirmationDTO>(sqlForHashAndSalt, sqlParameters);
    
            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);
  
            for(int index = 0; index < passwordHash.Length; index++){
                if(passwordHash[index] != userForConfirmation.PasswordHash[index]){
                    return StatusCode(401, "Incorrect password");
                }
            }
            
            string userIdSql = @"SELECT UserId 
                FROM ProjectSchema.ProjectTable
                WHERE Email = '" + userForLogin.Email + "'";
            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });
        }
        //put creates new resources or replaces them, in this case replacing password
        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserLoginDTO userPasswordReset){
            if(_authHelper.SetPassword(userPasswordReset)){
                return Ok();
            }
            throw new Exception("Failed to reset password");
        }


        [HttpGet("RefreshToken")]
        public string RefreshToken(){
            string userIdSql = @"SELECT UserId 
                FROM ProjectSchema.ProjectTable 
                WHERE UserId = '" +
                User.FindFirst("userId")?.Value + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return _authHelper.CreateToken(userId);
        }
    }
}