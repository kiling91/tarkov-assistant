using Serilog;
using CommonLib;
using Tarkov.Assistant.Telegram.Bot.Feature;
using Telegram.Bot.Wrapper;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((_, lc) => lc.InitLogger());

// Add services to the container.
var services = builder.Services;
var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();

services.InitTelegramBot(builder.Configuration);
// Локализация хранится в ресурсах
services.AddLocalization(options => options.ResourcesPath = "Resources");

// Переопределяем свое хранилище
services.AddSingleton<IUserStateManager, UserStateManager>();
services.AddSingleton<IUserRegistry, UserRegistry>();
// business-logic service
services.AddScoped<ITelegramBotController, TelegramBotController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseMiddleware<UserLanguageMiddleware>();
app.InitLocalization();
app.UseRouting();
app.UseCors();
app.InitTelegramBotEndPoint(builder.Configuration);

app.Run();