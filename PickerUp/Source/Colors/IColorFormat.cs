namespace PickerUp.Source.Colors;

public interface IColorFormat {
    byte A { get; }
    byte R { get; }
    byte G { get; }
    byte B { get; }

    string ToString();
}
