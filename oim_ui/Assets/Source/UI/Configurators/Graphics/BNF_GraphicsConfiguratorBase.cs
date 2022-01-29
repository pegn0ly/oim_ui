using System.Collections.Generic;
using System.IO;

using UnityEngine;

using TMPro;

using Newtonsoft.Json;

namespace BNF.UI.Configure
{
    // возможные уровни настроек
    public enum QualityLevel
    {
        QUALITY_VERY_LOW = 0,
        QUALITY_LOW = 1,
        QUALITY_MEDIUM = 2,
        QUALITY_HIGH = 3,
        QUALITY_VERY_HIGH = 4,
        QUALITY_ULTRA = 5
    }

    // форма информации о сохраненном уровне настроек для сериализации
    public struct SavedQualityLevel
    {
        public QualityLevel Level;

        public SavedQualityLevel(QualityLevel new_level)
        {
            Level = new_level;
        }
    }

    // Базовый класс для всех настройщиков графики.
    // 
    // Поля:
    // - ConfigurationsNames - словарь путей к именам профилей настроек
    // - PossibleQualityConfigurations - список возможных профилей настроек для конкретного настройщика
    // - QualityConfigurationFile - путь к конкретному файлу настроек
    // - QualityVisualizator - текстовый элемент для отображения имени профиля
    // - QualityLevelsList - связный список последовательности изменения профилей
    // - CurrentQualityLevel - текущий профиль
    // - SavedQualityLevels - сохраненные профили настроек
    public abstract class BNF_GraphicsConfiguratorBase : MonoBehaviour
    {
        [SerializeField]
        protected List<QualityLevel> PossibleQualityConfigurations;

        [SerializeField]
        protected string QualityConfigurationFile;

        [SerializeField]
        protected TextMeshProUGUI MainTitle;

        [SerializeField]
        protected TextMeshProUGUI LocalTitle;

        protected LocaleLanguage Language;

        [SerializeField]
        protected TextMeshProUGUI QualityVisualizator;

        protected LinkedList<QualityLevel> QualityLevelsList = new LinkedList<QualityLevel>();

        protected LinkedListNode<QualityLevel> CurrentQualityLevel;

        protected Dictionary<string, SavedQualityLevel> SavedQualityLevels;

        // Сформировать связный список последовательности профилей; загрузить сохраненные профили.
        private void Awake()
        {
            foreach(QualityLevel level in PossibleQualityConfigurations)
            {
                QualityLevelsList.AddLast(level);
            }

            CurrentQualityLevel = QualityLevelsList.First;

            SavedQualityLevels = JsonConvert.DeserializeObject<Dictionary<string, SavedQualityLevel>>(File.ReadAllText("Assets/Config/Graphics/Saved.json"));

            BNF_LanguageConfigurator.LanguageChanged += (new_language) => Language = new_language;
            BNF_LanguageConfigurator.LanguageChanged += (new_language) => MainTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_language, "graphics_quality"));
        }

        // Переключение на следующий профиль в списке, публичный, т.к. назначается кнопке UI.
        public virtual void IncreaseQuality()
        {
            CurrentQualityLevel = CurrentQualityLevel == QualityLevelsList.Last ? QualityLevelsList.First : CurrentQualityLevel.Next;
        }

        // Переключение на предыдущий профиль в списке, публичный, т.к. назначается кнопке UI.
        public virtual void DecreaseQuality()
        {
            CurrentQualityLevel = CurrentQualityLevel == QualityLevelsList.First ? QualityLevelsList.Last : CurrentQualityLevel.Previous; 
        }

        // Загружает сохраненный профиль для конкретного типа настроек.
        protected virtual void LoadQuality(string quality_type)
        {
            SavedQualityLevel SavedLevel = SavedQualityLevels[quality_type];
            CurrentQualityLevel = QualityLevelsList.FindLast(SavedLevel.Level);
        }

        // Сохраняет текущий профиль для конкретного типа настроек.
        protected virtual void SaveQuality(string quality_type, QualityLevel level_to_save)
        {
            SavedQualityLevels[quality_type] = new SavedQualityLevel(level_to_save);
            string NewSavedLevels = JsonConvert.SerializeObject(SavedQualityLevels);
            File.WriteAllText("Assets/Config/Graphics/Saved.json", NewSavedLevels);
        }

        // Устанавливает новый профиль настроек.

        protected virtual void UpdateQuality(QualityLevel new_level)
        {
           // Debug.Log("Quality updated to " + new_level.ToString());
        }
        
        // Обновляет текст настройки при изменении локализации.
        protected virtual void UpdateLocale(LocaleLanguage new_lng)
        {
            QualityVisualizator.SetText(BNF_Localizer.Instance.GetLocalizedString(new_lng, CurrentQualityLevel.Value.ToString()));
        }
    }
}