using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Definitions.Serializers.Json;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
namespace Riders.Tweakbox.Configs.Json;

public abstract class JsonConfigBase<TParent, TConfig> : IConfiguration where TParent : new() where TConfig : new()
{
    private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        IncludeFields = true,
        WriteIndented = true,
        IgnoreNullValues = true,
        Converters = { new TextInputJsonConverter(),  }
    };

    /// <summary>
    /// Gets a list of all properties for the internal configuration.
    /// </summary>
    public static readonly IReadOnlyList<string> Properties = Reflection.GetAllInstanceFieldNames(typeof(TConfig));

    protected JsonConfigBase() { }

    /// <inheritdoc />
    public Action ConfigUpdated { get; set; }

    /// <summary>
    /// The data of the current config.
    /// </summary>
    public TConfig Data = new TConfig();

    /// <inheritdoc />
    public virtual byte[] ToBytes() => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Data, SerializerOptions));

    /// <inheritdoc />
    public virtual void FromBytes(Span<byte> bytes)
    {
        var result = JsonSerializer.Deserialize<TConfig>(bytes, SerializerOptions);
        ConfigUpdated?.Invoke();
        Mapping.Mapper.From(result).AdaptTo(Data);
    }

    /// <inheritdoc />
    public virtual void Apply()
    {
        if (Data is INotifyPropertyUpdated ex)
            foreach (var prop in Properties)
                ex.RaisePropertyUpdated(prop);
    }

    /// <inheritdoc />
    public virtual IConfiguration GetCurrent() => this;

    /// <inheritdoc />
    public virtual IConfiguration GetDefault() => (IConfiguration)new TParent();
}
