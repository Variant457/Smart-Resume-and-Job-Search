using Smart_Resume_Job_Search_App.UINav;
using Smart_Resume_Job_Search_App.UI;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;

namespace Smart_Resume_Job_Search_App
{
    public partial class App : Application
    {
        private readonly IAppFlow flow;
        private bool started;

        public App(IAppFlow flow)
        {
            InitializeComponent();
            this.flow = flow;
            Debug.WriteLine("[App] .ctor complete");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Debug.WriteLine("[App] CreateWindow start");
            // Create a single AppShell instance and ensure Application.MainPage references it
            var shell = new AppShell();
            var window = new Window(shell);

            // Important: keep Application.Current.MainPage in sync with the visible Window's Page
            // so navigation services operate on the actual shell instance being displayed.
            Application.Current!.MainPage = shell;

            Debug.WriteLine("[App] Scheduling flow.Start() on MainThread");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (started) { Debug.WriteLine("[App] Start skipped (already started)"); return; }
                started = true;

                try
                {
                    Debug.WriteLine("[App] Calling flow.Start()");
                    await flow.Start();
                    Debug.WriteLine("[App] flow.Start() completed");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[App] flow.Start() failed: {ex}");
                }
            });

            Debug.WriteLine("[App] CreateWindow end");
            return window;
        }
    }
}