using PickerUp.Source.Colors;

namespace PickerUp.Source;

public static class ColorPicked {
    public static bool HasChanged { get; set; } = true;
    private static IColor Color { get; set; } = new();
    private static string PreviousColorString { get; set; } = "";

    public static IColor Get(bool checkIfChanged = false) {
        if (!checkIfChanged) return Color; // Solo retorna el color

        HasChanged = Color.ToString() != PreviousColorString;
        PreviousColorString = Color.ToString();

        return Color;
    }

    public static IColor GetAndCheck() => Get(true);

    public static void Pick(IColor color) {
        // Guarda el color actual antes de cambiar e indica si ha habido cambio
        PreviousColorString = Color.ToString();
        HasChanged = color.ToString() != PreviousColorString;

        Color = color;
        History.AddColor(Color);
    }
}
