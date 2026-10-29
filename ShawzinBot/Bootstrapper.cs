using System.Windows;
using Caliburn.Micro;
using ShawzinBot.Languages;
using ShawzinBot.ViewModels;

namespace ShawzinBot {
    // Caliburn.Micro 应用启动器：负责在启动时应用语言设置并显示根视图
    public class Bootstrapper : BootstrapperBase {
        public Bootstrapper() {
            Initialize();
        }

        // 应用启动：先恢复上次选择的语言，再展示主界面
        protected override void OnStartup(object sender, StartupEventArgs e) {
            LanguageManager.ApplySavedOrDefault();
            DisplayRootViewFor<MainViewModel>();
        }
    }
}