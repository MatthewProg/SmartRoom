using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartRoom.Connectors
{
    public class TcpConnector : Interfaces.ITcpConnector
    {
        private readonly object _transmitLock = new object();
        private const int SERVER_BUFFER = 32;

        private TcpClient _client;
        private string _address;
        private int _port;

        private CancellationTokenSource _cts;
        private List<byte> _receiveBuffer;
        private List<byte> _sendBuffer;
        private bool _wasConnected;
        private Task _processTask;
        private Task _connectTask;

        public int ConnectionTimeout { get; set; }

        public event EventHandler<Events.ConnectionStatusEventArgs> ConnectionEvent;
        public event EventHandler<Events.ObjectEventArgs> DataReceivedEvent;

        public TcpConnector(string address = "127.0.0.1", int port = 23)
        {
            _address = address;
            _port = port;

            _sendBuffer = new List<byte>();
            _receiveBuffer = new List<byte>();
            _wasConnected = false;

            ConnectionTimeout = 15000;
            _client = new TcpClient();
        }

        public void Connect() => Connect(_address, _port);
        public void Connect(string address, int port)
        {
            if (_address == address && _port == port && _connectTask?.Status == TaskStatus.Running)
                return;

            Close();

            _address = address;
            _port = port;

            _cts = new CancellationTokenSource();
            _connectTask = Task.Run(() =>
            {
                var result = _client.BeginConnect(_address, _port, null, null);
                ConnectionEvent?.Invoke(null, new Events.ConnectionStatusEventArgs(Events.ConnectionStatusEventArgs.ConnectionStatus.CONNECTING));

                WaitHandle[] handles = new WaitHandle[] { _cts.Token.WaitHandle, result.AsyncWaitHandle };
                bool timeout = WaitHandle.WaitAny(handles, ConnectionTimeout) == 1;

                if (IsConnected)
                {
                    _client.EndConnect(result);
                    ConnectionEvent?.Invoke(null, new Events.ConnectionStatusEventArgs(Events.ConnectionStatusEventArgs.ConnectionStatus.CONNECTED));
                }
                else
                {
                    _client.Client.Close();
                    _client.Close();
                    _client = new TcpClient();
                    if (timeout)
                        ConnectionEvent?.Invoke(null, new Events.ConnectionStatusEventArgs(Events.ConnectionStatusEventArgs.ConnectionStatus.DISCONNECTED));
                    else
                        ConnectionEvent?.Invoke(null, new Events.ConnectionStatusEventArgs(Events.ConnectionStatusEventArgs.ConnectionStatus.FAILED));
                }
            }, _cts.Token);
        }

        public bool IsConnected
        {
            get
            {
                var connected = _client?.Client?.Connected ?? false;
                if (connected == false && _wasConnected == true)
                    ConnectionEvent?.Invoke(null, new Events.ConnectionStatusEventArgs(Events.ConnectionStatusEventArgs.ConnectionStatus.DISCONNECTED));
                _wasConnected = connected;
                return connected;
            }
        }
        public bool IsReady => (_processTask == null || _processTask.IsCompleted == true);

        public void Send(byte[] data)
        {
            lock (_transmitLock)
            {
                _sendBuffer.AddRange(data);
            }
            if (IsReady)
                _processTask = Task.Run(async () =>
                {
                    await Process();
                    DataReceivedEvent?.Invoke(null, new Events.ObjectEventArgs(new List<byte>(_receiveBuffer)));
                    _receiveBuffer.Clear();
                });
            else
                _processTask.ContinueWith(delegate
                {
                    _processTask = Task.Run(async () =>
                    {
                        await Process();
                        DataReceivedEvent?.Invoke(null, new Events.ObjectEventArgs(new List<byte>(_receiveBuffer)));
                        _receiveBuffer.Clear();
                    });
                });
        }

        private int GetMaxRange(List<byte> array, int limit)
        {
            if (array.Count <= limit)
                return array.Count;
            else
            {
                int currMax = 0;
                for(int i = 0; i<array.Count; )
                {
                    if (array[i] >> 7 == 1) i += 2; //SET PKG
                    else i++; //GET PKG

                    if (i <= limit)
                        currMax = i;
                    else
                        return currMax;
                }
                return currMax;
            }
        }

        private async Task Process()
        {
            if (IsConnected)
            {
                var stream = _client.GetStream();
                while (_sendBuffer.Count != 0)
                {
                    var range = GetMaxRange(_sendBuffer, SERVER_BUFFER - 1);
                    var toSend = _sendBuffer.GetRange(0, range);
                    toSend.Add(0b01100000); //EOT

                    lock (_transmitLock)
                    {
                        _sendBuffer.RemoveRange(0, range);
                    }

                    await stream.WriteAsync(toSend.ToArray(), 0, toSend.Count);

                    bool values = false;
                    bool text = false;
                    while (true)
                    {
                        try
                        {
                            int lastRead = stream.ReadByte();

                            if (values == false && text == false)
                            {
                                if (lastRead == 0b11100000)
                                    continue; //PING BYTE
                                else if (lastRead == 0b01100000) //EOT
                                    break;
                                else if ((lastRead >> 7) == 0)
                                    values = true;
                                else if ((lastRead >> 7) == 1)
                                    text = true;
                            }
                            else if (values == true)
                                values = false;
                            else if (text == true && lastRead == 0b00000011)
                                text = false;

                            _receiveBuffer.Add((byte)lastRead);
                        }
                        catch (TimeoutException) {; }
                    }
                }
            }
            else
                ConnectionEvent?.Invoke(null, new Events.ConnectionStatusEventArgs(Events.ConnectionStatusEventArgs.ConnectionStatus.DISCONNECTED));
        }

        public void Close()
        {
            if (_connectTask?.IsCompleted == false)
            {
                _cts.Cancel();
                _connectTask.Wait();
                _connectTask.Dispose();
            }

            if (IsConnected)
            {
                if (_processTask != null)
                    _processTask.Dispose();

                _client.Close();
                ConnectionEvent?.Invoke(null, new Events.ConnectionStatusEventArgs(Events.ConnectionStatusEventArgs.ConnectionStatus.DISCONNECTED));
                _client = new TcpClient();
            }

            lock (_transmitLock)
            {
                _sendBuffer.Clear();
                _receiveBuffer.Clear();
            }
        }

        ~TcpConnector()
        {
            Close();
        }
    }
}