using System.Windows;
using System.Windows.Input;
using Melanchall.DryWetMidi.Devices;
using ShawzinBot.Languages;

namespace ShawzinBot.Views {
    // MainView.xaml 的逻辑交互部分：主窗口，处理拖动与语言菜单
    public partial class MainView : Window {
        public MainView() {
            InitializeComponent();
        }

        // 在窗口空白处按下左键时拖动窗口
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);

            DragMove();
        }

        // 点击语言按钮时展开语言选择菜单
        private void OnChangeLanguage(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement fe && fe.ContextMenu != null) {
                fe.ContextMenu.PlacementTarget = fe;
                fe.ContextMenu.IsOpen = true;
            }
        }

        // 选择中文
        private void OnSelectChinese(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(0);
        }

        // 选择英文
        private void OnSelectEnglish(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(1);
        }

        // 选择日文
        private void OnSelectJapanese(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(2);
        }

        // 关闭程序：先释放播放时间监听器，再关闭窗口
        private void OnCloseProgram(object sender, RoutedEventArgs e) {
            PlaybackCurrentTimeWatcher.Instance.Dispose();
            Close();
        }


        // 最小化窗口
        private void OnMinimizeProgram(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
    }
}