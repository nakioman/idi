using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Speech.Recognition.SrgsGrammar;

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
    }
}