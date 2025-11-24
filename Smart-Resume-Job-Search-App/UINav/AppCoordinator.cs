using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.Data;
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

        public async Task<TailoredResume?> TailorResume(JobPosting job)
        {
            Debug.WriteLine("[AppCoordinator] TailorResume invoked");
            await nav.Show<LoadingViewModel>();

            try
            {
                var tailored = await facade.TailorResume(job);
                return tailored;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppCoordinator] Error during resume tailoring: {ex}");
                await Application.Current.MainPage.DisplayAlert(
                    "Tailoring Failed",
                    "There is a problem tailoring your resume. Please Try Again",
                    "OK");
                return null;
            }
            finally
            {
                await nav.GoBack();
            }
        }

        public async Task StartSearchFromLatestResume()
        {
            Debug.WriteLine("[AppCoordinator] StartSearchFromLatestResume invoked");
            await nav.Show<LoadingViewModel>();

            try
            {
                await facade.SearchJobsWithLatestResume();
                await nav.ReplaceRoot<JobResultsViewModel>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppCoordinator] Error during search from latest resume: {ex}");
                await nav.GoBack();
            }

        }

        public async Task ShowTailoredResume(JobPosting job)
        {
            Debug.WriteLine("[AppCoordinator] ShowTailoredResume invoked");

            var tailored = await facade.GetLatestTailoredResume(job.Id);
            if (tailored == null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Tailored Resume",
                    "No tailored resume found for this job. Please tailor it first.",
                    "OK");
                return;
            }

            await nav.Show<TailoredResumeViewModel>((object)(Action<TailoredResumeViewModel>)(vm =>
            {
                vm.Initialize(tailored);
            }));
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
