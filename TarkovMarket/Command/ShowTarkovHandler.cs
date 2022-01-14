using MediatR;
using Newtonsoft.Json;
using Telegram.Bot.Wrapper.UserRegistry;

namespace TarkovMarket.Command;

public class ShowTarkovHandler
{
    public const string Key = "search_tarkov_item/show_tarkov_item_handler";

    public record Query(UserProfile User, string? Data) : IRequest<Unit>;

    public class Handler : IRequestHandler<Query, Unit>
    {
        private readonly IMediator _mediator;

        public Handler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Unit> Handle(Query request, CancellationToken ct)
        {
            var data = JsonConvert.DeserializeObject<ShowTarkovSearchData>(request.Data!);
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            await _mediator.Send(new SearchTarkovAction.Query(request.User, data), ct);
            return Unit.Value;
        }
    }
}