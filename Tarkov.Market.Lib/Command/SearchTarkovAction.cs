using System.Globalization;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tarkov.Market.Lib.Feature;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Market.Lib.Command;

public class SearchTarkovAction
{
    private const int TakeDefault = 3;
    private const int ItemsPerRow = 1;
    
    public const string InputState = "SearchTarkovItem";

    public record Query(UserProfile User, ShowTarkovSearchData Data) : IRequest<Unit>;

    public class Handler : IRequestHandler<Query, Unit>
    {
        private readonly ITelegramBotWrapper _tg;
        private readonly ITarkovMarket _tarkovMarket;
        private readonly IOptions<TarkovMarketConfiguration> _config;
        private readonly IStringLocalizer<SearchTarkovAction> _localizer;
        private readonly string _lang = CultureInfo.CurrentCulture.Name;

        public Handler(ITelegramBotWrapper tg,
            ITarkovMarket tarkovMarket,
            IOptions<TarkovMarketConfiguration> config,
            IStringLocalizer<SearchTarkovAction> localizer)
        {
            _tg = tg;
            _config = config;
            _localizer = localizer;
            _tarkovMarket = tarkovMarket;
        }

        private string TranslateTag(string tag)
        {
            if (tag == "Ammo") return _localizer["Ammo"];
            if (tag == "Magazines") return _localizer["Magazines"];
            if (tag == "Tactical_devices") return _localizer["Tactical devices"];
            if (tag == "Weapon_parts") return _localizer["Weapon parts"];
            if (tag == "Barter") return _localizer["Barter"];
            if (tag == "Sights") return _localizer["Sights"];
            if (tag == "Special_scopes") return _localizer["Special scopes"];
            if (tag == "Provisions") return _localizer["Provisions"];
            if (tag == "Drinks") return _localizer["Drinks"];
            if (tag == "Containers") return _localizer["Containers"];
            if (tag == "Keys") return _localizer["Keys"];
            if (tag == "The_Lab") return _localizer["Keycards"];
            if (tag == "Weapon") return _localizer["Weapon"];
            if (tag == "Melee_weapons") return _localizer["Melee weapons"];
            if (tag == "Interchange") return _localizer["Interchange"];
            if (tag == "Shoreline") return _localizer["Shoreline"];
            if (tag == "Factory") return _localizer["Factory"];
            if (tag == "Reserve") return _localizer["Reserve"];
            if (tag == "Customs") return _localizer["Customs"];
            if (tag == "Woods") return _localizer["Woods"];
            if (tag == "Lighthouse") return _localizer["Lighthouse"];
            if (tag == "Gear") return _localizer["Gear"];
            if (tag == "Face_shields") return _localizer["Face protection"];
            if (tag == "Meds") return _localizer["Meds"];
            if (tag == "Food") return _localizer["Food"];
            if (tag == "Assault_scopes") return _localizer["Assault scopes"];
            if (tag == "Suppressors") return _localizer["Suppressors"];
            if (tag == "Optics") return _localizer["Optics"];
            if (tag == "Compact_Collimators") return _localizer["Compact reflex sights"];
            if (tag == "Collimators") return _localizer["Collimators"];
            if (tag == "Backpacks") return _localizer["Backpacks"];
            if (tag == "Headwear") return _localizer["Headwear"];
            if (tag == "Helmets") return _localizer["Helmets"];
            if (tag == "Facecovers") return _localizer["Facecovers"];
            if (tag == "Tactical_rigs") return _localizer["Chest rigs"];
            if (tag == "Headsets") return _localizer["Headsets"];
            if (tag == "Armor_vests") return _localizer["Armor vests"];
            if (tag == "Assault_rifles") return _localizer["Assault rifles"];
            if (tag == "Assault_carbines") return _localizer["Assault carbines"];
            if (tag == "Bolt_action_rifles") return _localizer["Bolt action rifles"];
            if (tag == "Machine_guns") return _localizer["Light machine guns"];
            if (tag == "Marksman_rifles") return _localizer["Designated marksman rifles"];
            if (tag == "Pistols") return _localizer["Pistols"];
            if (tag == "Shotguns") return _localizer["Shotguns"];
            if (tag == "SMGs") return _localizer["Submachine guns"];
            if (tag == "Throwables") return _localizer["Hand grenade"];
            if (tag == "Ammo") return _localizer["Ammo"];
            if (tag == "Handguards") return _localizer["Handguards"];
            if (tag == "Mounts") return _localizer["Mounts"];
            if (tag == "Barrels") return _localizer["Barrels"];
            if (tag == "Iron_sights") return _localizer["Iron sights"];
            if (tag == "Ammo_boxes") return _localizer["Secure containers"];
            if (tag == "Armband") return _localizer["Armband"];
            if (tag == "Currency") return _localizer["Currency"];
            if (tag == "Grenade_Launchers") return _localizer["Grenade launchers"];
            if (tag == "Special_equipment") return _localizer["Special equipment"];
            if (tag == "Bipods") return _localizer["Bipods"];
            if (tag == "Maps") return _localizer["Maps"];
            if (tag == "Stocks_chassis") return _localizer["Stocks & chassis"];
            if (tag == "Pistol_grips") return _localizer["Pistol grips"];
            if (tag == "Muzzle_adapters") return _localizer["Muzzle adapters"];

            throw new ArgumentOutOfRangeException(nameof(tag));
        }

        private string RenderText(TarkovItem item)
        {
            var text = "";
            text += $"Name: {item.Translation?[_lang].ShortName!}\n";
            text += $"Flea price: {item.BasePrice}₽\n";
            return text;
        }

        private Dictionary<string, int> GetTags(int count, string message, string? tag)
        {
            var tags = new Dictionary<string, int>();
            var search = _tarkovMarket.SearchByName(message, 0, count, tag);
            foreach (var item in search.Items)
            {
                foreach (var itag in item.Tags!)
                {
                    if (!tags.ContainsKey(itag))
                        tags.Add(itag, 0);
                    tags[itag]++;
                }
            }

            return tags.OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<Unit> Handle(Query request, CancellationToken ct)
        {
            var user = request.User;
            var data = request.Data;

            var search =
                _tarkovMarket.SearchByName(data.Message!, data.Skip, TakeDefault, data.Tag);

            if (search.AllCount <= 0)
            {
                await _tg.SendText(user, _localizer["No results found for your search"]);
                return Unit.Value;
            }

            var tags = GetTags(search.AllCount, data.Message!, data.Tag);
            var skip = data.Skip + TakeDefault;

            var showItems = search.AllCount <= TakeDefault || search.MainTagsCount <= 1;
            if (showItems)
            {
                await _tg.SendText(user, _localizer["Found the following items"]);
                foreach (var item in search.Items)
                {
                    var icon = Path.Join(_config.Value.TarkovMarketDataBaseFolder, item.IconLg);
                    await _tg.SendPhoto(user, icon, RenderText(item));
                }
            }

            var inlineMenu = new InlineMenu(ShowTarkovHandler.Key)
            {
                ItemsPerRow = ItemsPerRow,
            };

            // Show tags
            if (tags.Count > 1)
                foreach (var tag in tags)
                {
                    var inputTagData = new ShowTarkovSearchData()
                    {
                        Message = data.Message,
                        Tag = tag.Key,
                        Skip = 0,
                    };
                    inlineMenu.Items.Add(
                        new InlineMenuItem(_localizer["{0} {1} items", TranslateTag(tag.Key), tag.Value])
                        {
                            Data = JsonConvert.SerializeObject(inputTagData),
                        });
                }

            if (search.AllCount <= skip) return Unit.Value;

            // Show more...
            if (showItems)
            {
                var inputData = new ShowTarkovSearchData()
                {
                    Message = data.Message,
                    Skip = data.Skip + TakeDefault,
                    Tag = data.Tag
                };
                inlineMenu.Items.Add(new InlineMenuItem(_localizer["Show more..."])
                {
                    Data = JsonConvert.SerializeObject(inputData),
                });
            }

            var showItemsText = _localizer["Showing {0} of {1} items", skip, search.AllCount];
            if (!showItems)
                showItemsText = _localizer["Refine your request"];
            await _tg.SendInlineMenu(request.User, showItemsText, inlineMenu);
            return Unit.Value;
        }
    }
}