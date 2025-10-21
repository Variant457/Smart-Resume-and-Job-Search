namespace Smart_Resume_Job_Search_App.UINav
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public TViewModel Create<TViewModel>(object? parameter = null)
        {
            if (parameter is null)
            {
                return ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider);
            }
            return ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider, parameter);
        }
    }
}
