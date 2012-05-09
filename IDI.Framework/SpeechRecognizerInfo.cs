using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Media;
using System.Linq;
using System.Xml;
using IDI.Framework.Configuration;
using IDI.Framework.Plugins;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Recognition.SrgsGrammar;
using log4net;
using IDI.Framework.Properties;

namespace IDI.Framework
{
    [Export]
    public class SpeechRecognizerInfo : IPartImportsSatisfiedNotification
    {
        [Import]
        private ILog _log;

        [ImportMany(typeof(BasePlugin), AllowRecomposition = true)]
        private IEnumerable<BasePlugin> _plugins;

        private readonly IDIFrameworkSection _config;
        private readonly SpeechRecognitionEngine _speechRecognitionEngine;
        private KinectSensorInfo _kinectSensor;

        public SpeechRecognizerInfo()
        {
            _config = (IDIFrameworkSection)ConfigurationManager.GetSection("idiFrameworkSection");
            _speechRecognitionEngine = SetupSpeechRecognitionEngine();
        }

        private SpeechRecognitionEngine SetupSpeechRecognitionEngine()
        {
            var speechRecognitionEngine = new SpeechRecognitionEngine(new CultureInfo(_config.SpeechRecognitionElement.Culture));

            if (_config.SpeechRecognitionElement.UseKinectAudioSource)
            {
                _kinectSensor = new KinectSensorInfo(speechRecognitionEngine);
            }
            else
            {
                speechRecognitionEngine.SetInputToDefaultAudioDevice();
            }

            return speechRecognitionEngine;
        }

        private void LoadGrammarForEngineAndPlugins()
        {
            var memoryStream = new MemoryStream(Resources.mainDocument);
            var xmlReader = new XmlTextReader(memoryStream);
            var mainDocument = new SrgsDocument(xmlReader);

            //Rule for plugins
            var pluginRules = new SrgsRule("plugin");
            var choices = new SrgsOneOf();
            pluginRules.Add(choices);

            //Main rule (entry point for speech)
            var mainRule = new SrgsRule("main");
            var systemName = new SrgsItem(_config.SpeechRecognitionElement.Name);
            var speakAnything = SrgsRuleRef.Garbage;
            var refPluginRules = new SrgsRuleRef(pluginRules);
            SrgsItem executeCommandRule = null;
            if (!String.IsNullOrWhiteSpace(_config.SpeechRecognitionElement.ExecuteCommandPhrase))
                executeCommandRule = new SrgsItem(_config.SpeechRecognitionElement.ExecuteCommandPhrase);

            //Genere main Rule
            mainRule.Add(systemName);
            mainRule.Add(speakAnything);
            mainRule.Add(refPluginRules);
            mainRule.Add(new SrgsSemanticInterpretationTag("out=rules.plugin;"));
            mainRule.Add(speakAnything);
            if (!String.IsNullOrWhiteSpace(_config.SpeechRecognitionElement.ExecuteCommandPhrase))
                mainRule.Add(executeCommandRule);
            mainRule.Scope = SrgsRuleScope.Private;

            //Here you should add choices to the document per Plugin
            foreach (var plugin in _plugins)
            {
                mainDocument.Rules.Add(plugin.GetGrammarRules());

                var pluginRuleItem = new SrgsItem();
                var refCurrentPluginRule = new SrgsRuleRef(mainDocument.Rules.Single(x => x.Id == plugin.Id));
                var outPluginRuleItemSemantic = String.Format("out.type=\"{0}\"; out.params=rules.{0};", plugin.Id);

                pluginRuleItem.Add(refCurrentPluginRule);
                pluginRuleItem.Add(new SrgsSemanticInterpretationTag(outPluginRuleItemSemantic));
                choices.Add(pluginRuleItem);
            }

            mainDocument.Rules.Add(mainRule);
            mainDocument.Rules.Add(pluginRules);
            mainDocument.Root = mainRule;
            mainDocument.Culture = _speechRecognitionEngine.RecognizerInfo.Culture;
            mainDocument.Mode = SrgsGrammarMode.Voice;
            mainDocument.PhoneticAlphabet = SrgsPhoneticAlphabet.Sapi;

            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = new XmlTextWriter(textWriter))
                {
                    mainDocument.WriteSrgs(xmlWriter);
                    _log.Info(String.Format("Grammar for the recognition engine in the culture {0}, is {1}", _speechRecognitionEngine.RecognizerInfo.Culture.TwoLetterISOLanguageName, textWriter));
                }
            }

            _speechRecognitionEngine.LoadGrammar(new Grammar(mainDocument));
        }

        public void OnImportsSatisfied()
        {
            if (_kinectSensor != null && _kinectSensor.IsStarted)
            {
                _kinectSensor.Stop();
            }

            _speechRecognitionEngine.RecognizeAsyncStop();
            _speechRecognitionEngine.UnloadAllGrammars();
            LoadGrammarForEngineAndPlugins();
            _speechRecognitionEngine.SpeechRecognized += SpeechRecognitionSpeechRecognized;

            if (_kinectSensor != null && _config.SpeechRecognitionElement.UseKinectAudioSource)
            {
                _kinectSensor.Start();
            }
            else
            {
                _speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        private static void PlayRecognitionError()
        {
            using (var soundplayer = new SoundPlayer { Stream = Resources.SpeechNotRecognized })
            {
                soundplayer.Play();
            }
        }

        private static void PlayRecognitionOk()
        {
            using (var soundPlayer = new SoundPlayer { Stream = Resources.SpeechRecognized })
            {
                soundPlayer.Play();
            }
        }

        private void SpeechRecognitionSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null)
            {
                _log.Debug(String.Format("Speech recognized, result {0}, confidence {1}", e.Result.Text, e.Result.Confidence));

                if (e.Result.Confidence >= _config.SpeechRecognitionElement.MinimunConfidence)
                {
                    var semantics = e.Result.Semantics;
                    if (semantics != null)
                    {
                        var plugin = _plugins.Single(x => x.Id == (string)semantics["type"].Value);
                        var dictionary = GetPluginParameters(semantics);

                        _log.Debug(String.Format("Plugin found name {0}", plugin.Id));

                        PlayRecognitionOk();
                        plugin.Execute(dictionary);
                    }
                }
                else
                {
                    PlayRecognitionError();
                }
            }
            else
            {
                PlayRecognitionError();
            }
        }

        private static Dictionary<string, string> GetPluginParameters(SemanticValue semantics)
        {
            Dictionary<string, string> dictionary = null;

            if (semantics["params"] != null && semantics["params"].Count > 0)
            {
                dictionary = new Dictionary<string, string>();
                foreach (var semantic in semantics["params"])
                {
                    var semanticValue = semantic.Value;
                    dictionary.Add(semantic.Key, (string)semanticValue.Value);
                }
            }
            return dictionary;
        }

        public SpeechRecognitionEngine SpeechRecognitionEngine
        {
            get { return _speechRecognitionEngine; }
        }
    }
}