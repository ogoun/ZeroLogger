namespace APILogger.Models.Requests
{
    public class AuthRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Source { get; set; }
    }
}
