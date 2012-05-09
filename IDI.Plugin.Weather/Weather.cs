using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Animaonline.Globals;
using Animaonline.Weather;
using IDI.Framework;
using IDI.Framework.Plugins;
using IDI.Plugin.Weather.Properties;
using Microsoft.Speech.Recognition.SrgsGrammar;

namespace IDI.Plugin.Weather
{
    public class Weather : BasePlugin
    {        
        public override string Id
        {
            get { return "weather"; }
        }        

        protected override SrgsRule[] GetGrammarRules()
        {
            using (var stream = new MemoryStream(Resources.grammar))
            using (var xmlReader = XmlReader.Create(stream))
            {
                var srgsDocument = new SrgsDocument(xmlReader);
                return srgsDocument.Rules.ToArray();
            }
        }

        public override void Execute(IDictionary<string, string> dictionary)
        {
            const LanguageCode languageCode = LanguageCode.en_US;
            var cityName = dictionary["city"];
            var weather = GoogleWeatherAPI.GetWeather(languageCode, cityName);
            var unit = dictionary["unit"];
            var temperature = unit == "celsius"
                ? weather.CurrentConditions.Temperature.Celsius
                : weather.CurrentConditions.Temperature.Fahrenheit;

            var info = String.Format("La temperatura en {0} es de {1} grados {2}", cityName, temperature, unit);
            var speak = String.Format("La temperatura es de {0} grados", temperature);

            Log.Info(info);
            SpeechSynthesizer.SpeechSynthesizer.SpeakAsync(speak);
        }
    }
}
