using System;

namespace PickerUp.Source.Colors;

public class HsvColor : IColorFormat {
  public byte A { get; }
  public byte R { get; }
  public byte G { get; }
  public byte B { get; }

  public double HH { get; }
  public double HS { get; }
  public double HV { get; }
  public double HA { get; }

  public HsvColor(byte r, byte g, byte b, byte a = 255) {
    // Normalizar valores RGB (0–255) a rango 0–1
    double rNorm = r / 255.0;
    double gNorm = g / 255.0;
    double bNorm = b / 255.0;
    double aNorm = a / 255.0;

    // Obtener el máximo y el mínimo
    double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
    double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
    double delta = max - min;

    // Calcular Hue (Tono)
    double hue = 0;
    if (delta != 0){
      if (max == rNorm)
        hue = 60 * (((gNorm - bNorm) / delta) % 6);
      else if (max == gNorm)
        hue = 60 * (((bNorm - rNorm) / delta) + 2);
      else if (max == bNorm)
        hue = 60 * (((rNorm - gNorm) / delta) + 4);
    }

    if (hue < 0)
      hue += 360;

    // Calcular Saturation (Saturación)
    double saturation = (max == 0 ? 0 : delta / max);

    // Calcular Value (Valor)
    double value = max;

    // Devolver H, S, V con el canal Alpha normalizado
    HH = hue;
    HS = saturation;
    HV = value;
    HA = aNorm;

    A = a;
    R = r;
    G = g;
    B = b;
  }

  public HsvColor(double h, double s, double v, double a = 1) {
    // Validación de rango
    if (h is < 0 or > 360) throw new ArgumentOutOfRangeException(nameof(h));
    if (s is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(s));
    if (v is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(v));

    // Cálculo de componentes intermedios
    double c = v * s;
    double hPrime = h / 60.0;
    double x = c * (1 - Math.Abs(hPrime % 2 - 1));
    double m = v - c;

    // RGB inicial
    double r = 0, g = 0, b = 0;

    // Determinar sector del círculo de color Hue (H')
    if (hPrime >= 0 && hPrime < 1){
      r = c;
      g = x;
      b = 0;
    }
    else if (hPrime >= 1 && hPrime < 2){
      r = x;
      g = c;
      b = 0;
    }
    else if (hPrime >= 2 && hPrime < 3){
      r = 0;
      g = c;
      b = x;
    }
    else if (hPrime >= 3 && hPrime < 4){
      r = 0;
      g = x;
      b = c;
    }
    else if (hPrime >= 4 && hPrime < 5){
      r = x;
      g = 0;
      b = c;
    }
    else if (hPrime >= 5 && hPrime <= 6){
      r = c;
      g = 0;
      b = x;
    }

    // Ajustar al rango 0–255
    byte red = (byte)((r + m) * 255);
    byte green = (byte)((g + m) * 255);
    byte blue = (byte)((b + m) * 255);

    // Devolver componentes RGB y el canal Alpha sin modificar
    HH = h;
    HS = s;
    HV = v;
    HA = a;

    A = 255;
    R = red;
    G = green;
    B = blue;
  }

  public HsvColor(HsvColor color) : this(color.HH, color.HS, color.HV, color.HA) {}
  public HsvColor(IColorFormat color) : this(color.R, color.G, color.B, color.A) {}
  public string ToDescriptor() => $"HSV({ToString()})";
  public override string ToString() => $"{HH:F2}, {HS:F2}, {HV:F2}, {HA:F2}";
  public IColor ToArgbColor() => new(this);

}
