using Microsoft.Extensions.Logging;
using Smart_Resume_Job_Search_App.Infrastructure;
using Smart_Resume_Job_Search_App.Presentation;
using Smart_Resume_Job_Search_App.UINav;
using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.AI;
using System.Diagnostics;

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

            var services = builder.Services;

            // App services
            services.AddSingleton<INetworkStatus, NetworkStatus>();
            services.AddSingleton<ApplicationFacade>();                    
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();
            services.AddSingleton<IViewResolver, ViewResolver>();          
            services.AddSingleton<INavigationService, ShellNavigation>(); 
            services.AddSingleton<IAppFlow, AppCoordinator>();
            services.AddSingleton<IResumeParser, ResumeParser>();

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
            Debug.WriteLine("[MauiProgram] CreateMauiApp completed");
            return app;
        }
    }
}
