namespace Smart_Resume_Job_Search_App.UINav
{
    internal class ShellNavigation : INavigationService
    {
        private readonly IViewModelFactory viewModelFactory;
        private readonly IViewResolver resolver;

        public ShellNavigation(IViewModelFactory viewModelFactory, IViewResolver resolver)
        {
            this.viewModelFactory = viewModelFactory;
            this.resolver = resolver;
        }

        public Task ReplaceRoot<TViewModel>(object? parameter = null)
        {
            var vm = viewModelFactory.Create<TViewModel>(parameter);
            if (vm == null)
                throw new InvalidOperationException($"ViewModel of type {typeof(TViewModel).Name} could not be created.");
            var page = resolver.Resolve(vm);
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                window.Page = new AppShell
                {
                    CurrentItem = new ShellContent { Content = page }
                };
            }
            return Task.CompletedTask;
        }

        public async Task Show<TViewModel>(object? parameter = null)
        {
            var vm = viewModelFactory.Create<TViewModel>(parameter);
            if (vm == null)
                throw new InvalidOperationException($"ViewModel of type {typeof(TViewModel).Name} could not be created.");
            var page = resolver.Resolve(vm);
            await Shell.Current!.Navigation.PushAsync(page);
        }

        public Task GoBack() => Shell.Current!.Navigation.PopAsync();
    }
}
