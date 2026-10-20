using System.Windows;
using System.Windows.Input;
using Melanchall.DryWetMidi.Devices;
using ShawzinBot.Languages;

namespace ShawzinBot.Views {
    /// <summary>
    ///     Lógica de interacción para MainView.xaml
    /// </summary>
    public partial class MainView : Window {
        public MainView() {
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void OnChangeLanguage(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement fe && fe.ContextMenu != null) {
                fe.ContextMenu.PlacementTarget = fe;
                fe.ContextMenu.IsOpen = true;
            }
        }

        private void OnSelectChinese(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(0);
        }

        private void OnSelectEnglish(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(1);
        }

        private void OnSelectJapanese(object sender, RoutedEventArgs e) {
            LanguageManager.Apply(2);
        }

        private void OnCloseProgram(object sender, RoutedEventArgs e) {
            PlaybackCurrentTimeWatcher.Instance.Dispose();
            Close();
        }


        private void OnMinimizeProgram(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
    }
}