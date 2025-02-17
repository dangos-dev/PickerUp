using PickerUp.Source.Colors;
using PickerUp.Source.HistoryDefinition;
using PickerUp.Source.PalettesDefinition;
using PickerUp.Source.Watcher;

namespace PickerUp.Source.EditorDefinition;

public class Editor {
    private static IColor Color { get; set; } = new();
    public static bool IsColorChanged { get; set; } = true;
    private static string _previousColorString { get; set; } = "";
    public static IColor GetFocused() {
        IsColorChanged = Color.ToString() != _previousColorString;
        _previousColorString = Color.ToString();

        return Color;
    }
    public static void FocusColor(IColor color) {
        // Guarda el color actual antes de cambiar e indica si ha habido cambio
        _previousColorString = Color.ToString();
        IsColorChanged = color.ToString() != _previousColorString;

        Color = color;
        History.AddColor(Color);
        SaturationPalette.SetColor(Color);
    }
}
