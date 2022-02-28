using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIP4DCS
{
    public class ModularFip
    {
        public static FIP.Fip fip = new FIP.Fip();
        private bool _initOk = false;
        private string _appname = "";

        public ModularFip(string appname)
        {
            _appname = appname;
            //_initOk = fip.Initialize(appname);
        }

        public bool Initialize(string appname = "")
        {
            if (appname != "") _appname = appname;
            if (!_initOk) _initOk = fip.Initialize(_appname);
            return _initOk;
        }

        public void Close()
        {
            fip.Close();
            _initOk = fip.InitOk;
        }

        public bool InitOk { get { return _initOk;} }

        public void RefreshAllPages()
        {
            fip.RefreshAllPages();
        }

        public void SetRedrawDelegate(FIP.FipPanel.RedrawPanel redrawPanel, int index){
            fip.SetRedrawPanelDelegate(redrawPanel, index);
        }

        public void SetButtonsDelegate(FIP.FipPanel.CheckButtons checkButtons , int index)
        {
            fip.SetCheckButtonsHandler(checkButtons, index);
        }
    }
}
