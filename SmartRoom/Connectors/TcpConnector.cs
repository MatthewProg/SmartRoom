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
using System.Threading.Tasks;

namespace SmartRoom.Connectors
{
    public class TcpConnector
    {
        private const int SERVER_BUFFER = 32;

        private TcpClient _client;
        private string _address;
        private int _port;

        private List<byte> _sendBuffer;
        private List<byte> _receiveBuffer;
        private Task _processTask;

        public event EventHandler DataReceivedEvent;

        public TcpConnector(string address, int port)
        {
            _address = address;
            _port = port;

            _sendBuffer = new List<byte>();
            _receiveBuffer = new List<byte>();

            _client = new TcpClient();
        }

        public void Connect() => Connect(_address, _port);
        public void Connect(string address, int port)
        {
            if (IsConnected)
                Close();

            Task.Run(async () => await _client.ConnectAsync(address, port));
        }

        public bool IsConnected => _client?.Connected ?? false;
        public bool IsReady => (_processTask == null || _processTask.IsCompleted == true);

        public void Send(byte[] data)
        {
            _sendBuffer.AddRange(data);
            if (IsReady)
                _processTask = Task.Run(async () => { await Process(); DataReceivedEvent?.Invoke(_receiveBuffer, null); });
            else
                _processTask.ContinueWith(delegate {
                    _processTask = Task.Run(async () => { await Process(); DataReceivedEvent?.Invoke(_receiveBuffer, null); });
                });
        }

        private async Task Process()
        {
            if(IsConnected)
            {
                var stream = _client.GetStream();
                while (_sendBuffer.Count != 0)
                {
                    var range = (_sendBuffer.Count < SERVER_BUFFER ? _sendBuffer.Count : SERVER_BUFFER);
                    var toSend = _sendBuffer.GetRange(0, range);
                    _sendBuffer.RemoveRange(0, range);

                    await stream.WriteAsync(toSend.ToArray(), 0, toSend.Count);

                    while (true)
                    {
                        try
                        {
                            int lastRead = stream.ReadByte();
                            if (lastRead == -1) break;

                            _receiveBuffer.Add((byte)lastRead);
                        }
                        catch (TimeoutException) { ; }
                    }
                }
            }
        }

        public void Close()
        {
            if (IsConnected)
            {
                if (_processTask != null)
                    _processTask.Dispose();
                _client.Close();
            }
        }

        ~TcpConnector()
        {
            Close();
        }
    }
}