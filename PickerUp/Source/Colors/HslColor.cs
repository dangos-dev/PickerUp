using System.Drawing;

namespace PickerUp.Source.Colors;

public class HslColor : IColorFormat {
    private byte _a;
    private byte _r;
    private byte _g;
    private byte _b;
    private float _h;
    private float _s;
    private float _l;

    public byte A {
        get => _a;
        set {
            _a = value;
            ComputeRgbChannel();
        }
    }

    public byte R {
        get => _r;
        set {
            _r = value;
            ComputeRgbChannel();
        }
    }

    public byte G {
        get => _g;
        set {
            _g = value;
            ComputeRgbChannel();
        }
    }

    public byte B {
        get => _b;
        set {
            _b = value;
            ComputeRgbChannel();
        }
    }

    public float H {
        get => _h;
        set {
            _h = value;
            ComputeRgbChannel();
        }
    }

    public float S {
        get => _s;
        set {
            _s = value;
            ComputeRgbChannel();
        }
    }

    public float L {
        get => _l;
        set {
            _l = value;
            ComputeRgbChannel();
        }
    }

    public HslColor(byte r, byte g, byte b, byte a = 255) {

        _r = r;
        _g = g;
        _b = b;
        _a = a;

        ComputeHslChannel();
    }

    public HslColor(HslColor color) {
        _r = color.R;
        _g = color.G;
        _b = color.B;
        _a = color.A;

        _h = color.H;
        _s = color.S;
        _l = color.L;
    }

    private void ComputeHslChannel() {
        // Obtiene los valores HSL desde System.Drawing
        Color sysColor = Color.FromArgb(_a, _r, _g, _b);

        _h = sysColor.GetHue();
        _s = sysColor.GetSaturation();
        _l = sysColor.GetBrightness();
    }

    private void ComputeRgbChannel() {
        byte r;
        byte g;
        byte b;

        if (S == 0){
            r = g = b = (byte) (_l * 255);
        }
        else{
            float hue = _h / 360;
            float v2 = (_l < 0.5) ? (_l * (1 + _s)) : ((_l + _s) - (_l * _s));
            float v1 = 2 * _l - v2;

            r = (byte) (255 * HueToRgbChannel(v1, v2, hue + (1.0f / 3)));
            g = (byte) (255 * HueToRgbChannel(v1, v2, hue));
            b = (byte) (255 * HueToRgbChannel(v1, v2, hue - (1.0f / 3)));
        }

        _r = r;
        _g = g;
        _b = b;
        _a = 255;
    }

    private static float HueToRgbChannel(float v1, float v2, float vH) {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;

        if ((6 * vH) < 1) return (v1 + (v2 - v1) * 6 * vH);
        if ((2 * vH) < 1) return v2;
        if ((3 * vH) < 2) return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

        return v1;
    }

    public HslColor(IColorFormat color) : this(color.R, color.G, color.B, color.A) {}
    public string ToDescriptor() => $"HSL({H:0.##}, {S:P2}, {L:P2})";
    public override string ToString() => $"{H:0.#}, {S:P0}, {L:P0}";
}
