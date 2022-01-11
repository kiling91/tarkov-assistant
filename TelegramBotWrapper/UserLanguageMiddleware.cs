using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserLanguageMiddleware> _logger;

        public UserLanguageMiddleware(RequestDelegate next,
            IUserRegistry userRegistry,
            ILogger<UserLanguageMiddleware> logger)
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

        public async Task Invoke(HttpContext context)
        {
            var data = await ReadBuffer(context);
            var update = JsonConvert.DeserializeObject<Update>(data);
            if (update != null)
            {
                var userId = update.Message?.From?.Id ?? -1;
                if (userId == -1)
                    userId = update.CallbackQuery?.From?.Id ?? -1;

                var user = await _userRegistry.FindUser(userId);
                if (user != null && user.Lang != null)
                {
                    ChangeHeaderLang(context, user.Lang);
                    _logger.LogWarning($"Set language {user.Lang}");
                }
                else
                {
                    var lang = update.Message?.From?.LanguageCode;
                    if (lang != null)
                    {
                        ChangeHeaderLang(context, lang);
                        _logger.LogWarning($"Set language {lang}");
                    }
                }
            }

            try
            {
                await _next.Invoke(context);
            }
            catch (NeedReloadLanguageException e)
            {
                if (update != null)
                {
                    var headers = new List<KeyValuePair<string, string>>();
                    foreach (var header in context.Request.Headers)
                        headers.Add(new KeyValuePair<string, string>(header.Key, header.Value));
                    var client = new RestClient($"https://{context.Request.Host.Value}");

                    var request = new RestRequest(context.Request.Path)
                        .AddHeaders(headers)
                        .AddStringBody(data, DataFormat.Json);
                    await client.PostAsync(request);
                }
            }
        }
    }
}