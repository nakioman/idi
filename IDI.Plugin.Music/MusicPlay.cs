using System;
using System.Collections.Generic;
using System.Speech.Recognition.SrgsGrammar;
using IDI.Framework;
using WMPLib;

namespace IDI.Plugin.Music
{
    public class MusicPlay : BaseMusic
    {
        public override string Id
        {
            get { return "musicPlay"; }
        }

        public override SrgsRule[] GetGrammarRules()
        {
            var srgsRuleArtist = new SrgsRule("artist");
            var srgsItemArtist = new SrgsItem(0, 1);
            var srgsOneOfArtist = new SrgsOneOf();
            srgsItemArtist.Add(srgsOneOfArtist);
            srgsRuleArtist.Add(srgsItemArtist);

            var srgsRuleAlbum = new SrgsRule("album");
            var srgsItemAlbum = new SrgsItem(0, 1);
            var srgsOneOfAlbum = new SrgsOneOf();
            srgsItemAlbum.Add(srgsOneOfAlbum);
            srgsRuleAlbum.Add(srgsItemAlbum);

            var srgsRuleTitle = new SrgsRule("title");
            var srgsItemTitle = new SrgsItem(0, 1);
            var srgsOneOfTitle = new SrgsOneOf();
            srgsItemTitle.Add(srgsOneOfTitle);
            srgsRuleTitle.Add(srgsItemTitle);

            var srgsRuleMusic = new SrgsRule("musicPlay");
            srgsRuleMusic.Add(new SrgsItem("reproducí"));
            srgsRuleMusic.Add(SrgsRuleRef.Garbage);
            srgsRuleMusic.Add(new SrgsRuleRef(srgsRuleArtist));
            srgsRuleMusic.Add(SrgsRuleRef.Garbage);
            srgsRuleMusic.Add(new SrgsRuleRef(srgsRuleAlbum));
            srgsRuleMusic.Add(SrgsRuleRef.Garbage);
            srgsRuleMusic.Add(new SrgsRuleRef(srgsRuleTitle));
            srgsRuleMusic.Add(SrgsRuleRef.Garbage);
            srgsRuleMusic.Add(new SrgsRuleRef(srgsRuleArtist));
            srgsRuleMusic.Add(new SrgsSemanticInterpretationTag("out.artist = rules.artist; out.album = rules.album; out.title = rules.title"));

            var mediaList = Player.mediaCollection.getByAttribute("MediaType", "audio");
            for (var i = 0; i < mediaList.count; i++)
            {
                var item = mediaList.Item[i];

                var author = item.getItemInfo("author");
                var album = item.getItemInfo("album");
                var title = item.getItemInfo("title");

                var srgsAuthorItem = new SrgsItem(author);
                //srgsAuthorItem.Add(new SrgsSemanticInterpretationTag("out = \"" + author + "\""));
                srgsOneOfArtist.Add(srgsAuthorItem);

                var srgsAlbumItem = new SrgsItem(album);
                //srgsAlbumItem.Add(new SrgsSemanticInterpretationTag("out = \"" + album + "\""));
                srgsOneOfAlbum.Add(srgsAlbumItem);

                var srgsTitleItem = new SrgsItem(title);
                //srgsTitleItem.Add(new SrgsSemanticInterpretationTag("out = \"" + title + "\""));
                srgsOneOfTitle.Add(srgsTitleItem);
            }

            return new[] { srgsRuleAlbum, srgsRuleArtist, srgsRuleMusic, srgsRuleTitle };
        }

        public override void Execute(IDictionary<string, string> dictionary)
        {
            var artist = dictionary["artist"];
            var album = dictionary["album"];
            var title = dictionary["title"];
            IWMPPlaylist media = null;

            if (!String.IsNullOrWhiteSpace(title))
            {
                media = Player.mediaCollection.getByName(title);
                var titles = Player.mediaCollection.getByAttribute("MediaType", "audio");
                titles.clear();
                if (!String.IsNullOrWhiteSpace(artist))
                {
                    for (var i = 0; i < media.count; i++)
                    {
                        if (media.Item[i].getItemInfo("author") == artist)
                        {
                            titles.appendItem(media.Item[i]);
                        }
                    }

                    media = titles;
                }
            }
            else if (!String.IsNullOrWhiteSpace(album))
            {
                media = Player.mediaCollection.getByAlbum(album);
                var albums = Player.mediaCollection.getByAttribute("MediaType", "audio");
                albums.clear();
                if (!String.IsNullOrWhiteSpace(artist))
                {
                    for (var i = 0; i < media.count; i++)
                    {
                        if (media.Item[i].getItemInfo("author") == artist)
                        {
                            albums.appendItem(media.Item[i]);
                        }
                    }

                    media = albums;
                }
            }
            else if (!String.IsNullOrWhiteSpace(artist))
            {
                media = Player.mediaCollection.getByAuthor(artist);
            }

            if (media != null)
            {
                Player.currentPlaylist = media;
            }
        }
    }
}
