using Microsoft.UI.Xaml.Media;
using PickerUp.Source.Watcher;
using SysColor=System.Drawing.Color;
using WinColor=Windows.UI.Color;

namespace PickerUp.Source.Colors;

public class IColor(byte r, byte g, byte b, byte a = 255) : IColorFormat {
    public byte A { get; set; } = a;
    public byte R { get; set; } = r;
    public byte G { get; set; } = g;
    public byte B { get; set; } = b;

    /// <summary>
    /// Constructores
    /// </summary>
    public IColor() : this(255, 255, 255, 255) {}// Por defecto
    public IColor(IColorFormat color) : this(color.R, color.G, color.B, color.A) {}// Desde otro formato
    public IColor(SysColor color) : this(color.R, color.G, color.B, color.A) {}// Desde System.Color
    public IColor(WinColor color) : this(color.R, color.G, color.B, color.A) {}// Desde Windows.UI.Color

    /// <summary>
    /// Descriptores
    /// </summary>
    /// <returns></returns>
    public string ToDescriptor() => $"ARGB({A}, {R}, {G}, {B})";// ARGB(A, R, G, B)

    public HexColor ToHexColor() => new(R, G, B, A);
    public RgbColor ToRgbColor() => new(R, G, B, A);
    public HsvColor ToHsvColor() => new(R, G, B, A);
    public HslColor ToHslColor() => new(R, G, B, A);
    public string ToHexString() => ToHexColor().ToString();
    public string ToRgbString() => ToRgbColor().ToString();
    public string ToHsvString() => ToHsvColor().ToString();
    public string ToHslString() => ToHslColor().ToString();

    public string ToString(string format = "") {
        if (format == ""){
            format = Watcher.Watcher.GetFormat();
        }

        // Formatear el color string según el formato
        string formattedColor = format.ToLower() switch {
            "rgb" => ToRgbString(),
            "hex" => ToHexString(),
            "hsv" => ToHsvString(),
            "hsl" => ToHslString(),
            // "argb" => ToString(),

            _ => ToRgbString()
        };

        return formattedColor;
    }

    /// <summary>
    /// Traductores
    /// </summary>
    public SysColor ToSysColor() => SysColor.FromArgb(A, R, G, B);
    public WinColor ToWinColor() => WinColor.FromArgb(A, R, G, B);
    public SolidColorBrush ToBrush() => new(ToWinColor());

    public void Clone(IColorFormat color) {
        R = color.R;
        G = color.G;
        B = color.B;
        A = color.A;
    }

}
