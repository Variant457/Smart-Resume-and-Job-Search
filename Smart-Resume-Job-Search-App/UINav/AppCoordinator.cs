using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.Presentation;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.UINav
{
    public class AppCoordinator : IAppFlow
    {
        private readonly INavigationService nav;
        private readonly IViewModelFactory vmFactory;
        private readonly ApplicationFacade facade;

        public AppCoordinator(INavigationService nav, IViewModelFactory vmFactory, ApplicationFacade facade)
        {
            this.nav = nav;
            this.vmFactory = vmFactory;
            this.facade = facade;
            Debug.WriteLine("[AppFlow] .ctor");
        }

        public Task Start()
        {
            Debug.WriteLine("[AppFlow] Start invoked");
            return nav.ReplaceRoot<HomeViewModel>();
        }

        public async Task StartSearch()
        {
            Debug.WriteLine("[AppFlow] StartSearch invoked");
            await Task.CompletedTask; //TODO
        }

        public async Task ShowResults()
        {
            Debug.WriteLine("[AppFlow] ShowResults invoked");
            await Task.CompletedTask; //TODO
        }

        public async Task StartOptimizing()
        {
            Debug.WriteLine("[AppFlow] StartOptimizing invoked");
            await Task.CompletedTask; //TODO
        }

        public async Task ShowOptimized()
        {
            Debug.WriteLine("[AppFlow] ShowOptimized invoked");
            await Task.CompletedTask; //TODO
        }

        public Task BackToHome()
        {
            Debug.WriteLine("[AppFlow] BackToHome invoked");
            return nav.ReplaceRoot<HomeViewModel>();
        }

        public Task BackToResults()
        {
            Debug.WriteLine("[AppFlow] BackToResults invoked");
            return nav.GoBack();
        }
    }
}
