using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class ComplementaryPalette {
    private const int Size = 2; // Las paletas complementarias siempre contienen exactamente 2 colores.

    public static IColor[] FromColor(IColor color) {

        IColor[] colors = new IColor[Size];
        HslColor baseColor = new(color);

        // Calcular el complemento directo (Hue + 180°)
        float complementaryHue = (baseColor.H + 180f) % 360f;

        HslColor complementaryColor = new(baseColor) { H = complementaryHue };

        colors[0] = new IColor(baseColor);
        colors[1] = new IColor(complementaryColor);

        return colors;
    }
}
