using Microsoft.Extensions.Logging;
using Smart_Resume_Job_Search_App.Infrastructure;
using Smart_Resume_Job_Search_App.Presentation;
using Smart_Resume_Job_Search_App.UINav;

namespace Smart_Resume_Job_Search_App
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

            var services = builder.Services;
            services.AddSingleton<INetworkStatus, NetworkStatus>();
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();

            services.AddTransient<HomeViewModel>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
