using SQLite;

namespace APILogger.Models.DB
{
    internal sealed class UserRecord
    {
        [Indexed, AutoIncrement, PrimaryKey]
        public long Id { get; set; }
        [Indexed]
        public string UserName { get; set; }
        public byte[] Sources { get; set; }
        public byte[] PwdHach { get; set; }
        public long CreateTimestamp { get; set; }
        public bool IsEnabled { get; set; }
    }
}
