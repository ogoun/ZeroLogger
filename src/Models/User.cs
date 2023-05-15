namespace APILogger.Models
{
    public sealed class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public HashSet<string> Sources { get; set; }
        public long CreateTimestamp { get; set; }
        public bool IsEnabled { get; set; }
    }
}
