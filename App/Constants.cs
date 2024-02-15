namespace App;

public static class Constants
{
    public static class RequestHeaders
    {
        public const string TelegramBotApiSecretToken = "X-Telegram-Bot-Api-Secret-Token";
    }
    public static class DB
    {
        public const string Id = "MemesatorDB";

        public static class Containers
        {
            public const string Users = "Users";
            public const string Subscribtions = "Subscriptions";
        }
    }
    public static class Bot
    {
        public const string StartCommand = "/start";
        public const string StopCommand = "/stop";
    }
}