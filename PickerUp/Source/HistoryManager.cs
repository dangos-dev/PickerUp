using PickerUp.Source.Colors;
using System;
using System.Linq;

namespace PickerUp.Source;

public class History {
    public static IColor[] Colors { get; set; } = Enumerable.Range(0, 30)
        .Select(_ => new IColor())// Example RGB values
        .ToArray();

    public static void AddColor(IColor newColor) {
        // Desplazar todos los elementos hacia la derecha
        for (int i = Colors.Length - 1; i > 0; i--){
            Colors[i] = Colors[i - 1];
        }

        // Coloca el nuevo color al inicio de la cola
        Colors[0] = newColor;
        NotifyColorsChanged();
    }

    public static event Action? ColorsChanged;

    // A method to invoke the ColorsChanged event
    public static void NotifyColorsChanged() {
        ColorsChanged?.Invoke();
    }
}
