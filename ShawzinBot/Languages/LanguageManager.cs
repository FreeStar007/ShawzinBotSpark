using System;
using System.Collections.Generic;
using System.Windows;
using ShawzinBot.Properties;

namespace ShawzinBot.Languages {
    // 语言管理器：加载/切换多语言资源字典，并提供跨语言的资源读取能力
    public static class LanguageManager {
        // 支持的语言字典地址，索引顺序即语言编号（0中文、1英文、2日文）
        private static readonly Uri[] DictUris = {
            new Uri("Languages/zh-CN.xaml", UriKind.Relative),
            new Uri("Languages/en-US.xaml", UriKind.Relative),
            new Uri("Languages/ja-JP.xaml", UriKind.Relative)
        };

        // 当前生效的语言字典
        private static ResourceDictionary _currentDict;

        // 惰性加载全部语言字典，供跨语言搜索使用；单个字典加载失败时以null占位
        private static readonly Lazy<ResourceDictionary[]> AllDicts =
            new Lazy<ResourceDictionary[]>(() => {
                var result = new ResourceDictionary[DictUris.Length];
                for (var i = 0; i < DictUris.Length; i++)
                    try {
                        result[i] = new ResourceDictionary { Source = DictUris[i] };
                    } catch (Exception) {
                        result[i] = null;
                    }

                return result;
            });

        // 语言切换完成事件
        public static event EventHandler LanguageChanged;

        // 应用指定索引的语言字典；索引非法时回退到默认语言，加载失败时降级到中文
        public static void Apply(int languageIndex) {
            if (languageIndex < 0 || languageIndex >= DictUris.Length)
                languageIndex = 0;

            ResourceDictionary dict;
            try {
                dict = new ResourceDictionary { Source = DictUris[languageIndex] };
            } catch (Exception) {
                if (languageIndex == 0) return;
                dict = new ResourceDictionary { Source = DictUris[0] };
                languageIndex = 0;
            }

            var merged = Application.Current.Resources.MergedDictionaries;
            // 若上次应用的字典仍在末尾，则原地替换，避免合并字典不断累积
            if (merged.Count > 0 && ReferenceEquals(merged[merged.Count - 1], _currentDict))
                merged[merged.Count - 1] = dict;
            else
                merged.Add(dict);

            _currentDict = dict;
            Settings.Default.LanguageIndex = languageIndex;
            Settings.Default.Save();

            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        // 启动时应用用户上次保存的语言设置
        public static void ApplySavedOrDefault() {
            Apply(Settings.Default.LanguageIndex);
        }

        // 从当前生效的语言字典中取值，取不到时返回键名本身
        public static string GetString(string key) {
            return Application.Current?.TryFindResource(key) as string ?? key;
        }

        // 跨所有语言字典搜索同一键的值并去重返回；同时支持 key0、key1… 形式的带后缀键
        public static string[] GetStrings(string key) {
            var values = new List<string>();
            foreach (var dict in AllDicts.Value) {
                var value = dict?[key] as string;
                if (!string.IsNullOrEmpty(value) && !values.Contains(value)) values.Add(value);
                for (var i = 0;; i++) {
                    var suffixValue = dict?[key + i] as string;
                    if (string.IsNullOrEmpty(suffixValue)) break;
                    if (!values.Contains(suffixValue)) values.Add(suffixValue);
                }
            }

            return values.ToArray();
        }
    }
}