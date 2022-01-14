using MediatR;
using Microsoft.Extensions.Localization;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Market.Lib.Command;

public class SearchTarkovItem
{
    public const string InputState = "SearchTarkovItem";
    public record Query(UserProfile User, string Message) : IRequest<Unit>;
    public class Handler : IRequestHandler<Query, Unit>
    {
        private readonly ITelegramBotWrapper _tg;
        private readonly IMediator _mediator;
        private readonly IStringLocalizer<SearchTarkovItem> _localizer;
        
        public Handler(ITelegramBotWrapper tg,
            IStringLocalizer<SearchTarkovItem> localizer, 
            IMediator mediator)
        {
            _tg = tg;
            _localizer = localizer;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(Query request, CancellationToken ct)
        {
            var user = request.User;
            var message = request.Message;

            if (message.Length < 3)
            {
                await _tg.SendText(user, _localizer["Minimum number of characters to search 3 characters"]);
                return Unit.Value;
            }

            var data = new ShowTarkovSearchData()
            {
                Message = message,
                Skip = 0,
            };
            
            await _mediator.Send(new SearchTarkovAction.Query(user, data), ct);
            
            return Unit.Value;
        }
    }
}