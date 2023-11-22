using Dapper;
using APIProject.DCD;
using APIProject.Models;
using System.Data;

namespace APIProject.Helpers{
    public class ReusableSQL{
        private readonly DataContextDapper _dapper;

        public ReusableSQL(IConfiguration config){
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(User user){
            string sql = @"EXEC ProjectSchema.user_Upsert
            @UserId = @UserIdParam,
            @Email = @EmailParam,
            @Password = @PasswordParam";

            DynamicParameters sqlParamters = new DynamicParameters();
            sqlParamters.Add("@UserIdParam", user.UserId, DbType.Int32);
            sqlParamters.Add("@EmailParam", user.Email, DbType.String);
            sqlParamters.Add("@PasswordParam", user.Password, DbType.String);
            

            return _dapper.ExecuteSqlWithParameters(sql, sqlParamters);
        }
    }
}