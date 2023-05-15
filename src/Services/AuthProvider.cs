using APILogger.Models;
using APILogger.Models.Requests;
using APILogger.Services.DB;
using ZeroLevel;
using ZeroLevel.Services.Shedulling;

namespace APILogger.Services
{
    internal sealed class UserCachee
        : IDisposable
    {
        private readonly ISheduller _updateSheduller;
        private readonly IDictionary<long, User> _cache = new Dictionary<long, User>();

        public UserCachee()
        {
            _updateSheduller = Sheduller.Create();
        }

        public void Dispose()
        {
            _updateSheduller.Dispose();
        }

        public User GetOrAdd(long id, Func<long, User> add)
        {
            if (_cache.TryGetValue(id, out var u))
            {
                return u;
            }
            var user = add(id);
            if (user != null)
            {
                _cache.Add(id, user);
                Sheduller.RemindAfter(TimeSpan.FromMinutes(1), () =>
                {
                    _cache.Remove(id);
                });
            }
            return user;
        }
    }

    internal static class AuthProvider
    {
        private static string BuiltInAdmin;
        private static string BuiltInPassword;
        private static UserRepository _repository;
        private static UserCachee _userCachee = new UserCachee();

        public static void Init(AppSettings appSettings, UserRepository repository)
        {
            _repository = repository;
            BuiltInAdmin = appSettings.BuiltinAdmin;
            BuiltInPassword = appSettings.BuiltinPassword;
        }
        public static string Auth(string username, string password, string source)
        {
            if (username == BuiltInAdmin && password == BuiltInPassword)
            {
                return Middlewares.Cryptor.Value.WriteToToken(new AuthData { ApplicationName = source, TestKey = Middlewares.TEST_KEY, IsAdmin = true, UserId = -1 });
            }
            var user = VerifyUser(username, password);
            if (user != null)
            {
                return Middlewares.Cryptor.Value.WriteToToken(new AuthData { ApplicationName = source, TestKey = Middlewares.TEST_KEY, IsAdmin = false, UserId = user.Id });
            }
            return string.Empty;
        }

        public static User GetUserThoughCachee(long userId)
        {
            return _userCachee.GetOrAdd(userId, id => _repository.GetById(userId));
        }

        public static void CreateUseer(UserRequest user) => _repository.Create(user);
        public static User VerifyUser(string username, string password) => _repository.VerifyUser(username, password);
        public static IEnumerable<User> GetUsers() => _repository.GetAllUsers();
        public static void BlockUser(long id) => _repository.BlockUser(id);
        public static void UnblockUser(long id) => _repository.UnblockUser(id);
        public static void AllowSource(long id, string source) => _repository.AllowSource(id, source);
        public static void DenySource(long id, string source) => _repository.DenySource(id, source);
    }
}
