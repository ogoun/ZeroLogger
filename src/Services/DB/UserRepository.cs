using APILogger.Models;
using APILogger.Models.DB;
using APILogger.Models.Requests;
using System.Security.Cryptography;
using System.Text;
using ZeroLevel;
using ZeroLevel.Services.Serialization;

namespace APILogger.Services.DB
{
    internal sealed class UserRepository
         : BaseSqLiteDB<UserRecord>
    {
        private static byte[] salt = new byte[16] { 81, 18, 36, 44, 15, 14, 18, 93, 77, 188, 201, 1, 202, 74, 6, 3 };

        public UserRepository(string name)
            : base(name)
        {
            CreateTable();
        }

        public void Create(UserRequest user)
        {
            if (Count(r => r.UserName == user.UserName) == 0)
            {
                var sources = MessageSerializer.SerializeCompatible(user.Sources);
                var passwordHash = Hash(user.Password);

                var record = new UserRecord
                {
                    CreateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    IsEnabled = true,
                    UserName = user.UserName,
                    PwdHach = passwordHash,
                    Sources = sources
                };

                Append(record);
            }
            else
            {
                throw new Exception($"The name '{user.UserName}' is already in use");
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            return SelectAll().Select(x => new User
            {
                Id = x.Id,
                UserName = x.UserName,
                CreateTimestamp = x.CreateTimestamp,
                IsEnabled = x.IsEnabled,
                Sources = MessageSerializer.DeserializeCompatible<string[]>(x.Sources).ToHashSet()
            });
        }

        public User GetById(long id)
        {
            var user = Single(r => r.Id == id);
            if (user != null)
            {
                return new User
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    CreateTimestamp = user.CreateTimestamp,
                    IsEnabled = user.IsEnabled,
                    Sources = MessageSerializer.DeserializeCompatible<string[]>(user.Sources).ToHashSet()
                };
            }
            return null!;
        }

        public User VerifyUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) == false && string.IsNullOrWhiteSpace(password) == false)
            {
                var user = Single(r => r.UserName == username);
                var hash = Hash(password);
                if (ArrayExtensions.UnsafeEquals(hash, user.PwdHach))
                {
                    return new User
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        CreateTimestamp = user.CreateTimestamp,
                        IsEnabled = user.IsEnabled,
                        Sources = MessageSerializer.DeserializeCompatible<string[]>(user.Sources).ToHashSet()
                    };
                }
            }
            return null!;
        }

        public void AllowSource(long id, string source)
        {
            var user = Single(x => x.Id == id);
            if (user != null)
            {
                var sources = MessageSerializer.DeserializeCompatible<string[]>(user.Sources).ToHashSet();
                sources.Add(source);
                user.Sources = MessageSerializer.SerializeCompatible(sources);
                Update(user);
            }
        }

        public void DenySource(long id, string source)
        {
            var user = Single(x => x.Id == id);
            if (user != null)
            {
                var sources = MessageSerializer.DeserializeCompatible<string[]>(user.Sources).ToHashSet();
                sources.Remove(source);
                user.Sources = MessageSerializer.SerializeCompatible(sources);
                Update(user);
            }
        }

        public void BlockUser(long id)
        {
            var user = Single(x => x.Id == id);
            if (user != null)
            {
                user.IsEnabled = false;
                Update(user);
            }
        }

        public void UnblockUser(long id)
        {
            var user = Single(x => x.Id == id);
            if (user != null)
            {
                user.IsEnabled = true;
                Update(user);
            }
        }

        private static byte[] Hash(string line)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(line), salt, 17813);
            return pbkdf2.GetBytes(20);
        }

        protected override void DisposeStorageData()
        {
        }
    }
}
