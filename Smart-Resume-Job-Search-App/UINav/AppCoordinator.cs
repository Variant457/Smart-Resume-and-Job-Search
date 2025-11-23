using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.Domain;
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
        }

        public Task Start()
        {
            Debug.WriteLine("[AppCoordinator] Start invoked");
            return nav.ReplaceRoot<HomeViewModel>();
        }

        public async Task StartSearch(InputBlob file)
        {
            Debug.WriteLine("[AppCoordinator] StartSearch invoked");
            await nav.Show<LoadingViewModel>();
            try
            {
                await facade.UploadResume(file);
                await nav.ReplaceRoot<JobResultsViewModel>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppCoordinator] Error during search: {ex}");
                await nav.GoBack();
            }
        }

        public Task ShowResults()
        {
            Debug.WriteLine("[AppCoordinator] ShowResults invoked");
            return nav.ReplaceRoot<JobResultsViewModel>();
        }

        public async Task StartOptimizing()
        {
            Debug.WriteLine("[AppCoordinator] StartOptimizing invoked");
            await Task.CompletedTask; //TODO
        }

        public async Task ShowOptimized()
        {
            Debug.WriteLine("[AppCoordinator] ShowOptimized invoked");
            await Task.CompletedTask; //TODO
        }

        public Task BackToHome()
        {
            Debug.WriteLine("[AppCoordinator] BackToHome invoked");
            return nav.ReplaceRoot<HomeViewModel>();
        }

        public Task BackToResults()
        {
            Debug.WriteLine("[AppCoordinator] BackToResults invoked");
            return nav.GoBack();
        }
    }
}
