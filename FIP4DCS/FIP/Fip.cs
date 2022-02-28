using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIP4DCS.FIP
{
    public class Fip
    {
        private List<FipPanel> _fipPanels = new List<FipPanel>();

        private DirectOutputClass.DeviceCallback _deviceCallback;
        private DirectOutputClass.EnumerateCallback _enumerateCallback;
        public bool InitOk;
        private IntPtr _device = IntPtr.Zero;

        //private const String DirectOutputKey = "SOFTWARE\\Saitek\\DirectOutput";

        private const string DirectOutputKey = "SOFTWARE\\Logitech\\DirectOutput";

        /// <summary>
        /// Initilizes the FIP with 1 Panel which has 1 Page on Index 0, Pagenumber 0.
        /// It's a 64bit DLL and Service which is checked in the Logitec registry
        /// </summary>
        /// <param name="appName">Identifier for the application in the FIP</param>
        /// <returns></returns>
        public bool Initialize(string appName)
        {
            InitOk = false;
            try
            {
                _deviceCallback = DeviceCallback;
                _enumerateCallback = EnumerateCallback;

                var key = Registry.LocalMachine.OpenSubKey(DirectOutputKey);

                var value = key?.GetValue("DirectOutput");
                if (value is string)
                {

                    var retVal = DirectOutputClass.Initialize(appName);
                    if (retVal != ReturnValues.S_OK)
                    {
                        Program.Log.Error("FIPHandler failed to init DirectOutputClass. " + retVal);
                        return false;
                    }

                    DirectOutputClass.RegisterDeviceCallback(_deviceCallback);

                    retVal = DirectOutputClass.Enumerate(_enumerateCallback);
                    if (retVal != ReturnValues.S_OK)
                    {
                        Program.Log.Error("FIPHandler failed to Enumerate DirectOutputClass. " + retVal);
                        return false;
                    }
                    InitOk = true;
                }
            }
            catch (Exception ex)
            {
                Program.Log.Error(ex);

                return false;
            }
            return InitOk;
        }

        public void SetCheckButtonsHandler(FipPanel.CheckButtons checkButtons, int panelIndex)
        {
            if (panelIndex > -1 && panelIndex < _fipPanels.Count)
            {
                _fipPanels[panelIndex].CheckButtonsDelegate = checkButtons;
            }
        }

        public void SetRedrawPanelDelegate(FipPanel.RedrawPanel redrawPanel, int panelIndex)
        {
            if (panelIndex > -1 && panelIndex < _fipPanels.Count)
            {
                _fipPanels[panelIndex].RedrawPanelDelegate = redrawPanel;
            }
        }

        /// <summary>
        /// Shutdown and deinitialize of all Pages and the FIP
        /// </summary>
        public void Close()
        {
            try
            {
                foreach (var fipPanel in _fipPanels)
                {
                    fipPanel.Shutdown();
                }
                _fipPanels.Clear();
                if (InitOk)
                {
                    //No need to deinit if init never worked. (e.g. missing Saitek Drivers)
                    DirectOutputClass.Deinitialize();
                }
            }
            catch (Exception ex)
            {
                Program.Log.Error(ex);
            }
            InitOk = false;
        }

        /// <summary>
        /// Refreshes all device pages
        /// </summary>
        public void RefreshAllPages()
        {
            for (var index = 0; index < _fipPanels.Count; index++)
            {
                var fipPanel = _fipPanels[index];

                fipPanel.RefreshDevicePage();
            }
        }

        /// <summary>
        /// Refreshes a single device page
        /// </summary>
        /// <param name="index">0 based index of the page. Not the page-name-index.</param>
        public void RefreshPage(int index)
        {
            if (index < _fipPanels.Count && _fipPanels.Count >= 0)
            {
                _fipPanels[index].RefreshDevicePage();
            }
        }

        /// <summary>
        /// Checks if the device is the DirectOutput Logitec/Saitec Flight Instruments Display
        /// </summary>
        /// <param name="device">Reference to the DirectOutput device</param>
        /// <returns>true if the device is the aforementioned one.</returns>
        private bool IsFipDevice(IntPtr device)
        {
            var mGuid = Guid.Empty;

            DirectOutputClass.GetDeviceType(device, ref mGuid);

            return string.Compare(mGuid.ToString(), "3E083CD8-6A37-4A58-80A8-3D6A2C07513E", true, CultureInfo.InvariantCulture) == 0;
        }

        /// <summary>
        /// Enumerates the panels in the FIP
        /// </summary>
        /// <param name="device"></param>
        /// <param name="context"></param>
        private void EnumerateCallback(IntPtr device, IntPtr context)
        {
            try
            {
                var mGuid = Guid.Empty;

                DirectOutputClass.GetDeviceType(device, ref mGuid);

                Program.Log.Info($"Adding new DirectOutput device {device} of type: {mGuid.ToString()}");

                //Called initially when enumerating FIPs.

                if (!IsFipDevice(device))
                {
                    return;
                }
                var fipPanel = new FipPanel(device);
                _fipPanels.Add(fipPanel);
                fipPanel.Initalize();
            }
            catch (Exception ex)
            {
                Program.Log.Error(ex);
            }
        }

        /// <summary>
        /// Called whenever a DirectOutput device is added or removed from the system.
        /// </summary>
        /// <param name="device">Reference to the DirectOutput device</param>
        /// <param name="added">true if it was added, false if it was removed</param>
        /// <param name="context">unused</param>
        private void DeviceCallback(IntPtr device, bool added, IntPtr context)
        {
            try
            {
                Program.Log.Info("DeviceCallback(): 0x" + device.ToString("x") + (added ? " Added" : " Removed"));

                if (!IsFipDevice(device))
                {
                    return;
                }

                if (!added && _fipPanels.Count == 0)
                {
                    return;
                }

                var i = _fipPanels.Count - 1;
                var found = false;
                do
                {
                    if (_fipPanels[i].FipDevicePointer == device)
                    {
                        found = true;
                        var fipPanel = _fipPanels[i];
                        if (!added)
                        {
                            fipPanel.Shutdown();
                            _fipPanels.Remove(fipPanel);
                        }
                    }
                    i--;
                } while (i >= 0);

                if (added && !found)
                {
                    Program.Log.Info("DeviceCallback() Spawning FipPanel. " + device);
                    _device = device;
                    var fipPanel = new FipPanel(device);
                    _fipPanels.Add(fipPanel);
                    fipPanel.Initalize();
                }
            }
            catch (Exception ex)
            {
                Program.Log.Error(ex);
            }
        }

        public int PanelCount { get { return _fipPanels.Count; } }

        public FipPanel GetPanel(int index)
        {
            if (index > -1 && index < _fipPanels.Count)
            {
                return _fipPanels[index];
            }
            return null;
        }

        public int AddPanel()
        {
            var fipPanel = new FipPanel(_device);
            _fipPanels.Add(fipPanel);
            fipPanel.Initalize();
            return _fipPanels.Count() - 1;
        }

        public void RemovePanel(int index)
        {
            if (index > -1 && index < _fipPanels.Count)
            {
                _fipPanels[index].Shutdown();
                _fipPanels.RemoveAt(index);
            }

        }

        public void AddPageToPanel(uint pageNumber, bool setActive, int panelIndex)
        {
            if (panelIndex > -1 && panelIndex < _fipPanels.Count)
            {
                _fipPanels[panelIndex].AddPage(pageNumber, setActive);
            }
        }

        public uint GetPageNumberByIndex(int pageIndex, int panelIndex)
        {
            if (panelIndex > -1 && panelIndex < _fipPanels.Count)
            {
                _fipPanels[panelIndex].PageNumber(pageIndex);
            }
            return 0;
        }


    }
}
