using Smart_Resume_Job_Search_App.Presentation;

namespace Smart_Resume_Job_Search_App.UI
{
    public partial class TailoredResumeView : ContentPage
    {
        public TailoredResumeView(TailoredResumeViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
