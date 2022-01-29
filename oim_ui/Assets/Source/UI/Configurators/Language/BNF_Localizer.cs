using System.Collections.Generic;
using System.IO;

using UnityEngine;

using Newtonsoft.Json;

namespace BNF.UI.Configure
{
    public struct LocaleProps
    {
        public LocaleLanguage Id;
        public string Code;
        public string Title;
        public Dictionary<string, string> Values;
    }

    public class BNF_Localizer : MonoBehaviour
    {
        public static BNF_Localizer Instance{ get; private set;}

        [SerializeField]
        private List<string> LocaleVariants;

        private Dictionary<LocaleLanguage, LocaleProps> LoadedLocaleProps = new Dictionary<LocaleLanguage, LocaleProps>();

        private void Awake()
        {
            if(!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }

            foreach(string path in LocaleVariants)
            {
                LocaleProps NewProps = JsonConvert.DeserializeObject<LocaleProps>(File.ReadAllText(path));
                LoadedLocaleProps[NewProps.Id] = NewProps;
            }
        }

        public string GetLocalizedString(LocaleLanguage language, string index)
        {
            return LoadedLocaleProps[language].Values[index];
        }
    }
}