using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class TriadPalette {
    private const int Size = 3; // Las triadas solo se componen de 3 colores

    public static IColor[] FromColor(IColor color) {

        IColor[] colors = new IColor[Size];
        HslColor baseColor = new(color);

        for (int i = 0; i < Size; i++){
            float newHue = (baseColor.H + (120f * i)) % 360f; // Avanza 120° por cada paso (triada)

            HslColor triadicColor = new(baseColor) {
                H = newHue // Asignar el nuevo matiz, manteniendo otros parámetros iguales
            };

            colors[i] = new IColor(triadicColor);
        }

        return colors;
    }
}
