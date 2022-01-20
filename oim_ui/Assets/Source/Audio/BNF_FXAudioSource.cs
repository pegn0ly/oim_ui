using BNF.UI.Configure;

namespace BNF
{
    namespace Audio
    {
        public class BNF_FXAudioSource : BNF_AudioSource
        {
            private void Awake()
            {
                BNF_AudioConfigurator.FXVolumeChanged += UpdateVolume;
            }
        }
    }
}