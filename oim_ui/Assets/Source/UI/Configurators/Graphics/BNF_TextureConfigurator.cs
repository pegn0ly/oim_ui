using System.IO;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace BNF
{
    namespace UI
    {
        namespace Configure
        {
            // возможные разрешения текстур(взято из стандартных настроек)
            public enum TextureResolutionLevel
            {
                RES_FULL = 0,
                RES_HALF = 1,
                RES_QUAT = 2,
                RES_EIGH = 3
            }

            // класс, отвечающий за настройку параметров текстур

            // Внутренние типы:
            // - TextureProps - формат данных о параметрах текстур для сериализации.
            //
            // Поля:
            // - PossibleTextureProps - словарь возможных параметров текстур относительно уровня настроек
            //
            // Ивенты:
            // - TextureQualityChanged - вызывается при изменении уровня настроек текстур, передается новый установленный уровень

            public class BNF_TextureConfigurator : BNF_GraphicsConfiguratorBase
            {
                private struct TextureProps
                {
                    public TextureResolutionLevel Resolution;
                    public AnisotropicFiltering Filtering;
                }
                //
                private Dictionary<QualityLevel, TextureProps> PossibleTextureProps = new Dictionary<QualityLevel, TextureProps>();
                //
                private delegate void OnTextureQualityChanged(QualityLevel new_level);
                private static event OnTextureQualityChanged TextureQualityChanged;

                // Загрузить из конфига возможные параметры; подписаться на ивенты изменения настроек и языка; загрузить сохраненную конфигурацию текстур.
                private void Start()
                {
                    PossibleTextureProps = JsonConvert.DeserializeObject<Dictionary<QualityLevel, TextureProps>>(File.ReadAllText(QualityConfigurationFile));

                    TextureQualityChanged += this.UpdateQuality;
                    TextureQualityChanged += (new_level) => QualityVisualizator.SetText(BNF_Localizer.Instance.GetLocalizedString(Language, new_level.ToString()));
                    TextureQualityChanged += (new_level) => SaveQuality("Textures", new_level);

                    BNF_LanguageConfigurator.LanguageChanged += UpdateLocale;

                    LoadQuality("Textures");
                }

                public override void IncreaseQuality()
                {
                    base.IncreaseQuality();

                    TextureQualityChanged(CurrentQualityLevel.Value);
                }

                public override void DecreaseQuality()
                {
                    base.DecreaseQuality();

                    TextureQualityChanged(CurrentQualityLevel.Value);
                }

                protected override void LoadQuality(string quality_type)
                {
                    base.LoadQuality(quality_type);

                    TextureQualityChanged(CurrentQualityLevel.Value);
                }

                protected override void UpdateQuality(QualityLevel new_level)
                {
                    base.UpdateQuality(new_level);

                    TextureProps NewProps = PossibleTextureProps[new_level];

                    QualitySettings.masterTextureLimit = (int)NewProps.Resolution;
                    QualitySettings.anisotropicFiltering = NewProps.Filtering;
                }
 
                protected override void UpdateLocale(LocaleLanguage new_lng)
                {
                    base.UpdateLocale(new_lng);
                    LocalTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_lng, "textures_quality"));
                }
            }
        }
    }
}