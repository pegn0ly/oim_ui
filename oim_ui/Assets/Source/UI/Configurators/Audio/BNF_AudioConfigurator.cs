using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

using TMPro;

using Newtonsoft.Json;

namespace BNF.UI.Configure
{
    // форма информации о текущих параметров звука для сериализации.
    public struct SavedVolumeConfiguration
    {
        public float Volume {get; set;}
        public bool IsMuted {get; set;}

        public SavedVolumeConfiguration(float new_volume, bool is_muted)
        {
            Volume = new_volume;
            IsMuted = is_muted;
        }
    }

    // класс, управляющий настройками звука.
    //
    // Константы:
    // - ConfigurationPath - путь к файлу с сохраненными параметрами звука.
    // - VOLUME_TYPE_MASTER, VOLUME_TYPE_MUSIC, VOLUME_TYPE_VOICE, VOLUME_TYPE_FX - строки, по которым в файле определяются параметры конкретных типов звука.
    //
    // Поля:
    // - SavedVolumes
    // - MasterVolumeSlider, MusicVolumeSlider, VoiceVolumeSlider, FXVolumeSlider - элементы интерфейса, отображающие уровни громкости.
    // - MasterVolumeMuter, MusicVolumeMuter, VoiceVolumeMuter, FXVolumeMuter - элементы интерфейса, отвечающие за включение/выключение конкретных типов звука.
    //
    // Ивенты:
    // - MasterVolumeChanged, MusicVolumeChanged, VoiceVolumeChanged, FXVolumeChanged - вызываются при изменении уровня громкости звука, передается новый уровень громкости.
    // - MasterVolumeMuted, MusicVolumeMuted, VoiceVolumeMuted, FXVolumeMuted - вызываются при включении/выключении типа звука, передается новое состояние этого типа.
    public class BNF_AudioConfigurator : MonoBehaviour
    {
        private readonly string ConfigurationPath = "Assets/Config/Audio/Saved.json";

        private readonly string VOLUME_TYPE_MASTER = "Master";

        private readonly string VOLUME_TYPE_MUSIC = "Music";

        private readonly string VOLUME_TYPE_VOICE = "Voice";

        private readonly string VOLUME_TYPE_FX = "Effects";

        private readonly float LOWEST_VOLUME = -50f;

        private readonly float HIGHEST_VOLUME = 15f;

        private Dictionary<string, SavedVolumeConfiguration> SavedVolumes;

        [SerializeField]
        private Slider MasterVolumeSlider;

        [SerializeField]
        private Slider MusicVolumeSlider;

        [SerializeField]
        private Slider VoiceVolumeSlider;

        [SerializeField]
        private Slider FXVolumeSlider;

        [SerializeField]
        private Toggle MasterVolumeMuter;

        [SerializeField]
        private Toggle MusicVolumeMuter;

        [SerializeField]
        private Toggle VoiceVolumeMuter;

        [SerializeField]
        private Toggle FXVolumeMuter;

        [SerializeField]
        private TextMeshProUGUI MainTitle;

        [SerializeField]
        private TextMeshProUGUI MasterTitle;

        [SerializeField]
        private TextMeshProUGUI MusicTitle;

        [SerializeField]
        private TextMeshProUGUI VoiceTitle;

        [SerializeField]
        private TextMeshProUGUI FXTitle;

        [SerializeField]
        private AudioMixer Mixer;

        public delegate void OnMasterVolumeChanged(float volume);
        public static event OnMasterVolumeChanged MasterVolumeChanged;

        public delegate void OnMusicVolumeChanged(float volume);
        public static event OnMusicVolumeChanged MusicVolumeChanged;

        public delegate void OnVoiceVolumeChanged(float volume);
        public static event OnVoiceVolumeChanged VoiceVolumeChanged;
    
        public delegate void OnFXVolumeChanged(float volume);
        public static event OnFXVolumeChanged FXVolumeChanged;

        public delegate void OnMasterVolumeMuted(bool mute);
        public static event OnMasterVolumeMuted MasterVolumeMuted;

        public delegate void OnMusicVolumeMuted(bool mute);
        public static event OnMusicVolumeMuted MusicVolumeMuted;

        public delegate void OnVoiceVolumeMuted(bool mute);
        public static event OnVoiceVolumeMuted VoiceVolumeMuted;

        public delegate void OnFXVolumeMuted(bool mute);
        public static event OnFXVolumeMuted FXVolumeMuted;

        // Назначить ивенты сохранения уровней звука в конфиг; загрузить сохраненные уровни.
        private void Start()
        {
            MasterVolumeChanged += (volume) => AudioListener.volume = volume;
            MasterVolumeMuted += (mute) => AudioListener.volume = mute == true ? 0 : SavedVolumes[VOLUME_TYPE_MASTER].Volume;

            MasterVolumeChanged += (volume) => SaveVolume(VOLUME_TYPE_MASTER, volume);
            MasterVolumeMuted += (mute) => SaveMuteCondition(VOLUME_TYPE_MASTER, mute);

            MusicVolumeChanged += (volume) => SaveVolume(VOLUME_TYPE_MUSIC, volume);
            MusicVolumeChanged += (volume) => Mixer.SetFloat("MusicVolume", SavedVolumes[VOLUME_TYPE_MUSIC].IsMuted ? LOWEST_VOLUME : volume);
            MusicVolumeMuted += (mute) => SaveMuteCondition(VOLUME_TYPE_MUSIC, mute);
            MusicVolumeMuted += (mute) => Mixer.SetFloat("MusicVolume", mute == true ? LOWEST_VOLUME : SavedVolumes[VOLUME_TYPE_MUSIC].Volume);

            VoiceVolumeChanged += (volume) => SaveVolume(VOLUME_TYPE_VOICE, volume);
            VoiceVolumeChanged += (volume) => Mixer.SetFloat("VoiceVolume", SavedVolumes[VOLUME_TYPE_VOICE].IsMuted ? LOWEST_VOLUME : volume);
            VoiceVolumeMuted += (mute) => SaveMuteCondition(VOLUME_TYPE_VOICE, mute);
            VoiceVolumeMuted += (mute) => Mixer.SetFloat("VoiceVolume", mute == true ? LOWEST_VOLUME : SavedVolumes[VOLUME_TYPE_VOICE].Volume);

            FXVolumeChanged += (volume) => SaveVolume(VOLUME_TYPE_FX, volume);
            FXVolumeChanged += (volume) => Mixer.SetFloat("FXVolume", SavedVolumes[VOLUME_TYPE_FX].IsMuted ? LOWEST_VOLUME : volume);
            FXVolumeMuted += (mute) => SaveMuteCondition(VOLUME_TYPE_FX, mute);
            FXVolumeMuted += (mute) => Mixer.SetFloat("FXVolume", mute == true ? LOWEST_VOLUME : SavedVolumes[VOLUME_TYPE_FX].Volume);

            BNF_LanguageConfigurator.LanguageChanged += UpdateLocale;

            LoadVolumes();
        }

        // Установить значения элементов UI в соответствии с сохраненной информацией; вызвать ивенты, чтобы передать загруженные значения подписчикам.
        private void LoadVolumes()
        {
            SavedVolumes = JsonConvert.DeserializeObject<Dictionary<string, SavedVolumeConfiguration>>(File.ReadAllText(ConfigurationPath));

            MasterVolumeSlider.value = SavedVolumes[VOLUME_TYPE_MASTER].Volume;
            MusicVolumeSlider.value = SavedVolumes[VOLUME_TYPE_MUSIC].Volume;
            VoiceVolumeSlider.value = SavedVolumes[VOLUME_TYPE_VOICE].Volume;
            FXVolumeSlider.value = SavedVolumes[VOLUME_TYPE_FX].Volume;

            MasterVolumeChanged(SavedVolumes[VOLUME_TYPE_MASTER].Volume);
            MusicVolumeChanged(SavedVolumes[VOLUME_TYPE_MUSIC].Volume);
            VoiceVolumeChanged(SavedVolumes[VOLUME_TYPE_VOICE].Volume);
            FXVolumeChanged(SavedVolumes[VOLUME_TYPE_FX].Volume);

            MasterVolumeMuter.isOn = SavedVolumes[VOLUME_TYPE_MASTER].IsMuted;
            MusicVolumeMuter.isOn = SavedVolumes[VOLUME_TYPE_MUSIC].IsMuted;
            VoiceVolumeMuter.isOn = SavedVolumes[VOLUME_TYPE_VOICE].IsMuted;
            FXVolumeMuter.isOn = SavedVolumes[VOLUME_TYPE_FX].IsMuted;

            MasterVolumeMuted(SavedVolumes[VOLUME_TYPE_MASTER].IsMuted);
            MusicVolumeMuted(SavedVolumes[VOLUME_TYPE_MUSIC].IsMuted);
            VoiceVolumeMuted(SavedVolumes[VOLUME_TYPE_VOICE].IsMuted);
            FXVolumeMuted(SavedVolumes[VOLUME_TYPE_FX].IsMuted);
        }

        private void SaveVolume(string volume_type, float value)
        {
            SavedVolumes[volume_type] = new SavedVolumeConfiguration(value, SavedVolumes[volume_type].IsMuted);
            string NewSavedVolume = JsonConvert.SerializeObject(SavedVolumes);
            File.WriteAllText(ConfigurationPath, NewSavedVolume);
        }

        private void SaveMuteCondition(string volume_type, bool new_condition)
        {
            SavedVolumes[volume_type] = new SavedVolumeConfiguration(SavedVolumes[volume_type].Volume, new_condition);
            string NewSavedVolume = JsonConvert.SerializeObject(SavedVolumes);
            File.WriteAllText(ConfigurationPath, NewSavedVolume);
        }

        private void UpdateLocale(LocaleLanguage new_language)
        {
            MainTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_language, "sound"));
            MasterTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_language, "master_volume"));
            MusicTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_language, "music_volume"));
            VoiceTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_language, "voice_volume"));
            FXTitle.SetText(BNF_Localizer.Instance.GetLocalizedString(new_language, "fx_volume"));
        }

        // Ниже все публичные методы для назначения элементам интерфейса.
        public void UpdateMasterVolume(float new_volume)
        {
            Debug.Log("Master volume changed to " + new_volume);
            MasterVolumeChanged(new_volume);
        }

        public void UpdateMusicVolume(float new_volume)
        {
            Debug.Log("Music volume changed to " + new_volume);
            MusicVolumeChanged(new_volume);
        }

        public void UpdateVoiceVolume(float new_volume)
        {
            Debug.Log("Voice volume changed to " + new_volume);
            VoiceVolumeChanged(new_volume);
        }

        public void UpdateFXVolume(float new_volume)
        {
            Debug.Log("FX volume changed to " + new_volume);
            FXVolumeChanged(new_volume);
        }

        //
        public void MuteMaster(bool new_mute_condition)
        {
            MasterVolumeMuted(new_mute_condition);
        }

        public void MuteMusic(bool new_mute_condition)
        {
            MusicVolumeMuted(new_mute_condition);
        }

        public void MuteVoice(bool new_mute_condition)
        {
            VoiceVolumeMuted(new_mute_condition);
        }

        public void MuteFX(bool new_mute_condition)
        {
            FXVolumeMuted(new_mute_condition);
        }
    }
}