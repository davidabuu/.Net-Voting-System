namespace DotnetAPI.Model
{
    public partial class CandidateModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string CandidateRole { get; set; } = "";
        public byte[] ImageData { get; set; } = new byte[0];
        public int VoteCount { get; set; }
        public string AdminEmailAddress { get; set; } = "";
    }
}