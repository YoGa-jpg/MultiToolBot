namespace MultiToolBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new MultiToolBot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
