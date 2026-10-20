using System.Windows;
using Caliburn.Micro;
using ShawzinBot.Languages;
using ShawzinBot.ViewModels;

namespace ShawzinBot {
    public class Bootstrapper : BootstrapperBase {
        public Bootstrapper() {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e) {
            LanguageManager.ApplySavedOrDefault();
            DisplayRootViewFor<MainViewModel>();
        }
    }
}