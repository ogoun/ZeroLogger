namespace APILogger.Models.Requests
{
    public class LogRecordRequest
    {
        public string Tag { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string LogLevel { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
