using System.Runtime.CompilerServices;
using System.Text;
namespace Riders.Tweakbox.Misc.Log;

[InterpolatedStringHandler]
public ref struct LoggerInterpolatedStringHandler
{
    private const int _formattedAverageLength = 10;

    /// <summary>
    /// The internal string builder.
    /// </summary>
    private StringBuilder _builder;

    public LoggerInterpolatedStringHandler(int literalLength, int formattedCount, Logger logger, out bool handlerIsValid)
    {
        handlerIsValid = logger.IsEnabled(ListenerType.Any);
        _builder = new StringBuilder(literalLength + (formattedCount * _formattedAverageLength));
    }

    public bool AppendLiteral(string s)
    {
        // Store and format part as required
        _builder.Append(s);
        return true;
    }

    public bool AppendFormatted<T>(T t) => AppendLiteral(t.ToString());
    public bool AppendFormatted(long t, string? format) => AppendLiteral(t.ToString(format));

    /// <summary>
    /// Retrieves the text associated with this <see cref="LoggerInterpolatedStringHandler"/>
    /// </summary>
    public override string ToString() => _builder.ToString();
}