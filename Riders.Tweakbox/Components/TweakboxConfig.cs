using System;
using System.Collections.Generic;
using Riders.Tweakbox.Components.Editors.Gear;
using Riders.Tweakbox.Components.Editors.Physics;
using Riders.Tweakbox.Components.Fixes;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components
{
    /// <summary>
    /// Stores a large overarching config file that contains all configurations supported by Tweakbox.
    /// </summary>
    public class TweakboxConfig : IConfiguration
    {
        public List<IConfiguration> GetConfigurations => new List<IConfiguration>()
        {
            // DO NOT REARRANGE, THIS IS ORDER OF SERIALIZATION.
            GearEditorConfig.FromGame(),
            PhysicsEditorConfig.FromGame(),
            IoC.GetConstant<FixesEditorConfig>()
        };

        /// <inheritdoc />
        public void Apply()
        {
            foreach (var conf in GetConfigurations)
                conf.Apply();
        }

        /// <inheritdoc />
        public byte[] ToBytes()
        {
            var bytes = new List<byte>();
            foreach (var conf in GetConfigurations) 
                bytes.AddRange(conf.ToBytes());

            return bytes.ToArray();
        }

        /// <inheritdoc />
        public Span<byte> FromBytes(Span<byte> bytes)
        {
            foreach (var conf in GetConfigurations)
            {
                // If there are no bytes left, user imported
                // config from a newer version of Tweakbox
                // into an older version.
                if (bytes.Length <= 0)
                    break;

                bytes = conf.FromBytes(bytes);
                conf.Apply();
            }

            return bytes;
        }

        public IConfiguration GetCurrent() => throw new NotImplementedException();
        public IConfiguration GetDefault() => throw new NotImplementedException();
    }
}
