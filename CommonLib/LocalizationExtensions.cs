using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Wrapper;

namespace CommonLib;

public static class LocalizationExtensions
{
    public static void InitLocalization(this IApplicationBuilder app, IConfiguration configuration)
    {
        var conf = configuration.GetSection(AvailableLanguagesConfiguration.ConfigName)
            .Get<AvailableLanguagesConfiguration>();
        if (conf == null || conf.Languages == null || !conf.Languages.Any())
            // TODO: Свой Exeption
            throw new ArgumentOutOfRangeException(AvailableLanguagesConfiguration.ConfigName);

        var supportedCultures = conf.Languages.Select(x => new CultureInfo(x.LanguageCode)).ToArray();

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(conf.Languages.First().LanguageCode),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
        });
    }
}