using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;

namespace BNF.UI.Configure
{
    // возможные языки.
    public enum LocaleLanguage
    {
        LNG_RUSSIAN = 0,
        LNG_ENGLISH = 1,
        LNG_CHINESE = 2
    }

    // класс отвечающий за настройку параметров языка(впоследствии, вероятно, и локализации в целом)
    //
    // Внутренние типы:
    // - SavedLanguageConfiguration - форма данных для сериализации информации о языке.
    // 
    // Константы:
    // - ConfigurationPath - путь к файлу конфигурации языка.
    //
    // Поля:
    // - RuToggle, EnToggle, ChnToggle - переключатели для конкретного языка.
    // - SelectedToggle - текущий активный переключатель.
    // - SubtitleSwitcher - переключатель субтитров.
    // - LanguageVariants - словарь соответствия переключателя языку.
    // - CurrentLanguageConfiguration - текущая сохраненная конфигурация языка и субтитров.
    // 
    // Ивенты:
    // - LanguageChanged - вызывается при изменении языка, передается новый установленный язык.
    public class BNF_LanguageConfigurator : MonoBehaviour
    {
        private struct SavedLanguageConfiguration
        {
            public LocaleLanguage Language;

            public bool SubltitlesEnabled;

            public SavedLanguageConfiguration(LocaleLanguage lng, bool sbt)
            {
                Language = lng;
                SubltitlesEnabled = sbt;
            }
        }

        private readonly string ConfigurationPath = "Assets/Config/Language/Saved.json";

        [SerializeField]
        private Toggle RuToggle;

        [SerializeField]
        private Toggle EnToggle;

        [SerializeField]
        private Toggle ChnToggle;

        private Toggle SelectedToggle;

        [SerializeField]
        private Toggle SubtitlesSwitcher;

        private Dictionary<LocaleLanguage, Toggle> LanguageVariants = new Dictionary<LocaleLanguage, Toggle>();

        private SavedLanguageConfiguration CurrentLanguageConfiguration;

        public delegate void OnLanguageChanged(LocaleLanguage new_language);
        public static event OnLanguageChanged LanguageChanged;

        private void Start()
        {   
            LanguageVariants.Add(LocaleLanguage.LNG_RUSSIAN, RuToggle);
            LanguageVariants.Add(LocaleLanguage.LNG_ENGLISH, EnToggle);
            LanguageVariants.Add(LocaleLanguage.LNG_CHINESE, ChnToggle);

            foreach(KeyValuePair<LocaleLanguage, Toggle> kvp in LanguageVariants)
            {
                kvp.Value.onValueChanged.AddListener((enabled) => SwitchLanguageToggle(kvp.Value, kvp.Key));
            }

            LanguageChanged += (new_language) => SaveLanguage(new_language);

            SubtitlesSwitcher.onValueChanged.AddListener((enabled) => SaveSubtitlesCondition(enabled));

            LoadLanguageConfiguration();
        }

        private void LoadLanguageConfiguration()
        {
            CurrentLanguageConfiguration = JsonConvert.DeserializeObject<SavedLanguageConfiguration>(File.ReadAllText(ConfigurationPath));

            SubtitlesSwitcher.isOn = CurrentLanguageConfiguration.SubltitlesEnabled;
            LanguageVariants[CurrentLanguageConfiguration.Language].isOn = true;

            SwitchLanguageToggle(LanguageVariants[CurrentLanguageConfiguration.Language], CurrentLanguageConfiguration.Language);
        }

        private void SwitchLanguageToggle(Toggle toggle, LocaleLanguage new_language)
        {
            if(SelectedToggle != null)
            {
                SelectedToggle.enabled = true;
            }

            SelectedToggle = toggle;
            //SelectedToggle.enabled = false;

            LanguageChanged(new_language);
        }

        private void SaveLanguage(LocaleLanguage new_language)
        {
            SavedLanguageConfiguration NewConfiguration = new SavedLanguageConfiguration(new_language, CurrentLanguageConfiguration.SubltitlesEnabled);
            string NewSavedLanguage = JsonConvert.SerializeObject(NewConfiguration);
            File.WriteAllText(ConfigurationPath, NewSavedLanguage);
        }

        private void SaveSubtitlesCondition(bool new_condition)
        {
            SavedLanguageConfiguration NewConfiguration = new SavedLanguageConfiguration(CurrentLanguageConfiguration.Language, new_condition);
            string NewSavedLanguage = JsonConvert.SerializeObject(NewConfiguration);
            File.WriteAllText(ConfigurationPath, NewSavedLanguage);
        }
    }
}