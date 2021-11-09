using SmartRoom.Events;
using System;

namespace SmartRoom.Interfaces
{
    public interface ITcpConnector
    {
        bool IsConnected { get; }
        bool IsReady { get; }

        event EventHandler<ConnectionStatusEventArgs> ConnectionEvent;
        event EventHandler<Events.ObjectEventArgs> DataReceivedEvent;

        void Close();
        void Connect();
        void Connect(string address, int port);
        void Send(byte[] data);
    }
}