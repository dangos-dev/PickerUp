using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class TintsPalette {

    public static IColor[] FromColor(IColor color, int size) {

        IColor[] colors = new IColor[size];
        HslColor baseColor = new(color);

        // Generar colores variando la LUMINANCIA para crear los tonos
        // Variando entre el color base (baseColor.L) y 1.0 (blanco)
        for (int i = 0; i < size; i++){
            baseColor.L += (1f - baseColor.L) * (i / (float) (size - 1));
            colors[i] = new IColor(baseColor);
        }

        return colors;
    }
}
