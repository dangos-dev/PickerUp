using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class TetradicPalette {
    private const int Size = 4; // Las tetradas siempre se componen de 4 colores

    public static IColor[] FromColor(IColor color) {

        IColor[] colors = new IColor[Size];
        HslColor baseColor = new(color);

        for (int i = 0; i < Size; i++){
            // Calcular el nuevo matiz basado en las tetradas (0°, +90°, +180°, +270°)
            float newHue = (baseColor.H + (90f * i)) % 360f; // Aumentar 90° en cada iteración

            HslColor tetradicColor = new(baseColor) {
                H = newHue // Asignar el nuevo matiz
            };

            colors[i] = new IColor(tetradicColor);
        }

        return colors;
    }
}
