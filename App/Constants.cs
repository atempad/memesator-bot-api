namespace App;

public static class Constants
{
    public static class RequestHeaders
    {
        public const string UserAgent = "User-Agent";
        public const string Cookie = "Cookie";
        public const string TelegramBotApiSecretToken = "X-Telegram-Bot-Api-Secret-Token";
    }
    public static class DB
    {
        public static class Containers
        {
            public const string Users = "Users";
            public const string Subscriptions = "Subscriptions";
        }
    }
    public static class Video
    {
        public const int MaxSize = 50 * 1024 * 1024;
        public const int ChunkSize = 10 * 1024 * 1024;
    }
}