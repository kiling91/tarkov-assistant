using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Wrapper.Services;
using Telegram.Bot.Wrapper.TelegramBot;

namespace Telegram.Bot.Wrapper;

public static class TelegramBotExtensions
{
    public static void InitTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        var botConfig = configuration.GetSection(BotConfiguration.ConfigName).Get<BotConfiguration>();

        // There are several strategies for completing asynchronous tasks during startup.
        // Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
        // We are going to use IHostedService to add and later remove Webhook
        services.AddHostedService<ConfigureWebhook>();
        services.AddSingleton<ICallbackStorage, CallbackStorage>();
        services.AddScoped<ITelegramBotWrapper, TelegramBotWrapper>();
        services.AddScoped<IHandleUpdateService, HandleUpdateService>();
        // Register named HttpClient to get benefits of IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient("tgwebhook")
            .AddTypedClient<ITelegramBotClient>(httpClient
                => new TelegramBotClient(botConfig.BotToken, httpClient));

        // The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
        // incoming webhook updates and send serialized responses back.
        // Read more about adding Newtonsoft.Json to ASP.NET Core pipeline:
        //   https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-5.0#add-newtonsoftjson-based-json-format-support
        services.AddControllers()
            .AddNewtonsoftJson();
    }

    public static void InitTelegramBotEndPoint(this IApplicationBuilder app, IConfiguration configuration)
    {
        var botConfig = configuration.GetSection(BotConfiguration.ConfigName).Get<BotConfiguration>();

        app.UseEndpoints(endpoints =>
        {
            // Configure custom endpoint per Telegram API recommendations:
            // https://core.telegram.org/bots/api#setwebhook
            // If you'd like to make sure that the Webhook request comes from Telegram, we recommend
            // using a secret path in the URL, e.g. https://www.example.com/<token>.
            // Since nobody else knows your bot's token, you can be pretty sure it's us.
            var token = botConfig.BotToken;
            endpoints.MapControllerRoute(name: "tgwebhook",
                pattern: $"bot/{token}",
                new { controller = "Webhook", action = "Post" });
            endpoints.MapControllers();
        });
    }
}