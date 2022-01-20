using UnityEngine;

using BNF.UI.Configure;

public class Settings : MonoBehaviour
{
    [SerializeField]
    private BNF_TextureConfigurator TextureConfigurator;
    
    [SerializeField]
    private BNF_ModelConfigurator ModelConfigurator;

    [SerializeField]
    private BNF_EffectConfigurator EffectConfigurator;

    [SerializeField]
    private BNF_AudioConfigurator AudioConfigurator;

    [SerializeField]
    private BNF_PlayerConfigurator PlayerConfigurator;
}
