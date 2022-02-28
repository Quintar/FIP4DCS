using Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Core;

namespace FIP4DCS.FIP
{
    public class HelperFunctions
    {
        public static CssData CssData;

        public static string ExePath;

        private static CachedSound _clickSound = null;
        public static void PlayClickSound()
        {
            if (_clickSound != null)
            {
                try
                {
                    AudioPlaybackEngine.Instance.PlaySound(_clickSound);
                }
                catch (Exception ex)
                {
                    Program.Log.Error($"PlaySound: {ex}");
                }
            }
        }

        private static void GetExePath()
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            ExePath = Path.GetDirectoryName(strExeFilePath);
        }

    }
}
