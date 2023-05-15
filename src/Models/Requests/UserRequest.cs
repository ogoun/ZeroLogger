namespace APILogger.Models.Requests
{
    public sealed class UserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public HashSet<string> Sources { get; set; }
        public void ValidateModel()
        {
            if (string.IsNullOrWhiteSpace(UserName)) throw new ArgumentException("The name must not be empty.");
            if (string.IsNullOrWhiteSpace(Password)) throw new ArgumentException("The password must not be empty.");
        }
    }
}
