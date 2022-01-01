using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TgenSerializer
{
    /// <summary>An interface for classes that control their own Serialize and Deserialize operations.
    /// Must have a default (No parameters) constructor!<summary>
    public interface ISerializable
    {
        void Serialize(DataWriter writer);

        void Deserialize(DataReader reader);
    }
}
