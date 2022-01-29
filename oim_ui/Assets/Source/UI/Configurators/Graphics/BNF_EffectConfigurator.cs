using System.IO;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace BNF.UI.Configure
{
    // класс, отвечающий за настройку параметров эффектов
    //
    // Внутренние типы:
    // - FXProps - формат данных о параметрах эффектов для сериализации
    //
    // Поля:
    // - PossibleFXProps - словарь возможных параметров эффектов относительно уровня настроек
    //
    // Ивенты:
    // - FXQualityChanged - вызывается при изменении уровня настроек эффектов, передается новый установленный уровень
    public class BNF_EffectConfigurator : BNF_GraphicsConfiguratorBase
    {
        private struct FXProps
        {
            public bool UseSoftParticles;
            public int ParticleRaycastBudget;

            public FXProps(bool use_soft_particles, int particle_raycast_budget)
            {
                UseSoftParticles = use_soft_particles;
                ParticleRaycastBudget = particle_raycast_budget;
            }
        }

        private Dictionary<QualityLevel, FXProps> PossibleFXProps = new Dictionary<QualityLevel, FXProps>();

        private delegate void OnFXQualityChanged(QualityLevel new_level);
        private static event OnFXQualityChanged FXQualityChanged;

        private void Start()
        { 
            PossibleFXProps = JsonConvert.DeserializeObject<Dictionary<QualityLevel, FXProps>>(File.ReadAllText(QualityConfigurationFile));

            LocalTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(Language, "fx_quality"));

            FXQualityChanged += this.UpdateQuality;
            FXQualityChanged += (new_level) => QualityVisualizator.SetText(BNF_Localizer.Instance.GetLocalizedString(Language, new_level.ToString()));
            FXQualityChanged += (new_level) => SaveQuality("Effects", new_level);

            BNF_LanguageConfigurator.LanguageChanged += UpdateLocale;

            LoadQuality("Effects");
        }

        public override void IncreaseQuality()
        {
            base.IncreaseQuality();

            FXQualityChanged(CurrentQualityLevel.Value);
        }

        public override void DecreaseQuality()
        {
            base.DecreaseQuality();

            FXQualityChanged(CurrentQualityLevel.Value);
        }

        protected override void LoadQuality(string quality_type)
        {
            Debug.Log("Trying to load fx...");
            base.LoadQuality(quality_type);

            FXQualityChanged(CurrentQualityLevel.Value);
        }

        protected override void UpdateQuality(QualityLevel new_level)
        {
            base.UpdateQuality(new_level);

            FXProps NewProps = PossibleFXProps[new_level];

            QualitySettings.softParticles = NewProps.UseSoftParticles;
            QualitySettings.particleRaycastBudget = NewProps.ParticleRaycastBudget;
        }

        protected override void UpdateLocale(LocaleLanguage new_lng)
        {
            Debug.Log("FX config: update locale " + Language);
            base.UpdateLocale(new_lng);
            LocalTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_lng, "fx_quality"));
        }
    }
}