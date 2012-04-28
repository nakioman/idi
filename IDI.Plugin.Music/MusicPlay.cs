using System;
using System.Collections.Generic;
using Microsoft.Speech.Recognition.SrgsGrammar;
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
            srgsRuleMusic.Add(new SrgsSemanticInterpretationTag("out.artist = rules.artist; out.album = rules.album; out.title = rules.title"));

            var mediaList = Player.mediaCollection.getByAttribute("MediaType", "audio");
            for (var i = 0; i < mediaList.count; i++)
            {
                var item = mediaList.Item[i];

                var author = item.getItemInfo("author");
                var album = item.getItemInfo("album");
                var title = item.getItemInfo("title");

                if (!String.IsNullOrWhiteSpace(author))
                {
                    try
                    {
                        var srgsAuthorItem = new SrgsItem(author);
                        srgsOneOfArtist.Add(srgsAuthorItem);
                    }
                    catch (FormatException exception)
                    {
                        Log.Info(string.Format("the author can't be parsed: {0}", exception));
                    }
                }

                if (!String.IsNullOrWhiteSpace(album))
                {
                    try
                    {
                        var srgsAlbumItem = new SrgsItem(album);
                        srgsOneOfAlbum.Add(srgsAlbumItem);
                    }
                    catch (FormatException exception)
                    {
                        Log.Info(string.Format("the album can't be parsed: {0}", exception));
                    }
                }

                if (!String.IsNullOrWhiteSpace(title))
                {
                    try
                    {
                        var srgsTitleItem = new SrgsItem(title);
                        srgsOneOfTitle.Add(srgsTitleItem);
                    }
                    catch (FormatException exception)
                    {
                        Log.Info(string.Format("the title of the song can't be parsed: {0}", exception));
                    }
                }
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
