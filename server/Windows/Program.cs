namespace HomeMediaRemote.Windows
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();

            var app = builder.Build();
            app.UseAuthorization();

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
            });

            app.Run();
        }
    }
}
