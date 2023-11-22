namespace APIProject.Models{
    //creation of model for each user
    //each user has email and password for login
    public partial class User{
        public int UserId{get; set;}
        public string Email{get; set;}
        public string Password{get; set;}
        


        public User(){
            if(Email == null){
                Email = "";
            }
            if(Password == null){
                Password = "";
            }
        }
    }
    
}