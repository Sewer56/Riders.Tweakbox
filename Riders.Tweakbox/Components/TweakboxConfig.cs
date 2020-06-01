using System;
using System.Collections.Generic;
using Riders.Tweakbox.Components.GearEditor;
using Riders.Tweakbox.Definitions.Interfaces;

namespace Riders.Tweakbox.Components
{
    /// <summary>
    /// Stores a large overarching config file that contains all configurations supported by Tweakbox.
    /// </summary>
    public class TweakboxConfig : IConfiguration
    {
        public List<IConfiguration> Configurations = new List<IConfiguration>()
        {
            // DO NOT REARRANGE, THIS IS ORDER OF SERIALIZATION.
            new GearEditorConfig(),
        };

        /// <inheritdoc />
        public void Apply()
        {
            foreach (var conf in Configurations)
                conf.Apply();
        }

        /// <inheritdoc />
        public byte[] ToBytes()
        {
            var bytes = new List<byte>();
            foreach (var conf in Configurations) 
                bytes.AddRange(conf.ToBytes());

            return bytes.ToArray();
        }

        /// <inheritdoc />
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            foreach (var conf in Configurations)
            {
                // If there are no bytes left, user imported
                // config from a newer version of Tweakbox
                // into an older version.
                if (bytes.Length <= 0)
                    break;

                bytes = conf.FromBytes(bytes);
            }

            return bytes;
        }
    }
}
