using BNF.UI.Configure;

namespace BNF
{
    namespace Audio
    {
        public class BNF_VoiceSource : BNF_AudioSource
        {
            private void Awake()
            {
                BNF_AudioConfigurator.VoiceVolumeChanged += UpdateVolume;
                BNF_AudioConfigurator.VoiceVolumeMuted += Mute;
            }
        }
    }
}