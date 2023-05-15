namespace APILogger.Models.Requests
{
    public class LogRecordSearchRequest
    {
        public long? Id { get; set; } = null;
        public DateTime? Start { get; set; } = null;
        public DateTime? End { get; set; } = null;
        public string? Source { get; set; } = null;
        public string? Tag { get; set; } = null;
        public string? Text { get; set; } = null;
        public string? LogLevel { get; set; } = null;
    }
}
