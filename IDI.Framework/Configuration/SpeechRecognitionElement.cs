using System;
using System.Configuration;

namespace IDI.Framework.Configuration
{
    public class SpeechRecognitionElement : ConfigurationElement
    {
        [ConfigurationProperty("culture", IsRequired = true)]
        public string Culture
        {
            get { return (string) this["culture"]; }
            set { this["culture"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("executeCommandPhrase")]
        public string ExecuteCommandPhrase
        {
            get { return (string)this["executeCommandPhrase"]; }
            set { this["executeCommandPhrase"] = value; }
        }

        [ConfigurationProperty("useKinectAudioSource")]
        public bool UseKinectAudioSource
        {
            get { return (bool) this["useKinectAudioSource"]; }
            set { this["useKinectAudioSource"] = value; }
        }

        [ConfigurationProperty("minConfidence", DefaultValue = (float)0.6)]
        public float MinimunConfidence
        {
            get { return (float)this["minConfidence"]; }
            set { this["minConfidence"] = value; }
        }
    }
}