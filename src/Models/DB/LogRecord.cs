using SQLite;

namespace APILogger.Models.DB
{
    public sealed class LogRecord
    {
        [Indexed, AutoIncrement, PrimaryKey]
        public long Id { get; set; }
        [Indexed]
        public long Timestamp { get; set; }
        [Indexed]
        public string Source { get; set; } = string.Empty;
        [Indexed]
        public string Tag { get; set; } = string.Empty;
        [Indexed]
        public string Text { get; set; } = string.Empty;
        [Indexed]
        public string LogLevel { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string RemoteAddress { get; set; }
    }
}
