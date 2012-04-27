using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Speech.Recognition.SrgsGrammar;
using log4net;

namespace IDI.Framework
{    
    [InheritedExport]
    public abstract class BasePlugin
    {
        public abstract string Id { get; }
        public abstract SrgsRule[] GetGrammarRules();
        public abstract void Execute(IDictionary<string, string> dictionary);

        [Import]
        protected SpeechSynthesizerInfo SpeechSynthesizer;

        [Import(typeof(ILog))]
        protected ILog Log;

        ~BasePlugin()
        {
            Log.Info("A plugin is destructing itself");
        }
    }
}