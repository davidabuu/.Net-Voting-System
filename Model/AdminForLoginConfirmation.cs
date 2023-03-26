namespace DotnetAPI.Model
{
    public partial class AdminForLoginConfirmation
    {
        public byte[] PasswordHash { get; set; } = new byte[0];
        public byte[] PasswordSalt { get; set; } = new byte[0];
    }
}