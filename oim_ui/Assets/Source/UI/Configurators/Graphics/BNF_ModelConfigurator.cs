using System.IO;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace BNF.UI.Configure
{
    // возможные уровни сглаживания
    public enum AntiAliasingLevel
    {
        NO_MSAA = 0,
        MSAA_2X = 2,
        MSAA_4X = 4,
        MSAA_8X = 8
    }

    // класс, отвечающий за настройку параметров моделей
    //
    // Внутренние типы:
    // - ModelsProps - формат данных о параметрах моделей для сериализации
    //
    // Поля:
    // - PossibleModelsProps - словарь возможных параметров моделей относительно уровня настроек
    //
    // Ивенты:
    // - ModelsQualityChanged - вызывается при изменении уровня настроек моделей, передается новый установленный уровень
    public class BNF_ModelConfigurator : BNF_GraphicsConfiguratorBase
    {
        private struct ModelsProps
        {
            public AntiAliasingLevel MSAA_Level;
            public float LODBias;
            public int MaxLODLevel;

            public ModelsProps(AntiAliasingLevel new_level, float new_bias, int new_lod)
            {
                MSAA_Level = new_level;
                LODBias = new_bias;
                MaxLODLevel = new_lod;
            }
        }

        private Dictionary<QualityLevel, ModelsProps> PossibleModelsProps = new Dictionary<QualityLevel, ModelsProps>();

        private delegate void OnModelsQualityChanged(QualityLevel new_level);
        private static event OnModelsQualityChanged ModelsQualityChanged;

        private void Start()
        {
            PossibleModelsProps = JsonConvert.DeserializeObject<Dictionary<QualityLevel, ModelsProps>>(File.ReadAllText(QualityConfigurationFile));

            ModelsQualityChanged += this.UpdateQuality;
            ModelsQualityChanged += (new_level) => QualityVisualizator.SetText(File.ReadAllText("Assets/Texts/Ru/" + ConfigurationsNames[new_level]));
            ModelsQualityChanged += (new_level) => SaveQuality("Models", new_level);

            LoadQuality("Models");
        }

        public override void IncreaseQuality()
        {
            base.IncreaseQuality();

            ModelsQualityChanged(CurrentQualityLevel.Value);
        }

        public override void DecreaseQuality()
        {
            base.DecreaseQuality();

            ModelsQualityChanged(CurrentQualityLevel.Value);
        }

        protected override void LoadQuality(string quality_type)
        {
            base.LoadQuality(quality_type);

            ModelsQualityChanged(CurrentQualityLevel.Value);
        }

        protected override void UpdateQuality(QualityLevel new_level)
        {
            base.UpdateQuality(new_level);

            ModelsProps NewProps = PossibleModelsProps[new_level];

            QualitySettings.antiAliasing = (int)NewProps.MSAA_Level;
            QualitySettings.lodBias = NewProps.LODBias;
            QualitySettings.maximumLODLevel = NewProps.MaxLODLevel;

        }
    }
}