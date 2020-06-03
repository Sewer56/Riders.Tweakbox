using System;
using System.Collections.Generic;
using System.Text;

namespace Riders.Netplay.Messages.Reliable.Structs
{
    public struct Seed
    {
        public int Value;
        public Seed(int seed) => Value = seed;
    }
}
