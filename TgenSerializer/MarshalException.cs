using System;
using System.Collections.Generic;
using System.Text;

namespace TgenSerializer
{
    public class MarshalException : SystemException
    {
        public MarshalError Error { private set; get; }
        private string description;
        public override string Message => description;
        public MarshalException(MarshalError error)
        {
            Error = error;
            description = GetMessage(error);
        }

        public MarshalException(MarshalError error, string message)
        {
            Error = error;
            description = message;
        }

        private string GetMessage(MarshalError error)
        {
            switch (error)
            {
                case MarshalError.TooLarge:
                    return "Stream data is too large to be read";
                case MarshalError.NegativeSize:
                    return "Stream data length is negative (possibly corrupted data)";
                case MarshalError.NonSerializable:
                    return "Object is NonSerializable";
                default:
                    return null;
            }
        }
    }
}
