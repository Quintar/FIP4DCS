using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WinForms;

namespace FIP4DCS.FIP
{
    public class FipPanel
    {
        private readonly object _refreshDevicePageLock = new object();

        private bool _initOk;

        private int CurrentCard = 0;

        private const int DEFAULT_PAGE = 0;

        private int _currentLcdYOffset;
        private int _currentLcdHeight;

        public IntPtr FipDevicePointer;
        private string SerialNumber;

        private uint _prevButtons;

        private bool[] _ledState = new bool[7];

        private List<uint> _pageList = new List<uint>();
        public uint PageNumber(int index) { return _pageList[index]; }

        public int PageIndex(uint pageNumber) {
            for (int i=0; i< _pageList.Count; i++)
                if (_pageList[i] == pageNumber) return i;
            return 0;
        }

        private readonly Pen _scrollPen = new Pen(Color.FromArgb(0xff, 0xFF, 0xB0, 0x00));
        private readonly Pen _whitePen = new Pen(Color.FromArgb(0xff, 0xFF, 0xFF, 0xFF), (float)0.1);


        private readonly SolidBrush _scrollBrush = new SolidBrush(Color.FromArgb(0xff, 0xFF, 0xB0, 0x00));
        private readonly SolidBrush _whiteBrush = new SolidBrush(Color.FromArgb(0xff, 0xFF, 0xFF, 0xFF));

        private readonly Font _drawFont = new Font("Arial", 13, GraphicsUnit.Pixel);

        private Image _htmlImage;
        private Image _cardcaptionHtmlImage;

        private const int HtmlWindowXOffset = 1;

        private int _htmlWindowWidth = 320;
        private int _htmlWindowHeight = 240;

        private int HtmlWindowUsableWidth => _htmlWindowWidth - 9 - HtmlWindowXOffset;

        private double ScrollBarHeight => _htmlWindowHeight - 7.0;

        private int ChartImageDisplayWidth => _htmlWindowWidth - 25;

        private const int ChartImageDisplayHeight = 60;

        private DirectOutputClass.PageCallback _pageCallbackDelegate;
        private DirectOutputClass.SoftButtonCallback _softButtonCallbackDelegate;

        private bool _blockNextUpState;

        private delegate void DrawPanel();

        public delegate void CheckButtons(uint buttons);
        public CheckButtons CheckButtonsDelegate;

        public delegate void RedrawPanel(Graphics graphics, bool mustRender);
        public RedrawPanel RedrawPanelDelegate;

        /// <summary>
        /// Constructor to the FIP, please use the function Initialize() next.
        /// </summary>
        /// <param name="devicePtr">Reference to the FIP as an DirectOutput device</param>
        public FipPanel(IntPtr devicePtr)
        {
            FipDevicePointer = devicePtr;
        }

        /// <summary>
        /// Initializes this FIP-panel and needs to be called first with a page that has the number 0
        /// </summary>
        public void Initalize()
        {
            // FIP = 3e083cd8-6a37-4a58-80a8-3d6a2c07513e

            // https://github.com/Raptor007/Falcon4toSaitek/blob/master/Raptor007's%20Falcon%204%20to%20Saitek%20Utility/DirectOutput.h
            //https://github.com/poiuqwer78/fip4j-core/tree/master/src/main/java/ch/poiuqwer/saitek/fip4j

            _pageCallbackDelegate = PageCallback;
            _softButtonCallbackDelegate = SoftButtonCallback;

            var returnValues1 = DirectOutputClass.RegisterPageCallback(FipDevicePointer, _pageCallbackDelegate);
            if (returnValues1 != ReturnValues.S_OK)
            {
                Program.Log.Error("FipPanel failed to init RegisterPageCallback. " + returnValues1);
            }
            var returnValues2 = DirectOutputClass.RegisterSoftButtonCallback(FipDevicePointer, _softButtonCallbackDelegate);
            if (returnValues2 != ReturnValues.S_OK)
            {
                Program.Log.Error("FipPanel failed to init RegisterSoftButtonCallback. " + returnValues1);
            }

            var returnValues3 = DirectOutputClass.GetSerialNumber(FipDevicePointer, out SerialNumber);
            if (returnValues3 != ReturnValues.S_OK)
            {
                Program.Log.Error("FipPanel failed to get Serial Number. " + returnValues1);
            }
            else
            {
                Program.Log.Info("FipPanel Serial Number : " + SerialNumber);

                _initOk = true;

                AddPage(DEFAULT_PAGE, true);

                //RefreshDevicePage();
            }

        }

        /// <summary>
        /// Shuts down this FIP panel and removes each page first.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                if (_pageList.Count > 0)
                {
                    do
                    {
                        if (_initOk)
                        {
                            DirectOutputClass.RemovePage(FipDevicePointer, _pageList[0]);
                        }

                        _pageList.Remove(_pageList[0]);


                    } while (_pageList.Count > 0);
                }
            }
            catch (Exception ex)
            {
                Program.Log.Error(ex);
            }

        }

        /// <summary>
        /// Callback for a page for it to be displayed/refreshed. Used with RegisterPageCallback
        /// </summary>
        /// <param name="device">Reference to the DirectOutput device</param>
        /// <param name="page">The page number (convert to uint) to be refreshed</param>
        /// <param name="bActivated">true if the page is activated, false if not</param>
        /// <param name="context">unused</param>
        private void PageCallback(IntPtr device, IntPtr page, byte bActivated, IntPtr context)
        {
            if (device == FipDevicePointer)
            {
                if (bActivated != 0)
                {
                    RefreshDevicePage();
                }
            }
        }


        /// <summary>
        /// Callback for when a button is pressed. Used for RegisterSoftButtonCallback
        /// </summary>
        /// <param name="device">Reference to the DirectOutput device</param>
        /// <param name="buttons">Pressed button word.
        /// Each pressed button is a 1, a depressed button is a 0, needs to be converted to uint.
        /// Word-Positions: 2 Scrollwheel 1 clockwise, 4 Scrollwheel 1 anti-clockwise, 8 Scrollwheel 2 clockwise, 16 Scrollwheel 2 anti-clockwise, 512 up-button, 1024 down-button</param>
        /// <param name="context"></param>
        private void SoftButtonCallback(IntPtr device, IntPtr buttons, IntPtr context)
        {
            if (CheckButtonsDelegate != null) CheckButtonsDelegate((uint)buttons);
            /* Hier eigene funktion um Knöpfe abzufragen */
        }

        /*

            if (device == FipDevicePointer & (uint)buttons != _prevButtons)
            {
                var button = (uint)buttons ^ _prevButtons;
                var state = ((uint)buttons & button) == button;
                _prevButtons = (uint)buttons;

                //Console.WriteLine($"button {button}  state {state}");

                var mustRefresh = false;

                var mustRender = true;

                switch (button)
                {
                    case 8: // scroll clockwise
                        if (state)
                        {

                            CurrentCard++;
                            _currentLcdYOffset = 0;

                            mustRefresh = true;

                            var playSound = true;


                            if (playSound)
                            {
                                HelperFunctions.PlayClickSound();
                            }
                        }

                        break;
                    case 16: // scroll anti-clockwise

                        if (state)
                        {
                            CurrentCard--;
                            _currentLcdYOffset = 0;

                            mustRefresh = true;

                            var playSound = true;

                            if (playSound)
                            {
                                HelperFunctions.PlayClickSound();
                            }
                        }

                        break;
                    case 2: // scroll clockwise
                        _currentLcdYOffset += 50;

                        mustRender = false;

                        mustRefresh = true;

                        break;
                    case 4: // scroll anti-clockwise

                        if (_currentLcdYOffset == 0) return;

                        _currentLcdYOffset -= 50;
                        if (_currentLcdYOffset < 0)
                        {
                            _currentLcdYOffset = 0;
                        }

                        mustRender = false;

                        mustRefresh = true;

                        break;
                }

                if (!mustRefresh)
                {
                    if (state || !_blockNextUpState)
                    {
                        switch (button)
                        {
                            case 512:

                                CurrentCard++;
                                _currentLcdYOffset = 0;

                                mustRefresh = true;

                                //App.PlayClickSound();

                                break;
                            case 1024:

                                CurrentCard--;
                                _currentLcdYOffset = 0;

                                mustRefresh = true;

                                //App.PlayClickSound();

                                break;
                        }
                    }

                }

                _blockNextUpState = state;

                if (mustRefresh)
                {
                    RefreshDevicePage(mustRender);
                }

            }         
         */

        /// <summary>
        /// Resets the image on the FIP
        /// </summary>
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

        /// <summary>
        /// Adds a new (blank) page to the FIP-panel if it doesn't exist. If it already exists it's not added
        /// </summary>
        /// <param name="pageNumber">Pagenumber to add. It doesn't need to be continous it's more like an ID.</param>
        /// <param name="setActive">De-Activates the page (so it can be navigated to)</param>
        /// <returns></returns>
        public ReturnValues AddPage(uint pageNumber, bool setActive)
        {
            var result = ReturnValues.E_FAIL;

            if (_initOk)
            {
                try
                {
                    if (_pageList.Contains(pageNumber))
                    {
                        return ReturnValues.S_OK;
                    }

                    result = DirectOutputClass.AddPage(FipDevicePointer, (IntPtr)pageNumber, string.Concat("0x", FipDevicePointer.ToString(), " PageNo: ", pageNumber), setActive);
                    if (result == ReturnValues.S_OK)
                    {
                        Program.Log.Info("Page: " + pageNumber + " added");

                        _pageList.Add(pageNumber);
                    }
                }
                catch (Exception ex)
                {
                    Program.Log.Error(ex);
                }
            }

            return result;
        }

        public ReturnValues RemovePage(int pageIndex = 0)
        {
            var ret = DirectOutputClass.RemovePage(FipDevicePointer, _pageList[pageIndex]);
            _pageList.RemoveAt(pageIndex);
            return ret;
        }

        /// <summary>
        /// Sends an image to a page
        /// </summary>
        /// <param name="page">The page index where to send the image to.</param>
        /// <param name="fipImage">A bitmap as a 24bitpp RGB image</param>
        /// <returns></returns>
        public ReturnValues SendImageToFip(uint page, Bitmap fipImage)
        {

            if (_initOk)
            {
                if (fipImage == null)
                {
                    return ReturnValues.E_INVALIDARG;
                }

                try
                {
                    fipImage.RotateFlip(RotateFlipType.Rotate180FlipX);

                    var bitmapData =
                        fipImage.LockBits(new Rectangle(0, 0, fipImage.Width, fipImage.Height),
                            ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    var intPtr = bitmapData.Scan0;
                    var local3 = bitmapData.Stride * fipImage.Height;
                    DirectOutputClass.SetImage(FipDevicePointer, page, 0, local3, intPtr);
                    fipImage.UnlockBits(bitmapData);
                    return ReturnValues.S_OK;
                }
                catch (Exception ex)
                {
                    Program.Log.Error(ex);
                }
            }

            return ReturnValues.E_FAIL;
        }

        /// <summary>
        /// Sets the lighting status of an LED
        /// </summary>
        /// <param name="ledNumber">The index of the LED (at time of writing there are 7 LEDs)</param>
        /// <param name="state">true = on (red), false = off</param>
        private void SetLed(uint ledNumber, bool state)
        {
            if (ledNumber < _ledState.Count() && _ledState[ledNumber] != state)
            {
                DirectOutputClass.SetLed(FipDevicePointer, DEFAULT_PAGE,
                    ledNumber, state);
                _ledState[ledNumber] = state;
            }
        }

        public void RefreshDevicePage(uint page = DEFAULT_PAGE, bool mustRender = true)
        {

            lock (_refreshDevicePageLock)
            {
                using (var fipImage = new Bitmap(_htmlWindowWidth, _htmlWindowHeight))
                {
                    using (var graphics = Graphics.FromImage(fipImage))
                    {
                        if(RedrawPanelDelegate != null) RedrawPanelDelegate(graphics, mustRender);
                        /* HIER funktion einbetten wenn Bild neu gezeichnet werden soll? */
                    }
                    SendImageToFip(page, fipImage);
                }
            }
        }
        /*

                            var str = "";

                            if (CurrentCard < 0)
                            {
                                CurrentCard = 1;
                            }
                            else
                            if (CurrentCard > 1)
                            {
                                CurrentCard = 0;
                            }

                            if (mustRender)
                            {
                                try
                                {
                                    //lock (HWInfo.RefreshHWInfoLock)
                                    //{
                                    //    str =
                                    //        Engine.Razor.Run("hwinfo.cshtml", null, new
                                    //        {
                                    //            CurrentCard = CurrentCard,

                                    //            SensorCount = HWInfo.SensorData.Count,

                                    //            SensorData = HWInfo.SensorData.Values.ToList(),

                                    //            ChartImageDisplayWidth = ChartImageDisplayWidth,
                                    //            ChartImageDisplayHeight = ChartImageDisplayHeight

                                    //        });
                                    //}
                                }
                                catch (Exception ex)
                                {
                                    Program.Log.Error(ex);
                                }
                            }

                            graphics.Clear(Color.Black);

                            if (mustRender)
                            {
                                var measureData = HtmlRender.Measure(graphics, str, HtmlWindowUsableWidth, HelperFunctions.CssData, null, OnImageLoad);

                                _currentLcdHeight = (int)measureData.Height;
                            }

                            CheckLcdOffset();

                            if (_currentLcdHeight > 0)
                            {

                                if (mustRender)
                                {
                                    _htmlImage = HtmlRender.RenderToImage(str,
                                        new Size(HtmlWindowUsableWidth, _currentLcdHeight + 20), Color.Black, HelperFunctions.CssData,
                                        null, OnImageLoad);
                                }

                                if (_htmlImage != null)
                                {
                                    graphics.DrawImage(_htmlImage, new Rectangle(new Point(HtmlWindowXOffset, 0),
                                            new Size(HtmlWindowUsableWidth, _htmlWindowHeight + 20)),
                                        new Rectangle(new Point(0, _currentLcdYOffset),
                                            new Size(HtmlWindowUsableWidth, _htmlWindowHeight + 20)),
                                        GraphicsUnit.Pixel);
                                }
                            }

                            if (_currentLcdHeight > _htmlWindowHeight)
                            {
                                var scrollThumbHeight = _htmlWindowHeight / (double)_currentLcdHeight * ScrollBarHeight;
                                var scrollThumbYOffset = _currentLcdYOffset / (double)_currentLcdHeight * ScrollBarHeight;

                                graphics.DrawRectangle(_scrollPen, new Rectangle(new Point(_htmlWindowWidth - 9, 2),
                                                                   new Size(5, (int)ScrollBarHeight)));

                                graphics.FillRectangle(_scrollBrush, new Rectangle(new Point(_htmlWindowWidth - 9, 2 + (int)scrollThumbYOffset),
                                    new Size(5, 1 + (int)scrollThumbHeight)));

                            }



                            if (mustRender)
                            {
                                var cardcaptionstr =
                                    Engine.Razor.Run("cardcaption.cshtml", null, new
                                    {
                                        CurrentCard = CurrentCard
                                    });

                                _cardcaptionHtmlImage = HtmlRender.RenderToImage(cardcaptionstr,
                                    new Size(HtmlWindowUsableWidth, 26), Color.Black, HelperFunctions.CssData, null,
                                    null);
                            }

                            if (_cardcaptionHtmlImage != null)
                            {
                                graphics.DrawImage(_cardcaptionHtmlImage, HtmlWindowXOffset, 0);
                            }

                    // Knöpfe darstellen
                    if (_initOk)
                    {
                        for (uint i = 2; i <= 6; i++)
                        {
                            SetLed(i, false);
                        }

                        SetLed(5, true);
                        SetLed(6, true);

                    }
        /// <summary>
        /// Event handler for HTML Image loading, s called when images are loaded. The image is given back to the caller in the function
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">The HTML site where to embed the image into</param>
        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {

            try
            {
                var image = new Bitmap(ChartImageDisplayWidth, ChartImageDisplayHeight);

                using (var graphics = Graphics.FromImage(image))
                {
                    /* HIER eigene Funktion zum malen eines bildes wenn gewünscht? */
/*
                    //if (HWInfo.SensorTrends.ContainsKey(e.Src))
                    //{
                    //    graphics.DrawLines(_scrollPen, HWInfo.SensorTrends[e.Src].Read(ChartImageDisplayWidth, ChartImageDisplayHeight));
                    //}

                    //graphics.DrawRectangle(_whitePen,
                    //    new Rectangle(0, 0, ChartImageDisplayWidth - 1, ChartImageDisplayHeight - 1));

                    //graphics.DrawString(HWInfo.SensorTrends[e.Src].MaxV(), _drawFont, _whiteBrush, (float)1, (float)1);


                    //graphics.DrawString(HWInfo.SensorTrends[e.Src].MinV(), _drawFont, _whiteBrush, (float)1, (float)ChartImageDisplayHeight-17);
                }

                e.Callback(image);

                        }
                        catch
            {
                var image = new Bitmap(1, 1);

                e.Callback(image);
            }
        }
         */

    }
}
