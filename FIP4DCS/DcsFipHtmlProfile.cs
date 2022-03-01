using NCalc;
using Newtonsoft.Json;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WinForms;
using static DCSBios.DCSBios;

namespace FIP4DCS
{
    public struct ButtonSetting
    {
        public string Internal;
        public string DCSCommand;
        public string DCSArgumentPressed;
        public string DCSArgumentReleased;
    }
    public struct GaugeSetting
    {
        public string DCSAddress;
        public string Formular;
    }
    public struct FIPSettings
    {
        public ButtonSetting S1;
        public ButtonSetting S2;
        public ButtonSetting S3;
        public ButtonSetting S4;
        public ButtonSetting S5;
        public ButtonSetting S6;
        public ButtonSetting LeftRotaryPlus;
        public ButtonSetting LeftRotaryMinus;
        public ButtonSetting RightRotaryPlus;
        public ButtonSetting RightRotaryMinus;
        public GaugeSetting[] Gauges;
    }
    public class DcsFipHtmlProfile
    {
        private CssData _CssData;
        private string _FilePath;
        private IRazorEngineService _razor;

        private const int HtmlWindowXOffset = 1;
        private int _htmlWindowWidth = 320;
        private int _htmlWindowHeight = 240;
        private int _currentLcdYOffset = 0;
        private int _currentLcdHeight = 0;
        private int HtmlWindowUsableWidth => _htmlWindowWidth - 9 - HtmlWindowXOffset;

        private List<string> _htmlSites = new List<string>();
        public List<string> Sites { get { return _htmlSites; } }
        string pageName = "";
        EventHandler<HtmlImageLoadEventArgs> onImageLoad = null;

        private string _profileName = "";
        public string ProfileName { get { return _profileName; } }

        private FIPSettings fIPSettings = new FIPSettings();
        private bool _compile = false;

        public DcsFipHtmlProfile(string filePath, bool compile = true)
        {
            _FilePath = filePath;
            _compile = compile;
            if (Directory.Exists(_FilePath))
            {

                _profileName = _FilePath.Substring(_FilePath.LastIndexOf("\\") + 1);
            }
        }

            public void PrepareProfile()
        {
            if (Directory.Exists(_FilePath))
            {
                var config = new TemplateServiceConfiguration
                {
                    TemplateManager = new ResolvePathTemplateManager(new[] { _FilePath }),
                    DisableTempFileLocking = true,
                    BaseTemplateType = typeof(HtmlSupportTemplateBase<>)
                };
                _razor = RazorEngineService.Create(config);

                if (_compile)
                {
                    CompileProfileFile();
                }
            }
        }

        //private List<string> GaugePicturesFilenames = new List<string>();
        //private List<Image> GaugePictures = new List<Image>();
        private List<GaugePicture> gaugePictures = new List<GaugePicture>();

        private struct GaugePicture
        {
            public string Name { get; set; }
            public Image Image { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public bool rotate { get; set; }
            public bool slideud { get; set; }
            public bool slidelr { get; set; }
            public int gaugeindex { get; set; }
        }

        public void CompileProfileFile()
        {
            _htmlSites.Clear();
            var files = Directory.EnumerateFiles(_FilePath+"\\", "*.*");
            gaugePictures.Clear();
            foreach (var file in files)
            {
                if (file.ToLower().EndsWith("fip.json")) 
                    fIPSettings = JsonConvert.DeserializeObject<FIPSettings>(File.ReadAllText(file));
                if (file.EndsWith(".cshtml")) _razor.Compile(file);
                if (file.EndsWith(".css"))
                {
                    var thisCssData = HtmlRender.ParseStyleSheet(File.ReadAllText(file), true);
                    if (_CssData != null) _CssData.Combine(thisCssData);
                    else _CssData = thisCssData;
                }
                if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg")) {
                    int iTest = -1;
                    var f = file.Replace(_FilePath + "\\", "");
                    if (int.TryParse(f[0].ToString(), out iTest))
                    {
                        GaugePicture gp = new GaugePicture();
                        gp.Name = file.Substring(file.LastIndexOf("\\") + 1);
                        gp.Image = Image.FromFile(file);

                        int x = 0;
                        int y = 0;
                        int g = 0;
                        if (gp.Name.Contains("_x"))
                        {
                            int pos1 = gp.Name.IndexOf("_x") + 2;
                            string xtmp = gp.Name.Substring(pos1, gp.Name.IndexOf("_", pos1) - pos1);
                            int.TryParse(xtmp, out x);
                        }
                        if (gp.Name.Contains("_y"))
                        {
                            int pos1 = gp.Name.IndexOf("_y") + 2;
                            string xtmp = gp.Name.Substring(pos1, gp.Name.IndexOf("_", pos1) - pos1);
                            int.TryParse(xtmp, out y);
                        }
                        if (gp.Name.Contains("_g"))
                        {
                            int pos1 = gp.Name.IndexOf("_g") + 2;
                            string xtmp = gp.Name.Substring(pos1, gp.Name.IndexOf("_", pos1) - pos1);
                            int.TryParse(xtmp, out g);
                        }
                        if (gp.Name.Contains("_rot_")) { gp.rotate = true; } else { gp.rotate = false; }
                        if (gp.Name.Contains("_slideud_")) { gp.slideud = true; } else { gp.slideud = false; }
                        if (gp.Name.Contains("_slidelr_")) { gp.slidelr = true; } else { gp.slidelr = false; }

                        gp.X = x;
                        gp.Y = y;
                        gp.gaugeindex = g;

                        gaugePictures.Add(gp);
                    }
                }

                _htmlSites.Add(file);
            }
            gaugePictures.Sort((s1, s2) => s1.Name.CompareTo(s2.Name));
        }

        public void PrepareRender (int PageNumber, EventHandler<HtmlImageLoadEventArgs> OnImageLoad)
        {
            PrepareRender(_htmlSites[PageNumber], OnImageLoad);
        }

        public void PrepareRender(string PageName, EventHandler<HtmlImageLoadEventArgs> myOnImageLoad)
        {
            pageName = PageName.Trim();
            if(myOnImageLoad == null) onImageLoad = OnImageLoad;
            else onImageLoad = myOnImageLoad;
        }

        public int fps = 0;
        public Dictionary<int, MiniBios> miniBios = new Dictionary<int, MiniBios>();
        public string Page = "";

        public string sendDCSBIOS = "";
        private string lastDCSBIOSCommand = "";

        public void Render(Graphics graphics, bool mustRender)
        {
            var model = new
            {
                fps = fps,
                dcs = miniBios,
                page = Page
            };
            DynamicViewBag dynamicViewBag = new DynamicViewBag();
            string str = _razor.Run(pageName, null, model, dynamicViewBag); //Model.???
            graphics.Clear(System.Drawing.Color.White);
            var measureData = HtmlRender.Measure(graphics, str, HtmlWindowUsableWidth, _CssData, null, onImageLoad);
            _currentLcdHeight = (int)measureData.Height;

            CheckLcdOffset();

            var _htmlImage = HtmlRender.RenderToImage(str,
                                                new Size(HtmlWindowUsableWidth, (int)measureData.Height + 20), System.Drawing.Color.White, _CssData,
                                                null, onImageLoad);

            if (_htmlImage != null)
            {
                graphics.DrawImage(_htmlImage, new Rectangle(new Point(HtmlWindowXOffset, 0),
                        new Size(HtmlWindowUsableWidth, (int)measureData.Height + 20)),
                    new Rectangle(new Point(0, _currentLcdYOffset),
                        new Size(HtmlWindowUsableWidth, (int)measureData.Height + 20)),
                    GraphicsUnit.Pixel);

                lastImage = _htmlImage;
            }
        }

        //int rot = 0;
        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            var bm = new Bitmap(1, 1);
            var browser = (HtmlContainerInt)sender;
            if (e.Src.ToLower().Contains("gauge"))
            {
                    int[] rot = new int[fIPSettings.Gauges.Length];
                    int i = 0;
                    //int gaugeIndex = 0;
                    //if (e.Src.Length > "gauge".Length) int.TryParse(e.Src.ToLower().Replace("gauge", ""), out gaugeIndex);
                    foreach (GaugeSetting gauge in fIPSettings.Gauges)
                    {
                        //= [gaugeIndex];
                        double per = 0;
                        try
                        {
                            int value = miniBios[Convert.ToInt32(gauge.DCSAddress, 16)].value;
                            string formular = gauge.Formular.Replace("value", (value & 0xffff).ToString());
                            Expression exp = new Expression(formular);
                            per = (double)exp.Evaluate();
                        } catch { }
                        rot[i] = (int)(per);
                        i++;
                    }

                    foreach (GaugePicture gp in gaugePictures)
                    {
                        var image = gp.Image;
                        if (bm.Width == 1 && bm.Height == 1) bm = new Bitmap(image.Width, image.Height);
                    if (image != null)
                    {
                        using (Graphics g = Graphics.FromImage(bm))
                        {
                            try
                            {

                                if (gp.slideud)
                                {
                                    g.TranslateTransform(0, rot[gp.gaugeindex]);
                                }
                                if (gp.slidelr)
                                {
                                    g.TranslateTransform(rot[gp.gaugeindex], 0);
                                }
                                if (gp.rotate)
                                {
                                    g.TranslateTransform(image.Width / 2 + gp.X, image.Height / 2 + gp.Y);
                                    g.RotateTransform(rot[gp.gaugeindex]);
                                    g.TranslateTransform(-(image.Width / 2 + gp.X), -(image.Height / 2 + gp.Y));
                                }
                                g.DrawImage(image, gp.X, gp.Y, image.Width, image.Height);
                            } catch { }
                        }
                    }
                }
            }
            else
            {
                var image = Image.FromFile(_FilePath + "\\" + e.Src);
                bm = new Bitmap(image.Width, image.Height);
                using (Graphics g = Graphics.FromImage(bm))
                {
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }
                e.Callback(bm);
            e.Handled = true;
        }
            

        private void prepareDCSAction(ButtonSetting setting)
        {
            if (setting.DCSCommand != "")
            {
                sendDCSBIOS = setting.DCSCommand;
                if (setting.DCSArgumentPressed != "") sendDCSBIOS += " " + setting.DCSArgumentPressed;
                sendDCSBIOS += "\n";
                if (setting.DCSArgumentReleased != "") lastDCSBIOSCommand = setting.DCSCommand + " " + setting.DCSArgumentReleased + "\n";
            }
        }

        public void Buttons(uint buttons)
        {

            //+ Right
            if (buttons == 2)
            {
                if (fIPSettings.RightRotaryPlus.Internal != "")
                {

                }
                prepareDCSAction(fIPSettings.RightRotaryPlus);
            }

            //- Right
            if (buttons == 4)
            {
                if (fIPSettings.RightRotaryMinus.Internal != "")
                {

                }
                prepareDCSAction(fIPSettings.RightRotaryMinus);
            }

            //+ Left
            if (buttons == 8)
            {
                if (fIPSettings.LeftRotaryPlus.Internal != "")
                {

                }
                prepareDCSAction(fIPSettings.LeftRotaryPlus);
            }

            //- Left
            if (buttons == 16)
            {
                if (fIPSettings.LeftRotaryMinus.Internal != "")
                {

                }
                prepareDCSAction(fIPSettings.LeftRotaryMinus);
            }

            //S1
            if (buttons == 32)
            {
                if (fIPSettings.S1.Internal != "")
                {
                    if(fIPSettings.S1.Internal.ToLower().Contains("page"))
                    {
                        Page = fIPSettings.S1.Internal;
                    }
                }
                prepareDCSAction(fIPSettings.S1);
            }

            //S2
            if (buttons == 64)
            {
                if (fIPSettings.S2.Internal != "")
                {
                    if (fIPSettings.S2.Internal.ToLower().Contains("page"))
                    {
                        Page = fIPSettings.S2.Internal;
                    }
                }
                prepareDCSAction(fIPSettings.S2);
            }

            //S3
            if (buttons == 128)
            {
                if (fIPSettings.S3.Internal != "")
                {
                    if (fIPSettings.S3.Internal.ToLower().Contains("page"))
                    {
                        Page = fIPSettings.S3.Internal;
                    }
                }
                prepareDCSAction(fIPSettings.S3);
            }

            //S4
            if (buttons == 256)
            {
                if (fIPSettings.S4.Internal != "")
                {
                    if (fIPSettings.S4.Internal.ToLower().Contains("page"))
                    {
                        Page = fIPSettings.S4.Internal;
                    }
                }
                prepareDCSAction(fIPSettings.S4);
            }

            //S5
            if (buttons == 512)
            {
                if (fIPSettings.S5.Internal != "")
                {
                    if (fIPSettings.S5.Internal.ToLower().Contains("page"))
                    {
                        Page = fIPSettings.S5.Internal;
                    }
                }
                prepareDCSAction(fIPSettings.S5);
            }

            //S6
            if (buttons == 1024)
            {
                if (fIPSettings.S6.Internal != "")
                {
                    if (fIPSettings.S6.Internal.ToLower().Contains("page"))
                    {
                        Page = fIPSettings.S6.Internal;
                    }
                }
                prepareDCSAction(fIPSettings.S6);
            }


            //Depressed Buttons
            if (buttons == 0)
            {
                if (lastDCSBIOSCommand != "") { sendDCSBIOS = lastDCSBIOSCommand; lastDCSBIOSCommand = ""; }
            }
        }


        private Image lastImage = null;
        public Image LastImage { get { return lastImage; } }

        private void CheckLcdOffset()
        {
            if (_currentLcdHeight <= _htmlWindowHeight)
            {
                _currentLcdYOffset = 0;
            }

            if (_currentLcdYOffset + _htmlWindowHeight > _currentLcdHeight)
            {
                _currentLcdYOffset = _currentLcdHeight - _htmlWindowHeight + 4;
            }

            if (_currentLcdYOffset < 0) _currentLcdYOffset = 0;
        }


        public class MyHtmlHelper
        {
            public IEncodedString Raw(string rawString)
            {
                return new RawString(rawString);
            }
        }

        public abstract class HtmlSupportTemplateBase<T> : TemplateBase<T>
        {
            public HtmlSupportTemplateBase()
            {
                Html = new MyHtmlHelper();
            }

            public MyHtmlHelper Html { get; set; }
        }
    }
}
