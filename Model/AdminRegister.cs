namespace DotnetAPI.Model
{
    public partial class AdminRegister
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        public string EmailAddress { get; set; } = "";

        public string Password { get; set; } = "";

        public string ConfirmPassword { get; set; } = "";
    }
}