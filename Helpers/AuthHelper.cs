using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using APIProject.DTOs;
using APIProject.DCD;
using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;


namespace APIProject.Helpers{

    
    public class AuthHelper{
        
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;
        public AuthHelper(IConfiguration config){
            _dapper = new DataContextDapper(config);
            _config = config;
        }
        
        public byte[] GetPasswordHash(string password, byte[] passwordSalt){
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value 
                + Convert.ToBase64String(passwordSalt);
                    
            //password hash generation
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
                );
        }


        public string CreateToken(int userId){
            Claim[] claims = new Claim[] {
                new Claim("userId", userId.ToString())
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
                (tokenKeyString != null ? tokenKeyString : ""));

            SigningCredentials credentials = new SigningCredentials(
                tokenKey,
                SecurityAlgorithms.HmacSha512Signature);
            
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor(){
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                //rejects token after a day
                Expires = DateTime.Now.AddDays(1)
            };

            //token creation based off descriptor
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);


        }
        //CREATE STORED PROCEDURES THAT ALIGN WITH CURRENT PROJECT
        public bool SetPassword(UserLoginDTO userSetPassword){
            byte[] passwordSalt = new byte[128 / 8];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create()){
                    rng.GetNonZeroBytes(passwordSalt);
                }
                
                //password hash generation
                byte[] passwordHash = GetPasswordHash(userSetPassword.Password, passwordSalt);
                //the @ symbol denotes a variable
                string sqlAddAuth = @"EXEC ProjectSchema.userRegistration_Upsert
                @Email = @EmailParam, 
                @PasswordHash = @PasswordHashParam, 
                @PasswordSalt = @PasswordSaltParam";
            DynamicParameters sqlParameters = new DynamicParameters();

            
            sqlParameters.Add("@EmailParam", userSetPassword.Email, DbType.String);
            sqlParameters.Add("@PasswordHashParam", passwordHash, DbType.Binary);
            sqlParameters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);
            //Console.WriteLine(userSetPassword.Password);
            return _dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters);
        }
    }
}