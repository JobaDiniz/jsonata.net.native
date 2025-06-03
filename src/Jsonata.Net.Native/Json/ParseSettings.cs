namespace Jsonata.Net.Native.Json;

public sealed class ParseSettings
{
    internal static readonly ParseSettings DefaultSettings = new ParseSettings()
    {
        AllowTrailingComma = true,
    };

    private static readonly ParseSettings s_strictSettings = new ParseSettings()
    {
        AllowTrailingComma = false,
    };

    public static ParseSettings GetDefault()
    {
        return DefaultSettings.Clone();
    }

    public static ParseSettings GetStrict()
    {
        return s_strictSettings.Clone();
    }

    /** <summary>allows [1,] - supported by System.Text.Json</summary>*/
    public bool AllowTrailingComma { get; set; } = true;

    // Features removed (no longer supported without JsonParser):
    // - AllowSinglequoteStrings: {'a': 'b'} - System.Text.Json only supports double quotes
    // - AllowAllWhitespace: extended Unicode whitespace - System.Text.Json has its own whitespace rules
    // - AllowUnescapedControlChars: unescaped 0x00-0x1F chars - System.Text.Json requires proper escaping

    public ParseSettings Clone()
    {
        return (ParseSettings)this.MemberwiseClone();
    }
}
