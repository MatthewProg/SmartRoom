using System;

namespace SmartRoom.Interfaces
{
    public interface ITcpConnector
    {
        bool IsConnected { get; }
        bool IsReady { get; }

        event EventHandler<Events.ObjectEventArgs> DataReceivedEvent;

        void Close();
        void Connect();
        void Connect(string address, int port);
        void Send(byte[] data);
    }
}