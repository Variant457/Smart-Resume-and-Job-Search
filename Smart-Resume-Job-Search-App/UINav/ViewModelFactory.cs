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
            System.Diagnostics.Debug.WriteLine($"[ViewModelFactory] Create<{typeof(TViewModel).Name}> param?={(parameter!=null)}");
            
            if(parameter is Action<TViewModel> init)
            {
                var instance = ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider);
                System.Diagnostics.Debug.WriteLine($"[ViewModelFactory] Created {instance?.GetType().Name} (with init callback)");
                init(instance);
                return instance;
            }
            
            if (parameter is null)
            {
                var instance = ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider);
                System.Diagnostics.Debug.WriteLine($"[ViewModelFactory] Created {instance?.GetType().Name}");
                return instance;
            }

            var instanceWithParam = ActivatorUtilities.CreateInstance<TViewModel>(serviceProvider, parameter);
            System.Diagnostics.Debug.WriteLine($"[ViewModelFactory] Created with parameter {instanceWithParam?.GetType().Name}");
            return instanceWithParam;
        }
    }
}
