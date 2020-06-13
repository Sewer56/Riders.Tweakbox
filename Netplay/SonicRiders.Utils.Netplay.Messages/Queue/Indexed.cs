using System;
using System.Collections.Generic;
using System.Text;

namespace Riders.Netplay.Messages.Queue
{
    /// <summary>
    /// A tuple that combines a player index and a given data item together.
    /// </summary>
    public class Indexed<T>
    {
        public byte Index;
        public T Data;

        public Indexed(byte index, T data)
        {
            Index = index;
            Data = data;
        }
    }
}
