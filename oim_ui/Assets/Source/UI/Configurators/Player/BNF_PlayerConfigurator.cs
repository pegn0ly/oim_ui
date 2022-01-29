using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Newtonsoft.Json;

namespace BNF.UI.Configure
{
    // возможные типа гендеров.
    public enum GenderType
    {
        GENDER_MALE = 0,
        GENDER_FEMALE = 1,
        GENDER_OTHER = 2
    }

    // класс, отвечающий за настройку параметров игрока.
    //
    // Внутренние типы:
    // - SavedPlayerConfiguration - форма информации о текущих параметрах игрока для сериализации
    //
    // Константы:
    // - ConfigurationPath - путь к файлу конфигурации.
    // - GenderNames - словарь путей к названиям гендеров.
    //
    // Поля:
    // - NicknameInputField - элемент интерфейса, в который пользователь может ввести никнейм.
    // - NicknameEditButton - кнопка, активирующая ввод.
    // - GenderVisualizator - элемент интерфейса, в который выводится имя выбранного гендера.
    // - CurrentPlayerConfiguration - текущее сохраненное состояние параметров игрока.
    // - GenderTypesList - связный список последовательности гендеров.
    // - CurrentGender - текущий узел списка.
    // 
    // Ивенты:
    // - GenderChanged - вызывается при изменении гендера, передается новый выбранный гендер
    public class BNF_PlayerConfigurator : MonoBehaviour
    {
        private struct SavedPlayerConfiguration
        {
            public string Nickname;
            public GenderType Gender;

            public SavedPlayerConfiguration(string nickname, GenderType gender)
            {
                Nickname = nickname;
                Gender = gender;
            }
        }

        private readonly string ConfigurationPath = "Assets/Config/Player/Saved.json";

        [SerializeField]
        private TMP_InputField InputField;

        [SerializeField]
        private Button EditButton;

        [SerializeField]
        private TextMeshProUGUI GenderVisualizator;

        [SerializeField]
        private TextMeshProUGUI GenderTitle;

        private LocaleLanguage Language;

        private SavedPlayerConfiguration CurrentPlayerConfiguration;
        
        private LinkedList<GenderType> GenderTypesList = new LinkedList<GenderType>();
        private LinkedListNode<GenderType> CurrentGender;

        private delegate void OnGenderChanged(GenderType new_gender);
        private event OnGenderChanged GenderChanged;

        private void Awake()
        {
            GenderTypesList.AddLast(GenderType.GENDER_MALE);
            GenderTypesList.AddLast(GenderType.GENDER_FEMALE);
            GenderTypesList.AddLast(GenderType.GENDER_OTHER);

            BNF_LanguageConfigurator.LanguageChanged += (new_lng) => Language = new_lng;
            BNF_LanguageConfigurator.LanguageChanged += (new_lng) => GenderTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_lng, "select_gender"));
        }

        private void Start()
        {
            BNF_LanguageConfigurator.LanguageChanged += (new_lng) => GenderVisualizator.SetText(BNF_Localizer.Instance.GetLocalizedString(Language, CurrentGender.Value.ToString()));

            InputField.enabled = false;
            EditButton.onClick.AddListener(StartNicknameEdit);
            InputField.onEndEdit.AddListener((nickname) => SaveNickname(nickname));
            InputField.onEndEdit.AddListener((nickname) => InputField.enabled = false);

            GenderChanged += (gender) => GenderVisualizator.SetText(BNF_Localizer.Instance.GetLocalizedString(Language, gender.ToString()));
            GenderChanged += (gender) => SaveGender(gender);

            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            CurrentPlayerConfiguration = JsonConvert.DeserializeObject<SavedPlayerConfiguration>(File.ReadAllText(ConfigurationPath));

            InputField.text = CurrentPlayerConfiguration.Nickname;
            CurrentGender = GenderTypesList.FindLast(CurrentPlayerConfiguration.Gender);
            GenderChanged(CurrentGender.Value);
        }

        private void StartNicknameEdit()
        {
            InputField.enabled = true;
            InputField.ActivateInputField();
        }

        private void SaveNickname(string new_nickname)
        {
            SavedPlayerConfiguration NewConfiguration = new SavedPlayerConfiguration(new_nickname, CurrentPlayerConfiguration.Gender);
            string NewSavedConfiguration = JsonConvert.SerializeObject(NewConfiguration);
            File.WriteAllText(ConfigurationPath, NewSavedConfiguration);
        }

        // Публичные методы для назначения кнопкам интерфейса.
        public void NextGender()
        {
            CurrentGender = CurrentGender == GenderTypesList.Last ? GenderTypesList.First : CurrentGender.Next;
            GenderChanged(CurrentGender.Value);
        }

        public void PreviousGender()
        {
            CurrentGender = CurrentGender == GenderTypesList.First ? GenderTypesList.Last : CurrentGender.Previous;
            GenderChanged(CurrentGender.Value);
        }

        private void SaveGender(GenderType new_gender)
        {
            SavedPlayerConfiguration NewConfiguration = new SavedPlayerConfiguration(CurrentPlayerConfiguration.Nickname, new_gender);
            string NewSavedConfiguration = JsonConvert.SerializeObject(NewConfiguration);
            File.WriteAllText(ConfigurationPath, NewSavedConfiguration);
        }
    }
}