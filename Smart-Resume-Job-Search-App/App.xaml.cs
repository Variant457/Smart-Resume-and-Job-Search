using Smart_Resume_Job_Search_App.UINav;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App
{
    public partial class App : Application
    {
        private readonly IAppFlow _flow;
        private bool _started;

        public App(IAppFlow flow)
        {
            InitializeComponent();
            _flow = flow;
            Debug.WriteLine("[App] .ctor complete");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Debug.WriteLine("[App] CreateWindow start");

            var window = new Window(new AppShell());
            StartFlowOnce();

            Debug.WriteLine("[App] CreateWindow end");
            return window;
        }

        private void StartFlowOnce()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (_started) return;
                _started = true;

                try
                {
                    await _flow.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[App] flow.Start() failed: {ex}");
                }
            });
        }
    }
}
