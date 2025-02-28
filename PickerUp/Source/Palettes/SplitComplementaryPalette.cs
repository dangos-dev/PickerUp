using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class SplitComplementaryPalette {
    private const int Size = 3; // Las paletas complementarias divididas siempre contienen 3 colores

    public static IColor[] FromColor(IColor color) {

        IColor[] colors = new IColor[Size];
        HslColor baseColor = new(color);

        // Calcular los colores divididos ±30° alrededor del complemento directo
        float splitOffset = 30f; // Ángulo de desplazamiento desde el complemento
        float complement = (baseColor.H + 180f) % 360f; // Calcula el complemento directo (180° del matiz base)

        // Primer color complementario (+30° desde el complemento directo)
        float firstSplitHue = (complement - splitOffset) % 360f;
        if (firstSplitHue < 0) firstSplitHue += 360f;
        HslColor firstSplit = new(baseColor) { H = firstSplitHue };

        // Segundo color complementario (-30° desde el complemento directo)
        float secondSplitHue = (complement + splitOffset) % 360f;
        HslColor secondSplit = new(baseColor) { H = secondSplitHue };

        // Agregar el color base como el primer color de la paleta
        colors[0] = new IColor(baseColor);

        // Asignar los colores complementarios al array
        colors[1] = new IColor(firstSplit);
        colors[2] = new IColor(secondSplit);

        return colors;
    }
}
