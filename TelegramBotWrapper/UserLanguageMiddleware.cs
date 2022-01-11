using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using Telegram.Bot.Types;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper
{
    public class NeedReloadLanguageException : Exception
    {
        public string Lang { get; private init; }

        public NeedReloadLanguageException(string newLang) : base()
        {
            Lang = newLang;
        }
    }

    public class UserLanguageMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserRegistry _userRegistry;
        private ILogger<UserLanguageMiddleware> _logger;
        public UserLanguageMiddleware(RequestDelegate next,
            IUserRegistry userRegistry, ILogger<UserLanguageMiddleware> logger)
        {
            _next = next;
            _userRegistry = userRegistry;
            _logger = logger;
        }

        private async Task<string> ReadBuffer(HttpContext context)
        {
            var bodyReader = context.Request.BodyReader;
            var readResult = await bodyReader.ReadAsync();
            try
            {
                return Encoding.UTF8.GetString(readResult.Buffer);
            }
            finally
            {
                bodyReader.AdvanceTo(readResult.Buffer.Start);
            }
        }

        private void ChangeHeaderLang(HttpContext context, string lang)
        {
            if (context.Request.Headers.ContainsKey("Accept-Language"))
                context.Request.Headers["Accept-Language"] = lang;
            else
                context.Request.Headers.Add("Accept-Language", lang);
        }

        private async Task ReplayCommand(string data, HttpContext context, BotConfiguration configuration)
        {
            var headers = new List<KeyValuePair<string, string>>();
            foreach (var header in context.Request.Headers)
                headers.Add(new KeyValuePair<string, string>(header.Key, header.Value));
            var client = new RestClient(configuration.HostAddress);

            var request = new RestRequest(context.Request.Path)
                .AddHeaders(headers)
                .AddStringBody(data, DataFormat.Json);
            try
            {
                await client.PostAsync(request);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message);
            }
        }

        private async Task SetHeaderLanguage(string data, HttpContext context)
        {
            var update = JsonConvert.DeserializeObject<Update>(data);
            if (update != null)
            {
                var userId = update.Message?.From?.Id ?? -1;
                if (userId == -1)
                    userId = update.CallbackQuery?.From?.Id ?? -1;

                var user = await _userRegistry.FindUser(userId);
                if (user != null && user.Lang != null)
                    ChangeHeaderLang(context, user.Lang);
                else
                {
                    var lang = update.Message?.From?.LanguageCode;
                    if (lang != null)
                        ChangeHeaderLang(context, lang);
                }
            }
        }

        public async Task Invoke(HttpContext context, IOptions<BotConfiguration> configuration)
        {
            var data = await ReadBuffer(context);
            await SetHeaderLanguage(data, context);
            try
            {
                await _next.Invoke(context);
            }
            catch (NeedReloadLanguageException)
            {
                await ReplayCommand(data, context, configuration.Value);
            }
        }
    }
}