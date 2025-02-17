using System;

namespace PickerUp.Source.Colors;

public struct HexColor : IColorFormat {
    public byte A { get; }
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public HexColor(byte r, byte g, byte b, byte a = 255) {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public HexColor(string hex) {
        if (string.IsNullOrEmpty(hex)){
            throw new ArgumentNullException(nameof(hex), "The HEX color string cannot be null or empty.");
        }

        if (hex.StartsWith('#')){
            hex = hex[1..]; // Remove the '#'
        }

        switch (hex.Length){
            case 8:
                // ARGB format AARRGGBB
                A = Convert.ToByte(hex[..2], 16);
                R = Convert.ToByte(hex[2..4], 16);
                G = Convert.ToByte(hex[4..6], 16);
                B = Convert.ToByte(hex[6..8], 16);

                break;
            case 6:
                // RGB format RRGGBB
                A = 255;// Default alpha to fully opaque
                R = Convert.ToByte(hex[..2], 16);
                G = Convert.ToByte(hex[2..4], 16);
                B = Convert.ToByte(hex[4..6], 16);

                break;
            default:
                throw new ArgumentException("Invalid HEX color format. Use #AARRGGBB, #RRGGBB, AARRGGBB, or RRGGBB.", nameof(hex));
        }
    }
    
    public HexColor(IColorFormat color) : this(color.R, color.G, color.B, color.A) {}

    public string ToDescriptor() => $"HEX({ToString()})";
    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}";

}
