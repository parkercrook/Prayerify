using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Prayerify.Data;
using Prayerify.ViewModels;
using Prayerify.Pages;
using CommunityToolkit.Mvvm.Messaging;
using Prayerify.Services;

namespace Prayerify
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // Register services, viewmodels, and pages
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "prayerify.db3");
            builder.Services.AddSingleton<IPrayerDatabase>(_ => new PrayerDatabase(dbPath));
            builder.Services.AddSingleton<IGenericService, GenericService>();

            builder.Services.AddTransient<PrayersViewModel>();
            builder.Services.AddTransient<PrayersPage>();
            builder.Services.AddTransient<EditPrayerViewModel>();
            builder.Services.AddTransient<EditPrayerPage>();
            builder.Services.AddTransient<CategoriesViewModel>();
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddTransient<SessionSetupViewModel>();
            builder.Services.AddTransient<SessionSetupPage>();
            builder.Services.AddTransient<SessionRunViewModel>();
            builder.Services.AddTransient<SessionRunPage>();
            builder.Services.AddTransient<AnsweredPrayersViewModel>();
            builder.Services.AddTransient<AnsweredPrayersPage>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<SettingsPage>();

            var app = builder.Build();

            var db = app.Services.GetRequiredService<IPrayerDatabase>();
            Task.Run(() => db.InitializeAsync());

            return app;
        }
    }
}
