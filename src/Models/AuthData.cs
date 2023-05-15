using ZeroLevel.Services.Serialization;

namespace APILogger.Models
{
    internal sealed class AuthData
        : IBinarySerializable
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string TestKey { get; set; }
        public bool IsAdmin { get; set; }

        public void Deserialize(IBinaryReader reader)
        {
            this.UserId = reader.ReadLong();
            this.Username = reader.ReadString();
            this.TestKey = reader.ReadString();
            this.IsAdmin = reader.ReadBoolean();
        }

        public void Serialize(IBinaryWriter writer)
        {
            writer.WriteLong(this.UserId);
            writer.WriteString(this.Username);
            writer.WriteString(this.TestKey);
            writer.WriteBoolean(this.IsAdmin);
        }
    }
}
