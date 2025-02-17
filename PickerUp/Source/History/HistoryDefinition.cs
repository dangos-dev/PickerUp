using PickerUp.Source.Colors;

namespace PickerUp.Source.HistoryDefinition;

public class History {
  public static IColor[] Colors { get; set; } =
    [ new(), new(), new(), new(), new(), new() ];

  public static void AddColor(IColor newColor) {
    // Desplazar todos los elementos hacia la derecha
    for (int i = Colors.Length - 1; i > 0; i--){
      Colors[i] = Colors[i - 1];
    }

    // Coloca el nuevo color al inicio de la cola
    Colors[0] = newColor;
  }
}
