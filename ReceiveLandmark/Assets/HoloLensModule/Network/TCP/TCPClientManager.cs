﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.IO;
using System.Diagnostics;
#elif UNITY_EDITOR || UNITY_STANDALONE
using System.Net.Sockets;
using System.Threading;
#endif

namespace HoloLensModule.Network
{
    public class TCPClientManager
    {
        public delegate void ListenerMessageEventHandler(string ms);
        public ListenerMessageEventHandler ListenerMessageEvent;

        public delegate void ListenerByteEventHandler(byte[] data);
        public ListenerByteEventHandler ListenerByteEvent;

#if WINDOWS_UWP
        private StreamWriter writer = null;
        private Task writetask = null;
#elif UNITY_EDITOR || UNITY_STANDALONE
        private Thread sendthread = null;
        private NetworkStream stream = null;
#endif
        private bool isActiveThread = true;

        public TCPClientManager() { }

        public TCPClientManager(string ipaddress, int port)
        {
            ConnectClient(ipaddress, port);
        }

        public void ConnectClient(string ipaddress, int port)
        {
#if WINDOWS_UWP
            Task.Run(async() =>
            {
                StreamSocket socket = new StreamSocket();
                await socket.ConnectAsync(new HostName(ipaddress), port.ToString());
                writer = new StreamWriter(socket.OutputStream.AsStreamForWrite());
                StreamReader reader = new StreamReader(socket.InputStream.AsStreamForRead());
                byte[] bytes = new byte[65536];
                while (isActiveThread)
                {
                    try
                    {
                        int num = await reader.BaseStream.ReadAsync(bytes, 0, bytes.Length);
                        if (num > 0)
                        {
                            byte[] data = new byte[num];
                            Array.Copy(bytes, 0, data, 0, num);
                            if (ListenerMessageEvent != null) ListenerMessageEvent(Encoding.UTF8.GetString(data));
                            if (ListenerByteEvent != null) ListenerByteEvent(data);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Write(e);
                    }
                }
                socket.Dispose();
                if (writer != null)
                {
                    writer.Dispose();
                }
                writer = null;
            });
#elif UNITY_EDITOR || UNITY_STANDALONE
            TcpClient tcpclient = new TcpClient();
            tcpclient.BeginConnect(ipaddress, port, ConnectCallback, tcpclient);
#endif
        }

        public bool SendMessage(string ms)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(ms);
            return SendMessage(bytes);
        }

        public bool SendMessage(byte[] data)
        {
#if WINDOWS_UWP
            if (writetask == null || writetask.IsCompleted == true)
            {
                if (writer != null)
                {
                    writetask = Task.Run(async () =>
                    {
                        await writer.BaseStream.WriteAsync(data, 0, data.Length);
                        await writer.FlushAsync();
                    });
                }
            }
#elif UNITY_EDITOR || UNITY_STANDALONE
            if (sendthread == null || sendthread.ThreadState != ThreadState.Running)
            {
                if (stream != null)
                {
                    sendthread = new Thread(() => { stream.Write(data, 0, data.Length); });
                    sendthread.Start();
                    return true;
                }
            }
#endif
            return false;
        }


        public void DisConnectClient()
        {
#if WINDOWS_UWP
            if (writer!=null)
            {
                writer.Dispose();
            }
            writer = null;
#elif UNITY_EDITOR || UNITY_STANDALONE
            isActiveThread = false;
            if (sendthread != null)
            {
                sendthread.Abort();
                sendthread = null;
            }
            stream = null;
#endif
        }
#if WINDOWS_UWP
#elif UNITY_EDITOR || UNITY_STANDALONE
        private void ConnectCallback(IAsyncResult ar)
        {
            TcpClient tcp = (TcpClient)ar.AsyncState;
            tcp.EndConnect(ar);
            tcp.ReceiveTimeout = 100;
            stream = tcp.GetStream();
            byte[] bytes = new byte[tcp.ReceiveBufferSize];
            while (isActiveThread)
            {
                try
                {
                    int num = stream.Read(bytes, 0, bytes.Length);
                    if (num > 0)
                    {
                        byte[] data = new byte[num];
                        Array.Copy(bytes, 0, data, 0, num);
                        if (ListenerMessageEvent != null) ListenerMessageEvent(Encoding.UTF8.GetString(data));
                        if (ListenerByteEvent != null) ListenerByteEvent(data);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
            }
            stream.Close();
            stream = null;
            tcp.Close();
        }
#endif
    }
}
