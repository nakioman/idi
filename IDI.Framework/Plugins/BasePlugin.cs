using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
        protected abstract SrgsRule[] GetGrammarRules();
        public abstract void Execute(IDictionary<string, string> dictionary);

        [Import(typeof(SpeechSynthesizerInfo))] private Lazy<SpeechSynthesizerInfo> _speechSynthesizer;

        [Import(typeof(SpeechRecognizerInfo))] 
        private Lazy<SpeechRecognizerInfo> _speechRecognizer;

        [Import(typeof(ILog))] private Lazy<ILog> _log;

        public void AddGrammarRules(SrgsRulesCollection rules, SrgsOneOf choices)
        {
            var grammarRules = GetGrammarRules();

            if (grammarRules.Length > 0)
            {
                rules.Add(GetGrammarRules());

                var pluginRuleItem = new SrgsItem();
                var refCurrentPluginRule = new SrgsRuleRef(rules.Single(x => x.Id == Id));
                var outPluginRuleItemSemantic = String.Format("out.type=\"{0}\"; out.params=rules.{0};", Id);

                pluginRuleItem.Add(refCurrentPluginRule);
                pluginRuleItem.Add(new SrgsSemanticInterpretationTag(outPluginRuleItemSemantic));    

                choices.Add(pluginRuleItem);
            }
        }
    }
}