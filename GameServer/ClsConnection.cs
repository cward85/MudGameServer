using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GameServer
{
    class ClsConnection
    {
        private TcpListener m_objConnectionListener;
        private TcpClient m_objCurrentClient;
        private RichTextBox m_objOutputText;
        List<Tuple<Thread, TcpClient>> m_objThreadConnectionList;        

        public ClsConnection()
        {          
            m_objThreadConnectionList = new List<Tuple<Thread, TcpClient>>();
            m_objConnectionListener = new TcpListener(IPAddress.Parse("192.168.1.3"), 4000);            
        }

        public void StartServer(RichTextBox p_objOutputText)
        {
            m_objOutputText = p_objOutputText;
            WriteOutputText("Starting Server");
            Thread objThread = new Thread(new ThreadStart(ListenForConnections));
            objThread.Start();
            m_objThreadConnectionList.Add(new Tuple<Thread, TcpClient>(objThread, null));
        }

        public void CloseConnections()
        {
            m_objConnectionListener.Stop();
            foreach(Tuple<Thread, TcpClient> objThread in m_objThreadConnectionList)
            {
                if (objThread.Item2 != null)
                {
                    WriteToClient("Exit", objThread.Item2.GetStream());
                    objThread.Item2.Close();
                    Thread.Sleep(200);
                }

                objThread.Item1.Abort();
                
            }
        }

        private void ListenForConnections()
        {
            m_objConnectionListener.Start();

            while (true)
            {
                while (!m_objConnectionListener.Pending())
                {
                    Thread.Sleep(500);
                }
                
                m_objCurrentClient = m_objConnectionListener.AcceptTcpClient();
                Thread objClientConnection = new Thread(() => HandleConnection(m_objCurrentClient));                
                objClientConnection.Start();

                m_objThreadConnectionList.Add(new Tuple<Thread, TcpClient>(objClientConnection, m_objCurrentClient));
            }
        }

        private bool DoAction(string p_strOutput)
        {
            if (p_strOutput == "Exit")
            {
                m_objCurrentClient.Close();

                return false;
            }

            WriteOutputText(p_strOutput);                       

            return true;
        }

        private void HandleConnection(TcpClient p_objCurrentClient)
        {
            try
            {
                WriteOutputText("Client Connected from: " + p_objCurrentClient.Client.LocalEndPoint.ToString());
                using (NetworkStream objStreamInput = p_objCurrentClient.GetStream())
                {
                    while (p_objCurrentClient.Connected)
                    {
                        byte[] byteArray = new byte[1024];
                        string strOutput = string.Empty;
                        do
                        {
                            if (objStreamInput.Read(byteArray, 0, byteArray.Length) > 0)
                            {
                                char[] chars = new char[byteArray.Length / sizeof(char)];
                                System.Buffer.BlockCopy(byteArray, 0, chars, 0, byteArray.Length);
                                strOutput += new string(chars);
                            }
                        }
                        while (objStreamInput.DataAvailable);

                        strOutput = strOutput.Replace("\0", string.Empty);

                        if (!DoAction(strOutput))
                        {
                            break;
                        }
                        else
                        {
                            WriteToClient(strOutput, objStreamInput);
                        }
                    }
                }

                WriteOutputText("Client was disconnected");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Server: " + ex.Message);
            }
        }

        private byte[] ConvertStringToBytes(string p_strInput)
        {
            byte[] bytes = new byte[p_strInput.Length * sizeof(char)];
            System.Buffer.BlockCopy(p_strInput.ToCharArray(), 0, bytes, 0, bytes.Length);

            return bytes;
        }

        private void WriteToClient(string p_strInput, NetworkStream p_objStream)
        {
            byte[] bytes = ConvertStringToBytes(p_strInput);
            p_objStream.Write(bytes, 0, bytes.Length);
        }

        delegate void AppendRichTextInvoker(string p_strText);
        private void WriteOutputText(string p_strText)
        {
            if (this.m_objOutputText.Dispatcher.Thread == Thread.CurrentThread)
            {
                m_objOutputText.AppendText(p_strText.Trim() + Environment.NewLine );
                m_objOutputText.ScrollToEnd();
            }
            else
            {
                this.m_objOutputText.Dispatcher.BeginInvoke(new Action(() => WriteOutputText(p_strText)));
            }
        }
    }
}

