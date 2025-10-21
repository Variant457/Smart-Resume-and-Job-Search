using Android.App;
using Android.Runtime;

namespace Smart_Resume_Job_Search_App
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => Client.CreateMauiApp();
    }
}
