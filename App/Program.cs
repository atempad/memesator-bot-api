namespace App;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webHostBuilder => {
                webHostBuilder.UseStartup<Startup>();
            })
            .Build()
            .Run();
    }
}