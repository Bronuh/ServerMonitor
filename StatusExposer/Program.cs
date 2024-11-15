using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;

namespace StatusExposer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        // Configure Forwarded Headers options
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            
            // Adjust this setting to match the reverse proxy's IP range if it's static
            options.KnownProxies.Clear(); // Clear KnownProxies if not relevant or set if IP range is known
            options.KnownNetworks.Clear(); // Clear or set KnownNetworks based on the proxy network
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseAuthorization();
        app.Urls.Add("http://*:80");
        
        // Apply Forwarded Headers Middleware
        app.UseForwardedHeaders();

        app.MapGet("/online", (HttpContext httpContext) => Results.Ok());

        app.Run();
    }
}