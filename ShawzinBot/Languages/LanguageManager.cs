using System;
using System.Collections.Generic;
using System.Windows;
using ShawzinBot.Properties;

namespace ShawzinBot.Languages {
    // 语言管理器：加载与切换多语言资源字典，并提供当前语言及跨语言的资源读取
    public static class LanguageManager {
        // 支持的语言字典地址，数组索引即语言编号，依次为0中文、1英文、2日文
        private static readonly Uri[] DictUris = {
            new Uri("Languages/zh-CN.xaml", UriKind.Relative),
            new Uri("Languages/en-US.xaml", UriKind.Relative),
            new Uri("Languages/ja-JP.xaml", UriKind.Relative)
        };

        // 当前生效的语言字典，用于判断能否原地替换
        private static ResourceDictionary _currentDict;

        // 惰性加载全部语言字典，供跨语言搜索使用，单个字典加载失败时以null占位
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

        // 语言切换成功后的通知事件，界面据此刷新依赖语言的文案
        public static event EventHandler LanguageChanged;

        // 应用指定索引的语言字典，索引非法时回退默认语言，加载失败时降级到中文，并持久化生效的语言编号
        // languageIndex：语言字典索引
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
            // 末尾字典就是上次应用的语言字典时原地替换，避免合并字典不断累积
            if (merged.Count > 0 && ReferenceEquals(merged[merged.Count - 1], _currentDict))
                merged[merged.Count - 1] = dict;
            else
                merged.Add(dict);

            _currentDict = dict;
            PropertyManager.SetUpProperties("LanguageIndex", languageIndex);

            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        // 程序启动时应用用户上次保存的语言设置
        public static void ApplySavedOrDefault() {
            Apply(Settings.Default.LanguageIndex);
        }

        // 读取当前语言下指定键的文本
        // key：资源键名
        // 返回：找到的文本，找不到时返回键名本身作为兜底
        public static string GetString(string key) {
            return Application.Current?.TryFindResource(key) as string ?? key;
        }

        // 跨所有语言字典读取同一键的文本并去重，用于窗口标题这类需要匹配任意语言版本的场景
        // key：资源键名，同时会尝试读取key0、key1等带序号后缀的同族键，遇空缺即停止
        // 返回：去重后的全部文本集合
        public static string[] GetStrings(string key) {
            var values = new List<string>();
            foreach (var dict in AllDicts.Value) {
                // 先取无后缀的基础键值
                var value = dict?[key] as string;
                if (!string.IsNullOrEmpty(value) && !values.Contains(value)) values.Add(value);
                // 再逐个取带序号后缀的键
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