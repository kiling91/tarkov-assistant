using System.Globalization;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Tarkov.Assistant.Telegram.Bot.TarkovMarket;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Command;

public class SearchTarkovItem
{
    public const string InputState = "SearchTarkovItem";
    public record Query(UserProfile User, string Message) : IRequest<Unit>;

    public class Handler : IRequestHandler<Query, Unit>
    {
        private readonly ITelegramBotWrapper _tg;
        private readonly ITarkovMarket _tarkovMarket;
        private readonly IOptions<TarkovAssistantConfiguration> _config;
        private readonly IStringLocalizer<ChangeLanguage> _localizer;
        private readonly ILogger<ChangeLanguage> _logger;

        public Handler(ITelegramBotWrapper tg,
            ITarkovMarket tarkovMarket,
            IOptions<TarkovAssistantConfiguration> config,
            ILogger<ChangeLanguage> logger,
            IStringLocalizer<ChangeLanguage> localizer)
        {
            _tg = tg;
            _config = config;
            _logger = logger;
            _localizer = localizer;
            _tarkovMarket = tarkovMarket;
        }

        private string RenderText(TarkovItem item, string ln)
        {
            var text = "";
            text += $"Name: {item.Translation?[ln].ShortName!}\n";
            text += $"Flea price: {item.BasePrice}₽\n";
            return text;
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken ct)
        {
            await Task.CompletedTask;
            var ln = CultureInfo.CurrentCulture.Name;
            var user = request.User;
            var message = request.Message;

            if (message.Length < 3)
            {
                await _tg.SendText(user, _localizer["Min input 3 symbol"]);
                return Unit.Value;
            }
            
            var items = _tarkovMarket.SearchByName(message, ln, 0, 3);
            foreach (var item in items)
            {
                var icon = Path.Join(_config.Value.TarkovMarketDataBaseFolder, item.Icon);
                await _tg.SendPhoto(user, icon, RenderText(item, ln));
            }

            
            return Unit.Value;
        }
    }
}