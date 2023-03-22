namespace DotnetAPI.Model
{
    public partial class UserForLoginConfirmation
    {
        public byte[] PasswordHash { get; set; } = new byte[0];
        public byte[] PasswordSalt { get; set; } = new byte[0];
    }
}