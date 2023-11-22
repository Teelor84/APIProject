namespace APIProject.DTOs{
    public partial class UserLoginConfirmationDTO{
        public byte[] PasswordHash {get; set;}
        public byte[] PasswordSalt {get; set;}

        UserLoginConfirmationDTO(){
            if(PasswordHash == null){
                PasswordHash = new byte[0];
            }

            if(PasswordSalt == null){
                PasswordSalt = new byte[0];
            }
        }
    }
}