using System;
using System.Windows;
using ShawzinBot.Properties;

namespace ShawzinBot.Languages {
    public static class LanguageManager {
        private static readonly Uri[] DictUris = {
            new Uri("Languages/zh-CN.xaml", UriKind.Relative),
            new Uri("Languages/en-US.xaml", UriKind.Relative),
            new Uri("Languages/ja-JP.xaml", UriKind.Relative)
        };

        private static ResourceDictionary _currentDict;

        public static event EventHandler LanguageChanged;

        public static void Apply(int languageIndex) {
            if (languageIndex < 0 || languageIndex >= DictUris.Length)
                languageIndex = 0;

            ResourceDictionary dict;
            try {
                dict = new ResourceDictionary { Source = DictUris[languageIndex] };
            }
            catch (Exception) {
                if (languageIndex == 0) return;
                dict = new ResourceDictionary { Source = DictUris[0] };
                languageIndex = 0;
            }

            var merged = Application.Current.Resources.MergedDictionaries;
            if (merged.Count > 0 && ReferenceEquals(merged[merged.Count - 1], _currentDict))
                merged[merged.Count - 1] = dict;
            else
                merged.Add(dict);

            _currentDict = dict;
            Settings.Default.LanguageIndex = languageIndex;
            Settings.Default.Save();

            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void ApplySavedOrDefault() {
            Apply(Settings.Default.LanguageIndex);
        }

        public static string GetString(string key) {
            return Application.Current?.TryFindResource(key) as string ?? key;
        }
    }
}