namespace Smart_Resume_Job_Search_App.UINav
{
    internal class ViewResolver : IViewResolver
    {
        private readonly IServiceProvider serviceProvider;

        public ViewResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Page Resolve(object viewModel)
        {
            var vmType = viewModel.GetType();
            var viewTypeName = vmType.FullName!.Replace("ViewModel", "View");
            var viewType = vmType.Assembly.GetType(viewTypeName)
                ?? throw new InvalidOperationException($"View not found for {vmType.Name}");
            
            var view = serviceProvider.GetService(viewType) as Page
                ?? Activator.CreateInstance(viewType) as Page
                ?? throw new InvalidOperationException($"Could not create instance of {viewType.Name}");

            view.BindingContext = viewModel;
            return view;
        }
    }
}
