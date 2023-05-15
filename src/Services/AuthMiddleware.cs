using APILogger.Models;
using System.Web;
using ZeroLevel.Services;

namespace APILogger.Services
{
    internal static class Middlewares
    {
        private const string KEY = "ulFADbadf83#HjfK@BbfA:F8vVHSABN@RKAFBLKJB@ufvbadsFPIUr78fB342g25gHDF*&@(!G!uhgdnFHjBRFWbre210)!*U@hOD@WH8uED!@BcXASbrtf#ub@#";
        private const string SALT = "FIUASH3rUIFSdab#@RQub9F*)DBH#URkhjbfaA";
        internal const string TEST_KEY = "LoggerTestKey#version1.0.0.0";
        internal static Lazy<TokenEncryptor> Cryptor = new Lazy<TokenEncryptor>(() => new TokenEncryptor(KEY, SALT));
        internal static async Task HandleStartRequest(HttpContext context)
        {
            string remote = $"{context.Connection.RemoteIpAddress?.ToString() ?? string.Empty}:{context.Connection.RemotePort}";
            context.Items.Add("remoteAddress", remote);

            if (context.Request.Path.Value?.Contains("/web/") ?? false) return;
            if (context.Request.Path.Value?.Contains("/api/auth") ?? false) return;
            string token = context.Request?.Headers["X-Token"]!;
            if (string.IsNullOrWhiteSpace(token))
            {
                token = HttpUtility.ParseQueryString(context.Request?.QueryString.Value ?? string.Empty)?.Get("token")!;
            }
            var authData = ReadAccountInfoFromToken(token);
            if (authData != null)
            {
                context.Items.Add("UserId", authData.UserId);
                if (authData.IsAdmin)
                {
                    context.Items.Add("IsAdmin", true);
                }
                return;
            }
            throw new Exception("Invalid or missing token");
        }

        internal static AuthData ReadAccountInfoFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) == false)
            {
                var decryptor = Cryptor.Value.ReadFromToken<AuthData>(token);
                if (string.Compare(decryptor.TestKey, TEST_KEY, true) == 0)
                {
                    return decryptor;
                }
            }
            return null!;
        }
    }
}
