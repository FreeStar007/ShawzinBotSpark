using System.Windows;
using System.Windows.Input;
using Melanchall.DryWetMidi.Devices;
using ShawzinBot.Languages;

namespace ShawzinBot.Views {
    // MainView.xaml的逻辑交互部分，处理窗口拖动与语言选择菜单
    public partial class MainView : Window {
        // 初始化主窗口界面组件
        public MainView() {
            InitializeComponent();
        }

        // 在窗口空白处按下左键时拖动窗口
        // e：鼠标左键按下事件参数，本方法未使用
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);

            DragMove();
        }

        // 点击语言按钮时展开其挂接的语言选择菜单
        // sender：触发事件的语言按钮；e：路由事件参数，本方法未使用
        private void OnChangeLanguage(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement fe && fe.ContextMenu != null) {
                fe.ContextMenu.PlacementTarget = fe;
                fe.ContextMenu.IsOpen = true;
            }
        }

        // 切换到中文界面
        // sender：菜单项，本方法未使用；e：路由事件参数，本方法未使用
        private void OnSelectChinese(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(0);
        }

        // 切换到英文界面
        // sender：菜单项，本方法未使用；e：路由事件参数，本方法未使用
        private void OnSelectEnglish(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(1);
        }

        // 切换到日文界面
        // sender：菜单项，本方法未使用；e：路由事件参数，本方法未使用
        private void OnSelectJapanese(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(2);
        }

        // 关闭程序，先释放播放时间监听器再关闭窗口
        // sender：关闭按钮，本方法未使用；e：路由事件参数，本方法未使用
        private void OnCloseProgram(object sender, RoutedEventArgs e) {
            PlaybackCurrentTimeWatcher.Instance.Dispose();
            Close();
        }

        // 最小化窗口
        // sender：最小化按钮，本方法未使用；e：路由事件参数，本方法未使用
        private void OnMinimizeProgram(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
    }
}