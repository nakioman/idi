using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Speech.Synthesis;
using IDI.Framework.Configuration;
using IDI.Framework.Exceptions;
using log4net;

namespace IDI.Framework
{
    [Export]
    public class SpeechSynthesizerInfo
    {
        private readonly IDIFrameworkSection _config;
        public SpeechSynthesizer SpeechSynthesizer { get; private set; }
        [Import]
        private ILog _log;

        public SpeechSynthesizerInfo()
        {
            _config = (IDIFrameworkSection)ConfigurationManager.GetSection("idiFrameworkSection");
            SpeechSynthesizer = new SpeechSynthesizer();
            try
            {
                SpeechSynthesizer.SelectVoice(_config.SpeechSynthesizerElement.Name);    
            }
            catch (Exception ex)
            {
                
                throw new IDISynthetizerVoiceException("There seems to be a problem selecting the voice, " +
                                                       "are you sure the voice exists?", ex);
            }
        }
    }
}