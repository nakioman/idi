﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using IDI.Plugin.Music.Properties;
using Microsoft.Speech.Recognition.SrgsGrammar;

namespace IDI.Plugin.Music
{
    public class MusicContinuePause : BaseMusic
    {
        public override string Id
        {
            get { return "musicContinuePause"; }
        }

        protected override SrgsRule[] GetGrammarRules()
        {
            using (var stream = new MemoryStream(Resources.musicContinuePause))
            using (var xmlReader = XmlReader.Create(stream))
            {
                var srgsDocument = new SrgsDocument(xmlReader);
                return srgsDocument.Rules.ToArray();
            }
        }

        public override void Execute(IDictionary<string, string> dictionary)
        {
            switch (dictionary["option"])
            {
                case "pause":
                    Player.controls.pause();
                    break;
                case "play":
                    Player.controls.play();
                    break;
                case "next":
                    Player.controls.next();
                    break;
                case "previous":
                    Player.controls.previous();
                    break;
            }
        }
    }
}