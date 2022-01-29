using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace BNF.UI.Configure
{
    public class BNF_CommonConfigurator : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI MainHeader;

        [SerializeField]
        private Button ExitButton;

        [SerializeField]
        private Button LoginButton;

        [SerializeField]
        private Button SuccessButton;

        private void Awake()
        {
            BNF_LanguageConfigurator.LanguageChanged += (new_lng) => MainHeader.SetText(BNF_Localizer.Instance.GetLocalizedString(new_lng, "settings"));
        }
    }
}