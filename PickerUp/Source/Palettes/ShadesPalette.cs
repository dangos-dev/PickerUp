using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class ShadesPalette {

    public static IColor[] FromColor(IColor color, int size) {

        IColor[] colors = new IColor[size];
        HslColor baseColor = new(color);

        float originalLuminance = baseColor.L;

        for (int i = 0; i < size; i++){
            // Reducir la luminancia hacia el negro
            baseColor.L = originalLuminance - ((originalLuminance / (size - 1)) * i);
            colors[i] = new IColor(baseColor);
        }

        return colors;
    }
}
