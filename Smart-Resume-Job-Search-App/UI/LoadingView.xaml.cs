using Smart_Resume_Job_Search_App.Presentation;

namespace Smart_Resume_Job_Search_App.UI
{
    public partial class LoadingView : ContentPage
    {
        public LoadingView(LoadingViewModel vm) // DI will inject this
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
