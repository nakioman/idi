using IDI.Framework;
using IDI.Framework.Plugins;
using WMPLib;

namespace IDI.Plugin.Music
{
    public abstract  class BaseMusic : BasePlugin
    {
        protected static WindowsMediaPlayer Player;

        protected BaseMusic()
        {
            Player = new WindowsMediaPlayer();
            Player.settings.autoStart = true;
        }
    }
}