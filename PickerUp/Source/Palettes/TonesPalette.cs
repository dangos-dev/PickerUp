using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class TonesPalette {

    public static IColor[] FromColor(IColor color, int size) {

        IColor[] colors = new IColor[size];
        HslColor baseColor = new(color);

        float originalSaturation = baseColor.S; // Guardar la saturación original

        // Generar los colores variando la saturación
        for (int i = 0; i < size; i++){
            float newSaturation = (originalSaturation != 0)
                ? (i / (float) (size - 1)) // Gradúa la saturación entre 0 y el valor original
                : 0;

            // Ajustar la luminancia
            baseColor.S = newSaturation;

            colors[i] = new IColor(baseColor);
        }

        return colors;
    }
}
