using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIP4DCS
{
    public class Profile
    {
        private DcsFipHtmlProfile htmlProfile = null;

        public override string ToString()
        {
            if (htmlProfile != null) return htmlProfile.ProfileName;
            else return "";
        }

        public Profile(DcsFipHtmlProfile newHtmlProfile = null)
        {
            if (newHtmlProfile != null)
            {
                htmlProfile = newHtmlProfile;
            }
        }

        public int FPS { set { htmlProfile.fps = value; } }
        
        public DcsFipHtmlProfile HtmlProfile
        {
            get
            {
                return htmlProfile;
            }

            set
            {
                htmlProfile = value;
            }
        }
    }
}
