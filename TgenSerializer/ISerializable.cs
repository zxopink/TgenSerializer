using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    /// <summary>An interface for classes that control their own Serialize and Deserialize operations.
    /// Must have a parameterless constructor<summary>
    public interface ISerializable
    {
        Bytes Serialize();

        void Deserialize(Bytes data);
    }
}
