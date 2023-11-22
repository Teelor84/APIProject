using System.Data;
using System.Reflection.Metadata.Ecma335;
using Dapper;
using Microsoft.Data.SqlClient;

namespace APIProject.DCD{
    //the data context dapper uses the dapper framework to manage database connections
    class DataContextDapper{
        private readonly IConfiguration _config;
        public DataContextDapper(IConfiguration config){
            _config = config;
        }

        //returns all data types from the table
        public IEnumerable<T> LoadData<T>(string sql){
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }
        //returns a single data type
        public T LoadDataSingle<T>(string sql){
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }
        //load all data based on a certain parameter such as email
        public IEnumerable<T> LoadDataWithParameters<T>(string sql, DynamicParameters parameters){
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql, parameters);
        }
        //load a single data value based off a parameter
       public T LoadDataSingleWithParameters<T>(string sql, DynamicParameters parameters){
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql, parameters);
        }
        //executes a sql string with the parameters passed in 
        public bool ExecuteSqlWithParameters(string sql, DynamicParameters parameters){
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, parameters) > 0;
        }


    }
}