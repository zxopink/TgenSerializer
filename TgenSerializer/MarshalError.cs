using System;
using System.Collections.Generic;
using System.Text;

namespace TgenSerializer
{
    public enum MarshalError
    {
        ///<summary>Size of data is too large</summary>
        TooLarge,
        ///<summary>Size of data is Negative</summary>
        NegativeSize,
        ///<summary>Object is NonSerializable</summary>
        NonSerializable,
        ///<summary>Stream data is false (Could not find next operator during construction)</summary>
        SyntaxError,
        InternalError,
    }
}
