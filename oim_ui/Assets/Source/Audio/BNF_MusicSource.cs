using BNF.UI.Configure;

namespace BNF
{
    namespace Audio
    {
        public class BNF_MusicSource : BNF_AudioSource
        {
            private void Awake()
            {
                BNF_AudioConfigurator.MusicVolumeChanged += UpdateVolume;
                BNF_AudioConfigurator.MusicVolumeMuted += Mute;
            }
        }
    }
}