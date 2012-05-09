using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Speech.Recognition.SrgsGrammar;
using log4net;

namespace IDI.Framework.Plugins
{    
    [InheritedExport]
    public abstract class BasePlugin
    {
        protected ILog Log
        {
            get { return _log.Value; }
        }

        protected SpeechRecognizerInfo SpeechRecognizer
        {
            get { return _speechRecognizer.Value; }
        }

        protected SpeechSynthesizerInfo SpeechSynthesizer
        {
            get { return _speechSynthesizer.Value; }
        }

        public abstract string Id { get; }
        public abstract SrgsRule[] GetGrammarRules();
        public abstract void Execute(IDictionary<string, string> dictionary);

        [Import(typeof(SpeechSynthesizerInfo))] private Lazy<SpeechSynthesizerInfo> _speechSynthesizer;

        [Import(typeof(SpeechRecognizerInfo))] 
        private Lazy<SpeechRecognizerInfo> _speechRecognizer;

        [Import(typeof(ILog))] private Lazy<ILog> _log;
    }
}