using Prism.Mvvm;

namespace NCProgramConcatenationForHoleDrilling.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "穴加工用結合ソフト";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
