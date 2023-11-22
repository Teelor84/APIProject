namespace APIProject.DTOs{
    public partial class UserLoginDTO{
        public string Email{get; set;}
        public string Password{get; set;}

         public UserLoginDTO(){
            if(Email == null){
                Email = "";
            }
            if(Password == null){
                Password = "";
            }
        }
    }
}