
using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.Presentation;

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
        }

        public Task Start() => nav.ReplaceRoot<HomeViewModel>();

        public async Task StartSearch()
        {
            ; //TODO: Implement job search flow
        }

        public async Task ShowResults()
        {
            ; //TODO: Implement show results flow
        }

        public async Task StartOptimizing()
        {
            ; //TODO: Implement select job flow
        }

        public async Task ShowOptimized()
        {
            ; //TODO: Implement show optimized resume flow
        }

        public Task BackToHome()
        {
            return nav.ReplaceRoot<HomeViewModel>();
        }

        public Task BackToResults()
        {
            return nav.GoBack();
        }
    }
}
