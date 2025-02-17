using SysColor=System.Drawing.Color;
using WinColor=Windows.UI.Color;

namespace PickerUp.Source.Colors;

public readonly struct RgbColor(byte r, byte g, byte b, byte a = 255) : IColorFormat {
    public byte R { get; } = r;
    public byte G { get; } = g;
    public byte B { get; } = b;
    public byte A { get; } = a;


    // Constructors
    public RgbColor(IColorFormat color) : this(color.R, color.G, color.B, color.A) {}

    public RgbColor(SysColor color) : this(color.R, color.G, color.B, color.A) {}

    public RgbColor(WinColor color) : this(color.R, color.G, color.B, color.A) {}

    // Descriptors
    public override string ToString() => $"{R}, {G}, {B}";

    public string ToDescriptor() => $"RGB({R}, {G}, {B})";

    // Parsers
    public (byte r, byte g, byte b) ToTuple() => (R, G, B);

    public SysColor ToSysColor() => SysColor.FromArgb(255, R, G, B);

    public WinColor ToWinColor() => WinColor.FromArgb(255, R, G, B);

}
