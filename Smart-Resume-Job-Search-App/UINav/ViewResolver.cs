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
            System.Diagnostics.Debug.WriteLine($"[ViewResolver] Resolve for VM: {vmType.FullName}");

            var viewTypeName = vmType.FullName!
                .Replace("ViewModel", "View")
                .Replace(".Presentation.", ".UI.");
            System.Diagnostics.Debug.WriteLine($"[ViewResolver] Looking for view type: {viewTypeName}");

            var viewType = vmType.Assembly.GetType(viewTypeName)
                ?? vmType.Assembly
                    .GetTypes()
                    .FirstOrDefault(t => typeof(Page).IsAssignableFrom(t) &&
                                         t.Name == vmType.Name.Replace("ViewModel", "View"))
                ?? throw new InvalidOperationException($"View not found for {vmType.Name}");
            System.Diagnostics.Debug.WriteLine($"[ViewResolver] View type resolved: {viewType.FullName}");

            var view = serviceProvider.GetService(viewType) as Page
                ?? Activator.CreateInstance(viewType) as Page
                ?? throw new InvalidOperationException($"Could not create instance of {viewType.Name}");
            System.Diagnostics.Debug.WriteLine($"[ViewResolver] View instance created: {view?.GetType().FullName}");

            // Important: set BindingContext before InitializeComponent has been called,
            // but since ContentPage's InitializeComponent is in the constructor, this is fine.
            view.BindingContext = viewModel;
            System.Diagnostics.Debug.WriteLine("[ViewResolver] BindingContext assigned");

            // Extra safety: ensure the view is a ContentPage and not null content
            if (view is ContentPage cp && cp.Content is null)
            {
                System.Diagnostics.Debug.WriteLine("[ViewResolver] View Content was null - setting fallback content");
                // Create a minimal fallback UI to catch blank page issues
                cp.Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"Loaded {viewType.Name}", HorizontalOptions = LayoutOptions.Center, Margin =20 }
                    }
                };
            }

            return view;
        }
    }
}
