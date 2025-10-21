using Foundation;

namespace Smart_Resume_Job_Search_App
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => Client.CreateMauiApp();
    }
}
