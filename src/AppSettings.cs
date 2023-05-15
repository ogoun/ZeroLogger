using ZeroLevel;

namespace APILogger
{
    public class AppSettings
    {
        public string BuiltinAdmin;
        public string BuiltinPassword;

        public static AppSettings Create() 
        {
            var settings = Configuration.ReadFromIniFile("config.ini").Bind<AppSettings>();
            return settings;
        }
    }
}
