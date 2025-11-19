using Microsoft.Extensions.Logging;
using Smart_Resume_Job_Search_App.Infrastructure;
using Smart_Resume_Job_Search_App.Presentation;
using Smart_Resume_Job_Search_App.UINav;
using Smart_Resume_Job_Search_App.AppServices;
using System.Diagnostics;
using Smart_Resume_Job_Search_App.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;   // 👈 added

namespace Smart_Resume_Job_Search_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Debug.WriteLine("[MauiProgram] CreateMauiApp starting");
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Load appsettings.json (must be Content + Copy to Output Directory)
            builder.Configuration.AddJsonFile(
                "appsettings.json",
                optional: true,
                reloadOnChange: false);

            var services = builder.Services;

            // local SQLite database path (per user / per device)
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "smartresume.db");

            //  Database context with SQLite
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Job query + Adzuna integration
            // Base Adzuna API URL 
            services.AddSingleton(sp =>
                new JobQueryBuilder("http://api.adzuna.com:80"));

            // Typed HttpClient + JobSearchService (gets HttpClient, AppDbContext,
            // JobQueryBuilder, IConfiguration injected automatically)
            services.AddHttpClient<JobSearchService>();

            // App services
            services.AddSingleton<INetworkStatus, NetworkStatus>();
            services.AddSingleton<AppWorkflow>();               // register workflow for DI (even though ApplicationFacade currently 'new's it)
            services.AddSingleton<ApplicationFacade>();
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddSingleton<IViewResolver, ViewResolver>();
            services.AddSingleton<INavigationService, ShellNavigation>();
            services.AddSingleton<IAppFlow, AppCoordinator>();
            services.AddSingleton<TextExtractionService>();
            services.AddSingleton<ITextExtractor, TxtExtractor>();
            services.AddSingleton<ITextExtractor, DocxExtractor>();
            services.AddSingleton<ITextExtractor, PdfExtractor>();

            // ViewModels
            services.AddTransient<HomeViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Global exception logging hooks for extra visibility
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Debug.WriteLine($"[UnhandledException] {e.ExceptionObject}");
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Debug.WriteLine($"[UnobservedTaskException] {e.Exception}");
                e.SetObserved();
            };

            var app = builder.Build();

            //  Ensure database + tables are created (no migrations, no dotnet ef)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            Debug.WriteLine("[MauiProgram] CreateMauiApp completed");
            return app;
        }
    }
}
