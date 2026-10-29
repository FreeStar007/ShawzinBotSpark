using System.Windows;
using Caliburn.Micro;
using ShawzinBot.Languages;
using ShawzinBot.ViewModels;

namespace ShawzinBot {
    // 应用启动器：接管Caliburn.Micro的启动流程，负责恢复语言并显示根视图
    public class Bootstrapper : BootstrapperBase {
        // 构造时立即执行框架初始化
        public Bootstrapper() {
            Initialize();
        }

        // 应用启动入口：先恢复上次选择的语言，再展示主界面
        // sender：应用实例，本方法未使用；e：启动参数，本方法未使用
        protected override void OnStartup(object sender, StartupEventArgs e) {
            LanguageManager.ApplySavedOrDefault();
            DisplayRootViewFor<MainViewModel>();
        }
    }
}