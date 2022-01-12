using System.Globalization;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Telegram.Bot.Wrapper;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Command;

public class SelectLanguageHandler
{
    public const string Key = "change_language/select_language_handler";

    public record Query(UserProfile User, string? Data) : IRequest<Unit>;

    public class Handler : IRequestHandler<Query, Unit>
    {
        private readonly ITelegramBotWrapper _tg;
        private readonly IUserRegistry _userRegistry;
        private readonly ITelegramBotController _controller;

        public Handler(ITelegramBotWrapper tg,
            IUserRegistry userRegistry,
            ITelegramBotController controller)
        {
            _tg = tg;
            _userRegistry = userRegistry;
            _controller = controller;
        }

        public async Task<Unit> Handle(Query request, CancellationToken ct)
        {
            var newLang = request.Data;
            var user = request.User;

            if (user.Lang != newLang)
            {
                if (newLang == null)
                    return Unit.Value;

                await _userRegistry.ChangeLang(user.Id, newLang);
                throw new NeedReloadLanguageException(newLang);
            }

            _tg.SetupMainMenu(_controller.InitMainMenu());
            await _tg.DrawMainMenu(user);
            return Unit.Value;
        }
    }
}

public class ChangeLanguage
{
    public record Query(UserProfile User) : IRequest<Unit>;

    public class Handler : IRequestHandler<Query, Unit>
    {
        private readonly ITelegramBotWrapper _tg;
        private readonly IOptions<AvailableLanguagesConfiguration> _languages;
        private readonly IStringLocalizer<ChangeLanguage> _localizer;
        private readonly ILogger<ChangeLanguage> _logger;

        public Handler(ITelegramBotWrapper tg,
            IOptions<AvailableLanguagesConfiguration> languages,
            ILogger<ChangeLanguage> logger,
            IStringLocalizer<ChangeLanguage> localizer)
        {
            _tg = tg;
            _languages = languages;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<Unit> Handle(Query request, CancellationToken ct)
        {
            _logger.LogInformation($"User: {request.User.Id}, Language");
            var inlineMenu = new InlineMenu(SelectLanguageHandler.Key);
            
            foreach (var lng in _languages.Value.Languages!)
            {
                if (CultureInfo.CurrentCulture.Name == lng.LanguageCode)
                    continue;

                inlineMenu.Items.Add(new InlineMenuItem(lng.LanguageName)
                {
                    Data = lng.LanguageCode,
                });
            }

            await _tg.SendInlineMenu(request.User, _localizer["Select language"], inlineMenu);

            return Unit.Value;
        }
    }
}