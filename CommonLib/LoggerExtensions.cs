using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace CommonLib
{
    public static class LoggerExtensions
    {
        public static void InitLogger(this LoggerConfiguration app)
        {
            app.MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler",
                    LogEventLevel.Fatal)
                .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
                .MinimumLevel.Override("Serilog", LogEventLevel.Warning)
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {SourceContext} {Message}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code
                );
        }

        public static void InittLocalization(this IApplicationBuilder app)
        {
// Localization
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("ru")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("ru"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });
        }
    }
}