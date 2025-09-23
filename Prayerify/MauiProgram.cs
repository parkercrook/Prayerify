using Microsoft.Extensions.Logging;
using Prayerify.Data;
using Prayerify.Pages;
using Prayerify.Services;
using Prayerify.ViewModels;
using SQLitePCL;

namespace Prayerify
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Ensure SQLite is properly initialized before any database operations
            try
            {
                SQLitePCL.Batteries_V2.Init();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQLite initialization failed: {ex.Message}");
                // Continue anyway - try alternative initialization
                try
                {
                    SQLitePCL.Batteries.Init();
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine($"Alternative SQLite initialization failed: {ex2.Message}");
                }
            }

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                    fonts.AddFont("Montserrat-Semibold.ttf", "MontserratSemibold");
                });
       

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Register services, viewmodels, and pages
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "prayerify.db3");
            builder.Services.AddSingleton<IPrayerDatabase>(_ => 
            {
                try
                {
                    return new PrayerDatabase(dbPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create PrayerDatabase: {ex.Message}");
                    // Return a null or throw - this will be handled by the service container
                    throw;
                }
            });
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

            // Initialize the database using sqlite-net-pcl with lazy initialization
            Task.Run(async () => 
            {
                try
                {
                    // Add a longer delay to ensure SQLite is fully initialized
                    await Task.Delay(1000);
                    
                    // Try to initialize the database
                    var localdb = app.Services.GetRequiredService<IPrayerDatabase>();
                    await localdb.InitializeAsync();
                }
                catch (Exception ex)
                {
                    // Log the error for debugging
                    System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    // Try again after another delay
                    try
                    {
                        await Task.Delay(2000);
                        var localdb = app.Services.GetRequiredService<IPrayerDatabase>();
                        await localdb.InitializeAsync();
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine($"Second database initialization attempt failed: {ex2.Message}");
                    }
                }
            });

            return app;
        }
    }
}
