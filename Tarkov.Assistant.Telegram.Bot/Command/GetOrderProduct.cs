using System.Globalization;
using MediatR;
using Microsoft.Extensions.Options;
using Tarkov.Assistant.Telegram.Bot.Feature;
using Telegram.Bot.Wrapper;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Command;

public class GetOrderProduct
{
    public record Query(UserProfile user) : IRequest<Unit>;

    public class Handler : IRequestHandler<Query, Unit>
    {
        private readonly IUserRegistry _userRegistry;
        private readonly ILogger<GetOrderProduct> _logger;
        private readonly IOptions<AvailableLanguagesConfiguration> _availableLanguagesConfiguration;
        private readonly ITelegramBotWrapper _tg;
        private readonly ITelegramBotController _telegramBotController;
        
        public Handler(ILogger<GetOrderProduct> logger,
            IOptions<AvailableLanguagesConfiguration> availableLanguagesConfiguration,
            IUserRegistry userRegistry,
            ITelegramBotWrapper tg, ITelegramBotController telegramBotController)
        {
            _logger = logger;
            _availableLanguagesConfiguration = availableLanguagesConfiguration;
            _tg = tg;
            _telegramBotController = telegramBotController;
            _userRegistry = userRegistry;
        }

        async Task HandleChangeLanguage(InlineMenuItem menuItem, UserProfile user)
        {
            var newLang = menuItem.Key;
            if (user.Lang != menuItem.Key && newLang != null)
            {
                await _userRegistry.ChangeLang(user.Id, newLang);
                throw new NeedReloadLanguageException(newLang);
            }
            else
            {
                _tg.SetupMainMenu(_telegramBotController.InitMainMenu());
                await _tg.DrawMainMenu(user);
            }
        }
        
        public async Task<Unit> Handle(Query request, CancellationToken ct)
        {
            _logger.LogInformation($"User: {request.user.Id}, Language");
            var inlineMenu = new InlineMenu();
            foreach (var lng in _availableLanguagesConfiguration.Value.Languages!)
            {
                if (CultureInfo.CurrentCulture.Name == lng.LanguageCode)
                    continue;

                inlineMenu.Items.Add(new InlineMenuItem()
                {
                    ItemName = lng.LanguageName,
                    Key = lng.LanguageCode,
                    Callback = HandleChangeLanguage
                });
            }

            await _tg.SendInlineMenu(request.user, $"Select language", inlineMenu);

            return Unit.Value;
        }
    }
}