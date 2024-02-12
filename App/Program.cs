namespace App;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webHostBuilder => {
                webHostBuilder.UseStartup<Startup>();
            })
            .Build(); 
        host.Run();
    }
}