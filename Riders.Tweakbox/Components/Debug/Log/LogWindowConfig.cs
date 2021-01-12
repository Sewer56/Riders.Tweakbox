using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Memory;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Debug.Log
{
    public class LogWindowConfig : IConfiguration
    {
        /// <inheritdoc />
        public byte[] ToBytes() => Json.SerializeStruct(ref Misc.Log.EnabledCategories);

        public LogCategory Data = Misc.Log.DefaultCategories;

        /// <inheritdoc />
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            Data = Json.DeserializeStruct<LogCategory>(bytes);
            return bytes.Slice(Struct.GetSize<LogCategory>());
        }

        /// <inheritdoc />
        public void Apply() => Misc.Log.EnabledCategories = Data;
        public IConfiguration GetCurrent() => this;
        public IConfiguration GetDefault() => new LogWindowConfig();
    }
}
