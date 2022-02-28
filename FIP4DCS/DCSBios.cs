using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DCSBios
{
    public class DCSBios
    {
        public enum DcsBiosType
        {
            Byte, String, Int
        }
        public struct MiniBios
        {
            public int address;
            public string text;
            public int value;
        }

        public class DcsBiosData
        {
            public int address = 0;
            public int count = 0;
            public DcsBiosType type = DcsBiosType.Byte;
            public Byte[] byteData = new byte[0];
            public int intData { 
                get { 
                    if (count == 2) { return (byteData[1] * 256 + byteData[0]); } 
                    else if (count == 4) { return byteData[3] * 256 * 256 * 256 + byteData[2] * 256 * 256 + byteData[1] * 256 + byteData[0]; }
                    else return 0; 
                } 
            }
            public string strData { get { return Encoding.ASCII.GetString(byteData, 0, byteData.Length); } }

            public MiniBios asMiniBios()
            {
                MiniBios miniBios = new MiniBios();
                miniBios.address = address;
                miniBios.text = strData;
                miniBios.value = intData;
                return miniBios;
            }
        }
        
        public Dictionary<int, DcsBiosData> dcsBios = new Dictionary<int, DcsBiosData>();
        public Dictionary<int, MiniBios> miniBios = new Dictionary<int, MiniBios>();

        public void ParseDcsBios(Byte[] data)
        {
            uint state = 0; // 0=finished, 1=address, 2=Count, 3=Data
            uint subState = 0; //0=HighByte, 1=LowByte
            DcsBiosData dcs = new DcsBiosData();
            for (int i = 0; i < data.Length; i++)
            {
                if (state == 0 && data[i] == 0x55 && data[i + 1] == 0x55 && data[i + 2] == 0x55 && data[i + 3] == 0x55) { i += 3; continue; }
                if (state == 3 && subState == dcs.count)
                {
                    dcsBios[dcs.address] = dcs; state = 0; subState = 0;
                }
                if (state == 0) { dcs = new DcsBiosData(); state = 1; }
                if (state == 1 && subState == 0) { dcs.address = data[i]; subState = 1; continue; }
                if (state == 1 && subState == 1) { dcs.address += (data[i] * 256); state = 2; subState = 0; continue; }
                if (state == 2 && subState == 0) { dcs.count = data[i]; subState = 1; continue; }
                if (state == 2 && subState == 1) { dcs.count += (data[i] * 256); state = 3; subState = 0; continue; }
                if (state == 3 && subState == 0) { dcs.byteData = new byte[dcs.count]; dcs.byteData[0] = data[i]; subState++; continue; }
                if (state == 3 && subState > 0) { dcs.byteData[subState] = data[i]; subState++; continue; }
            }
        }

        public void PrepareList()
        {
            if (dcsBios.Count == 0)
            {
                dcsBios[0] = new DcsBiosData();
                miniBios[0] = dcsBios[0].asMiniBios();
            }
            else
            {
                try
                {
                    foreach (KeyValuePair<int, DcsBiosData> dcs in dcsBios)
                    {
                            miniBios[dcs.Value.address] = dcs.Value.asMiniBios();
                    }
                }
                catch { }
            }
        }

        Thread t;
        bool startUdP = true;

        private readonly string listenIP;
        private readonly int listenPort;
        private readonly string sendIP;
        private readonly int sendPort;

        public DCSBios(string listenIP = "239.255.50.10", int listenPort = 5010, string sendIP = "127.0.0.1", int sendPort = 7778)
        {
            this.listenIP = listenIP;
            this.listenPort = listenPort;
            this.sendIP = sendIP;
            this.sendPort = sendPort;
        }

        public void StartDcsUdp()
        {
            startUdP = true;
            t = new Thread(DcsUdpListener);
            t.Start();
        }

        public void StopDcsUdp()
        {
            startUdP = false;
            if (t != null)
            {
                t.Abort();
            }
        }

        public void Send(string data)
        {
            UdpClient udpClient = new UdpClient();
            Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
            udpClient.SendAsync(sendBytes, sendBytes.Length, sendIP, sendPort);
        }

        public void DcsUdpListener()
        {
            try
            {
                UdpClient client = new UdpClient();

                client.ExclusiveAddressUse = false;
                IPEndPoint localEp = new IPEndPoint(IPAddress.Any, listenPort);

                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.ExclusiveAddressUse = false;

                client.Client.Bind(localEp);

                IPAddress multicastaddress = IPAddress.Parse(listenIP);
                client.JoinMulticastGroup(multicastaddress);

                //Console.WriteLine("Listening this will never quit so you will need to ctrl-c it");

                Byte[] buffer = new Byte[1];
                bool added = false;
                dcsBios.Clear();

                while (startUdP)
                {
                    Byte[] data = client.Receive(ref localEp);
                    if (data[0] == 0x55 && data[1] == 0x55 && data[2] == 0x55 && data[3] == 0x55)
                    {
                        if (added) ParseDcsBios(buffer);
                        buffer = data;
                        added = false;
                        ParseDcsBios(buffer);
                    }
                    else
                    {
                        buffer.Concat(data);
                        added = true;
                    }
                    //Console.Write(".");// Encoding.ASCII.GetString(data, 0, data.Length));
                }
            }
            catch (SocketException e)
            {
            }
            finally
            {
            }
        }

    }
}
