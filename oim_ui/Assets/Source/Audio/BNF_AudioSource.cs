using UnityEngine;

namespace BNF
{
    namespace Audio
    {
        public abstract class BNF_AudioSource : MonoBehaviour
        {
            [SerializeField]
            protected AudioSource Source;

            protected void UpdateVolume(float new_volume)
            {
                Source.volume = new_volume;
            }

            protected void Mute(bool mute_condition)
            {
                Source.mute = mute_condition;
            }
        }
    }
}