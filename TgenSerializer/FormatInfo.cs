using System;
using System.Collections.Generic;
using System.Text;

namespace TgenSerializer
{
    [Serializable]
    class FormatInfo
    {
        public Type type;
        byte[] objInfo;
        public Bytes data;

        [NonSerialized]
        int index;
        public FormatInfo()
        {
            type = null;
            index = 0;
        }

        public void SetType(Type type) => this.type = type;

        public void AddInfo(object obj)
        {
            data += obj;
        }

        public T GetT<T>()
        {
            return (T)Bytes.ByteToPrimitive(typeof(T), objInfo, index);
        }
    }
}
