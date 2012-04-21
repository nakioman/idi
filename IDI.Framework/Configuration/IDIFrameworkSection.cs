using System.Configuration;

namespace IDI.Framework.Configuration
{
    public class IDIFrameworkSection : ConfigurationSection
    {
        [ConfigurationProperty("speechRecognition", IsRequired = true)]
        public SpeechRecognitionElement SpeechRecognitionElement
        {
            get { return (SpeechRecognitionElement)this["speechRecognition"]; }
            set { this["speechRecognition"] = value; }
        }

        [ConfigurationProperty("speechSynthesizer", IsRequired = true)]
        public SpeechSynthesizerElement SpeechSynthesizerElement
        {
            get { return (SpeechSynthesizerElement) this["speechSynthesizer"]; }
            set { this["speechSynthesizer"] = value; }
        }
    }
}