using PickerUp.Source.Colors;

namespace PickerUp.Source.PalettesDefinition;

public class SaturationPalette {
    public static IColor[] Colors { get; set; } =
        [new(), new(), new(), new(), new(), new(), new(), new(), new(), new()];

    public static void SetColor(IColor color) {
        // Convertir el color base a HSV
        HslColor baseColor = new(color);
        float originalSaturation = baseColor.S;
        
        // Generar colores variando la saturación
        for (int i = 0; i < Colors.Length; i++){
            float newSaturation = (originalSaturation != 0)
                ? (i / (float) (Colors.Length - 1)) 
                : 0;// Manejar caso de HS == 0

            // Ajusta la saturacion
            baseColor.L = newSaturation;

            // Convertir de vuelta a ArgbColor y asignar al array
            Colors[i].Clone(baseColor);
        }
    }
}
