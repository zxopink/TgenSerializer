using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    public class MarshalException : SerializationException
    {
        public MarshalError Error { private set; get; }
        private string description;
        public override string Message => description;

        public Type MarshalledType => throw new NotSupportedException();
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

        public MarshalException(MarshalError error, string message, Exception innerException) : base(message, innerException)
        {
            Error = error;
            description = message;
        }

        public MarshalException(Exception innerException) : base(innerException.Message, innerException)
        {
            Error = MarshalError.InternalError;
            description = innerException.Message;
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
                case MarshalError.SyntaxError:
                    return "Syntax Format is wrong";
                default:
                    return null;
            }
        }
    }
}
