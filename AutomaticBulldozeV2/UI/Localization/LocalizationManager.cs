using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ColossalFramework.Globalization;

namespace AutomaticBulldozeV2.UI.Localization
{
    public class LocalizationManager
    {
        private static readonly string DEFAULT_TRANSLATION_PREFIX = "lang";

        private static LocalizationManager _instance;
        private static string _language;
        private readonly string _assemblyPath;
        private static Dictionary<string, string> _translations;

        private LocalizationManager()
        {
            _assemblyPath = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.";
            _translations = new Dictionary<string, string>();

            LoadTranslations(LocaleManager.instance.language);
        }

        public delegate void LocaleChangedEventHandler(string language);

        public event LocaleChangedEventHandler eventLocaleChanged;

        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalizationManager();
                }
                return _instance;
            }
        }

        private string GetTranslatedFileName(string language)
        {
            switch (language)
            {
                case "jaex":
                    language = "ja";
                    break;
            }

            var filenameBuilder = new StringBuilder(DEFAULT_TRANSLATION_PREFIX);
            if (language != null)
            {
                filenameBuilder.Append("_");
                filenameBuilder.Append(language.Trim().ToLower());
            }
            filenameBuilder.Append(".xml");

            var translatedFilename = filenameBuilder.ToString();

            var assembly = Assembly.GetExecutingAssembly();
            if (assembly.GetManifestResourceNames().Contains(_assemblyPath + translatedFilename))
            {
                return translatedFilename;
            }

            if (language != null && !"en".Equals(language))
                Logger.LogWarning($"Translated file {translatedFilename} not found!");
            return DEFAULT_TRANSLATION_PREFIX + "_en.xml";
        }

        private void LoadTranslations(string language)
        {
            _language = language;
            _translations.Clear();
            try
            {
                var filename = _assemblyPath + GetTranslatedFileName(language);
                string xml;
                using (var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename))
                {
                    using (var sr = new StreamReader(rs))
                    {
                        xml = sr.ReadToEnd();
                    }
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                var nodes = xmlDoc.SelectNodes(@"//Language/LocaleResource");

                foreach (XmlNode node in nodes)
                {
                    var name = node.Attributes["Name"].InnerText.Trim();
                    var value = "";
                    var valueNode = node.SelectSingleNode("Value");
                    if (valueNode != null)
                        value = valueNode.InnerText;

                    _translations.Add(name, value);
                }
                Logger.LogDebug(() => $"{filename} translations loaded.");
                eventLocaleChanged?.Invoke(language);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while loading translations: {e}");
            }
        }

        public string GetString(string key)
        {
            string ret;
            try
            {
                _translations.TryGetValue(key, out ret);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error fetching the key {key} from the translation dictionary: {e}");
                return key;
            }
            if (ret == null)
                return key;
            return ret;
        }

        public void CheckAndUpdateLocales()
        {
            if (LocaleManager.instance.language != _language)
            {
                LoadTranslations(LocaleManager.instance.language);
            }
        }

        internal static int GetButtonWidth()
        {
            switch (LocaleManager.instance.language)
            {
                case null:
                case "en":
                case "de":
                default:
                    return 200;
                case "ru":
                case "pl":
                    return 220;
                case "pr":
                case "fr":
                case "es":
                    return 210;
            }
        }
    }
}
