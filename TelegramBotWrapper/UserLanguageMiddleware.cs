using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper
{
    public class UserLanguageMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserRegistry _userRegistry;
        
        public UserLanguageMiddleware(RequestDelegate next, IUserRegistry userRegistry)
        {
            _next = next;
            _userRegistry = userRegistry;
        }

        public async Task Invoke(HttpContext context)
        {
            var bodyReader = context.Request.BodyReader;
            var readResult = await bodyReader.ReadAsync();
            try
            {
                var data = Encoding.UTF8.GetString(readResult.Buffer);
                var update = JsonConvert.DeserializeObject<Update>(data);
                if (update != null)
                {
                    var userId = update.Message?.From?.Id ?? -1;
                    var user = await _userRegistry.FindUser(userId);
                    if (user != null && user.Lang != null)
                        context.Request.Headers.Add("Accept-Language", user.Lang);
                    else
                    {
                        var lang = update.Message?.From?.LanguageCode;
                        if (lang != null)
                            context.Request.Headers.Add("Accept-Language", lang);
                    }
                }
            }
            finally
            {
                bodyReader.AdvanceTo(readResult.Buffer.Start);
            }

            await _next(context);
        }
    }
}