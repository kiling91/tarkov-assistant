using System.Reflection;
using Serilog;
using CommonLib;
using MediatR;
using Microsoft.Extensions.Options;
using Tarkov.Assistant.Telegram.Bot;
using Tarkov.Assistant.Telegram.Bot.Feature;
using TarkovMarket;
using TarkovMarket.Command;
using TarkovMarket.Feature.TarkovMarket;
using Telegram.Bot.Wrapper;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((_, lc) => lc.InitLogger());

// Add services to the container.
var services = builder.Services;

services.InitTelegramBot(builder.Configuration);
// Локализация хранится в ресурсах
services.AddLocalization(options => options.ResourcesPath = "Resources");

// Переопределяем свое хранилище
services.AddSingleton<IUserStateManager, UserStateManager>();
services.AddSingleton<IUserRegistry, UserRegistry>();
// business-logic service
services.AddScoped<ITelegramBotController, TelegramBotController>();
services.AddSingleton<ITarkovMarket, TarkovMarket.Feature.TarkovMarket.TarkovMarket>();

services.Configure<TarkovAssistantConfiguration>(builder.Configuration.GetSection(TarkovAssistantConfiguration.ConfigName));
services.Configure<TarkovMarketConfiguration>(builder.Configuration.GetSection(TarkovMarketConfiguration.ConfigName));

var assembly = Assembly.GetExecutingAssembly();
services.AddMediatR(assembly);
services.AddMediatR(typeof(SearchTarkovItem).GetTypeInfo().Assembly);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseMiddleware<UserLanguageMiddleware>();
    
app.InitLocalization(builder.Configuration);
app.UseRouting();
app.UseCors();
app.InitTelegramBotEndPoint(builder.Configuration);

var tarkovMarket = app.Services.GetRequiredService<ITarkovMarket>();

var conf = app.Services.GetRequiredService<IOptions<TarkovMarketConfiguration>>();
tarkovMarket.LodItems(conf.Value.TarkovMarketDataBaseFolder);
app.Run();