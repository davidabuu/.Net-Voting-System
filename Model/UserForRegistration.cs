namespace DotnetAPI.Model
{
    public partial class UserForRegistration
    {

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        public string EmailAddress { get; set; } = "";

        public string Password { get; set; } = "";

        public string ConfirmPassword { get; set; } = "";


        public int Verified { get; set; }
        public int VotedForPresident { get; set; }
        public int VotedForVicePresident { get; set; }
        public int VotedForPublicRelationOfficer { get; set; }
    }
}