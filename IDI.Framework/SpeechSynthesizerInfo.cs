using System.ComponentModel.Composition;
using System.Configuration;
using System.Speech.Synthesis;
using IDI.Framework.Configuration;

namespace IDI.Framework
{
    [Export]
    public class SpeechSynthesizerInfo
    {
        private readonly IDIFrameworkSection _config;
        public SpeechSynthesizer SpeechSynthesizer { get; private set; }

        public SpeechSynthesizerInfo()
        {
            _config = (IDIFrameworkSection)ConfigurationManager.GetSection("idiFrameworkSection");
            SpeechSynthesizer = new SpeechSynthesizer();
            SpeechSynthesizer.SelectVoice(_config.SpeechSynthesizerElement.Name);    
        }
    }
}