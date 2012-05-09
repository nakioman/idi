using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using IDI.Framework.Properties;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Recognition.SrgsGrammar;

namespace IDI.Framework.Plugins
{
    public class TrainFacePlugin : BasePlugin
    {
        public override string Id
        {
            get { return "trainFace"; }
        }

        public override SrgsRule[] GetGrammarRules()
        {
            using (var stream = new MemoryStream(Resources.trainFace))
            using (var xmlReader = XmlReader.Create(stream))
            {
                var srgsDocument = new SrgsDocument(xmlReader);
                return srgsDocument.Rules.ToArray();
            }
        }

        public override void Execute(IDictionary<string, string> dictionary)
        {
            //var speechRecognitionEngine = new SpeechRecognitionEngine(SpeechRecognizer.SpeechRecognitionEngine.RecognizerInfo);
            //var dictaphone = new GrammarBuilder{Culture = speechRecognitionEngine.RecognizerInfo.Culture};
            //var spelling = new GrammarBuilder { Culture = speechRecognitionEngine.RecognizerInfo.Culture };
            //spelling.AppendDictation("spelling");

            //dictaphone.Append(new SemanticResultKey("StartSpelling", new SemanticResultValue(Resources.StartSpelling, true)));
            //dictaphone.Append(new SemanticResultKey("SpellingInput", spelling));
            //dictaphone.Append(new SemanticResultKey("EndSpelling", new SemanticResultValue(Resources.EndSpelling, true)));

            //var grammar = new Grammar(dictaphone) {Enabled = true, Name = "Spelling grammar"};            

            //speechRecognitionEngine.LoadGrammar(grammar);
            //var result = speechRecognitionEngine.Recognize();
        }
    }
}