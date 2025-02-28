using PickerUp.Source.Colors;
using System;

namespace PickerUp.Source.Palettes;

public static class AnalogousPalette {
    private const int Size = 3; // Las paletas análogas tendrán un número fijo de 3 colores.

    public static IColor[] FromColor(IColor color) {
        // Inicializar el array de colores con un tamaño fijo
        IColor[] colors = new IColor[Size];

        // Convertir el color base a HSL para manipulación más sencilla
        HslColor baseColor = new(color);

        // Calcular la separación en grados entre los colores análogos
        float angleStep = 30f / (Size - 1); // Dividir 30° entre el número de colores menos uno

        for (int i = 0; i < Size; i++){
            // Desviaciones alrededor del matiz base (-15° a +15°)
            float newHue = (baseColor.H - (30f / 2f) + (angleStep * i)) % 360f;
            if (newHue < 0) newHue += 360f; // Asegurar que el Hue esté en el rango [0, 360)

            // Crear un nuevo color análogo con el matiz calculado
            HslColor analogousColor = new(baseColor) {
                H = newHue // Establecer el nuevo matiz
            };

            // Asignar el color al arreglo
            colors[i] = new IColor(analogousColor);
        }

        // Retornar la paleta de colores análogos
        return colors;
    }
}
