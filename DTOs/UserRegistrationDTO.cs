namespace APIProject.DTOs{
    public partial class UserRegistrationDTO{
        
        public string Email{get; set;}
        public string Password{get; set;}
        public string ConfirmPassword{get; set;}
        

        public UserRegistrationDTO(){
            if(Email == null){
                Email = "";
            }
            if(Password == null){
                Password = "";
            }
            if(ConfirmPassword == null){
                ConfirmPassword = "";
            }
        }
    }
}