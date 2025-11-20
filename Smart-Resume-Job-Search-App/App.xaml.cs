using Smart_Resume_Job_Search_App.UINav;
using Smart_Resume_Job_Search_App.UI;
using Smart_Resume_Job_Search_App.Presentation;
using Smart_Resume_Job_Search_App.Models;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

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

            //  TEST HARNESS: show JobResultsView instead of the normal shell
            var testPage = BuildTestPage();
            var window = new Window(testPage);
            Application.Current!.MainPage = testPage;

            Debug.WriteLine("[App] CreateWindow end");
            return window;
        }

        private Page BuildTestPage()
        {
            var services = Current!.Handler!.MauiContext!.Services;

            var vm = services.GetRequiredService<JobResultsViewModel>();
            var view = new JobResultsView(vm);

            // Seed a few fake jobs so the UI has data
            view.Initialize(new[]
            {
                new JobPosting
                {
                    Title = "Senior Engineer",
                    Company = "Contoso",
                    Location = "Seattle, WA",
                    MinSalary = 120000,
                    MaxSalary = 150000,
                    Url = "https://example.com/job/1"
                },
                new JobPosting
                {
                    Title = "Mid-level Developer",
                    Company = "Fabrikam",
                    Location = "Remote",
                    MinSalary = 90000,
                    MaxSalary = 110000,
                    Url = "https://example.com/job/2"
                }
            });

            return view;
        }

        // Keep this around for when you want to go back to the normal app flow.
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
