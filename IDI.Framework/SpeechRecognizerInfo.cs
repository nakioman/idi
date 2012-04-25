using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Media;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Linq;
using System.Speech.Recognition.SrgsGrammar;
using System.Xml;
using IDI.Framework.Configuration;
using IDI.Framework.Exceptions;
using Microsoft.Kinect;
using log4net;
using IDI.Framework.Properties;

namespace IDI.Framework
{
    public class SpeechRecognizerInfo : IPartImportsSatisfiedNotification
    {

        [ImportMany(typeof(BasePlugin), AllowRecomposition = true)]
        private IEnumerable<BasePlugin> _plugins;
        private readonly IDIFrameworkSection _config;
        private readonly SpeechRecognitionEngine _speechRecognitionEngine;
        [Import]
        private ILog _log;
        KinectSensor _sensor;
        private bool _speechRecognitionOn;

        public SpeechRecognizerInfo()
        {
            _config = (IDIFrameworkSection)ConfigurationManager.GetSection("idiFrameworkSection");
            _speechRecognitionEngine = SetupSpeechRecognitionEngine();
        }

        void SpeechRecognitionSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null)
            {
                _log.Debug(String.Format("Speech recognized, result {0}, confidence {1}", e.Result.Text,
                                         e.Result.Confidence));

                if (e.Result.Confidence >= _config.SpeechRecognitionElement.MinimunConfidence)
                {
                    var semantics = e.Result.Semantics;
                    if (semantics == null) return;
                    var plugin = _plugins.SingleOrDefault(x => x.Id == (string)semantics["type"].Value);
                    if (plugin == null) return;

                    _log.Debug(String.Format("Plugin found name {0}", plugin.Id));

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

                    PlayRecognitionOk();
                    plugin.Execute(dictionary);
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

        private void PlayRecognitionOk()
        {
            using (var soundPlayer = new SoundPlayer { Stream = Resources.SpeechRecognized })
            {
                soundPlayer.Play();
            }
        }

        private SpeechRecognitionEngine SetupSpeechRecognitionEngine()
        {
            var speechRecognitionEngine = new SpeechRecognitionEngine(new CultureInfo(_config.SpeechRecognitionElement.Culture));

            if (_config.SpeechRecognitionElement.UseKinectAudioSource)
            {
                _sensor = KinectSensor.KinectSensors.FirstOrDefault();
                if (_sensor == null)
                {
                    throw new IDIRuntimeException("Can't find kinect sensor, is it connected?", null);
                }

                _sensor.ColorStream.Disable();
                _sensor.SkeletonStream.Enable();
                _sensor.DepthStream.Disable();
                _sensor.Start();


                var audioSource = _sensor.AudioSource;
                audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
                audioSource.EchoCancellationMode = EchoCancellationMode.CancellationAndSuppression;
                audioSource.NoiseSuppression = true;
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
            _speechRecognitionEngine.UnloadAllGrammars();
            LoadGrammarForEngineAndPlugins();
            _speechRecognitionEngine.SpeechRecognized += SpeechRecognitionSpeechRecognized;
            if (_config.SpeechRecognitionElement.UseKinectAudioSource)
            {
                _sensor.SkeletonFrameReady += SensorSkeletonFrameReady;
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

        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var skeletonFrame = e.OpenSkeletonFrame();
            if (skeletonFrame == null)
            {
                if (_speechRecognitionOn)
                {
                    _speechRecognitionEngine.RecognizeAsyncStop();
                    _speechRecognitionOn = false;
                }

            }
            else
            {
                var skeleton = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeleton);
                var skeletonReady = skeleton.SingleOrDefault(x => x.TrackingState == SkeletonTrackingState.Tracked);

                if (skeletonReady == null)
                {
                    if (_speechRecognitionOn)
                    {
                        _speechRecognitionEngine.RecognizeAsyncStop();
                        _speechRecognitionOn = false;
                    }
                }
                else
                {
                    if (!_speechRecognitionOn)
                    {
                        _speechRecognitionOn = true;
                        _speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
                    }
                }
            }
        }
    }
}