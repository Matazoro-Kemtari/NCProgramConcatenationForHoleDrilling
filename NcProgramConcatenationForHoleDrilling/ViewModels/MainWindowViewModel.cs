using Prism.Mvvm;
using System.Reflection;
using System;

namespace NcProgramConcatenationForHoleDrilling.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private static readonly Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly string? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private string _title = $"{assemblyName} {version}";
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
