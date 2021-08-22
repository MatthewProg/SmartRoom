using System;

namespace SmartRoom.Events
{
    public class PopupEventArgs : EventArgs
    {
        public bool HasResult { get; private set; }
        public object Result { get; private set; }

        public PopupEventArgs() : this(false, null) { ; }
        public PopupEventArgs(bool hasResult, object result)
        {
            HasResult = hasResult;
            Result = result;
        } 
    }
}