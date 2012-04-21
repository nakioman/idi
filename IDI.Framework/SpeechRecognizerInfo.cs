using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Linq;
using System.Speech.Recognition.SrgsGrammar;
using IDI.Framework.Configuration;
using IDI.Framework.Exceptions;
using Microsoft.Kinect;

namespace IDI.Framework
{
    public class SpeechRecognizerInfo : IPartImportsSatisfiedNotification
    {

        [ImportMany(typeof(BasePlugin), AllowRecomposition = true)]
        private IEnumerable<BasePlugin> _plugins;
        private readonly IDIFrameworkSection _config;
        private readonly SpeechRecognitionEngine _speechRecognitionEngine;

        public SpeechRecognizerInfo()
        {
            _config = (IDIFrameworkSection)ConfigurationManager.GetSection("idiFrameworkSection");

            _speechRecognitionEngine = GetSpeechRecognitionEngine();            
        }

        private void StartSpeechRecognition()
        {
            _speechRecognitionEngine.SpeechRecognized += SpeechRecognitionSpeechRecognized;
            _speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        void SpeechRecognitionSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result == null) return;
            var semantics = e.Result.Semantics;
            if (semantics == null) return;
            var plugin = _plugins.SingleOrDefault(x => x.Id == (string)semantics["type"].Value);
            if (plugin == null) return;
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

            plugin.Execute(dictionary);
        }

        private SpeechRecognitionEngine GetSpeechRecognitionEngine()
        {
            var speechRecognitionEngine = new SpeechRecognitionEngine(new CultureInfo(_config.SpeechRecognitionElement.Culture));

            if (_config.SpeechRecognitionElement.UseKinectAudioSource)
            {
                var sensor = KinectSensor.KinectSensors.FirstOrDefault();
                if (sensor == null)
                {
                    throw new IDIRuntimeException("Can't find kinect sensor, is it connected?");
                }

                sensor.Start();
                var audioSource = sensor.AudioSource;
                audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
                var kinectStream = audioSource.Start();
                speechRecognitionEngine.SetInputToAudioStream(kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            }
            else
            {
                speechRecognitionEngine.SetInputToDefaultAudioDevice();
            }

            return speechRecognitionEngine;
        }

        private void LoadGrammarForEngineAndPlugins()
        {
            var mainDocument = new SrgsDocument();

            //Rule for plugins
            var pluginRules = new SrgsRule("plugin");
            var choices = new SrgsOneOf();
            pluginRules.Add(choices);

            //Main rule (entry point for speech)
            var mainRule = new SrgsRule("main");
            var systemName = new SrgsItem(_config.SpeechRecognitionElement.Name);
            var speakAnything = SrgsRuleRef.Garbage;
            var refPluginRules = new SrgsRuleRef(pluginRules);
            var executeCommandRule = new SrgsItem(_config.SpeechRecognitionElement.ExecuteCommandPhrase);

            //Genere main Rule
            mainRule.Add(systemName);
            mainRule.Add(speakAnything);
            mainRule.Add(refPluginRules);
            mainRule.Add(new SrgsSemanticInterpretationTag("out=rules.plugin;"));
            mainRule.Add(speakAnything);
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

            _speechRecognitionEngine.LoadGrammar(new Grammar(mainDocument));
        }

        public void OnImportsSatisfied()
        {
            StopSpeechRecognition();
            LoadGrammarForEngineAndPlugins();
            StartSpeechRecognition();
        }

        private void StopSpeechRecognition()
        {
            _speechRecognitionEngine.RecognizeAsyncStop();
            _speechRecognitionEngine.UnloadAllGrammars();
        }
    }
}