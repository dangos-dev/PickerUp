using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PickerUp.Components.UserControls;
using PickerUp.Source;
using PickerUp.Source.Colors;
using PickerUp.Source.Palettes;
using System.Collections.Generic;
using System.Linq;

namespace PickerUp.Components.Pages;

public sealed partial class PalettesPage : Page {

    private static IColor _selectedColor = History.Colors.First();
    private static List<Border> _colorBorders = [];

    // Constructor
    public PalettesPage() {
        InitializeComponent();

        History.ColorsChanged += OnHistoryColorsChanged;

        PopulateHistoryPanel();
        PopulatePalettesPanel();
    }

    // Destructor (finalizador)
    ~PalettesPage() {
        History.ColorsChanged -= OnHistoryColorsChanged;
    }

    private void OnHistoryColorsChanged() {
        DispatcherQueue.TryEnqueue(() => PopulateHistoryPanel());
    }

    private void PopulateHistoryPanel() {

        _historyPanel.Children.Clear();
        _colorBorders.Clear();

        // Crear la lista de colores del historiañ
        foreach (var color in History.Colors){
            var border = new Border {
                Background = color.ToBrush(),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 0, 2.5),
                Height = 20,
                Width = 20,
                BorderThickness = new Thickness(0),
                BorderBrush = new SolidColorBrush(Colors.Transparent)
            };

            border.PointerPressed += (s, e) => {
                UpdateSelectedColor(border, color);
            };

            // Evento para cambiar el tamaño con el hover
            border.PointerEntered += (s, e) => {
                if (_selectedColor != color){
                    border.Width = 22;
                    border.Height = 22;
                }
            };

            border.PointerExited += (s, e) => {
                if (_selectedColor != color){
                    border.Width = 20; // Tamaño original si no está seleccionado
                    border.Height = 20;
                }
            };

            _historyPanel.Children.Add(border);
            _colorBorders.Add(border);
        }
    }

    private void UpdateSelectedColor(Border selectedBorder, IColor color) {
        _selectedColor = color; // Cambia el color seleccionado

        // Restablece el tamaño de todos los bordes
        foreach (var border in _colorBorders){
            border.Width = 20;
            border.Height = 20;
        }

        // Cambia el tamaño del borde seleccionado
        selectedBorder.Width = 25;
        selectedBorder.Height = 25;

        PopulatePalettesPanel();
    }

    private void AddColorPalette(IColor[] colors, string description = "") {
        _palettesPanel.Children.Add(
            new ColorPaletteControl {
                Columns = colors.Length,
                Colors = colors,
                Description = description,
                Height = 40,
                Margin = new Thickness(5, 2.5, 5, 10)
            }
        );
    }

    private void PopulatePalettesPanel() {
        _palettesPanel.Children.Clear();

        // Genera y agrega las paletas
        AddColorPalette(ShadesPalette.FromColor(_selectedColor, 10), "Shades"); // Tonos
        AddColorPalette(TintsPalette.FromColor(_selectedColor, 10), "Tints"); // Matices
        AddColorPalette(TonesPalette.FromColor(_selectedColor, 10), "Tones"); // Variantes
        AddColorPalette(AnalogousPalette.FromColor(_selectedColor), "Analogous"); // Análogos
        AddColorPalette(ComplementaryPalette.FromColor(_selectedColor), "Complementary"); // Complementarios
        AddColorPalette(SplitComplementaryPalette.FromColor(_selectedColor), "Split Complementary"); // Complementarios divididos
        AddColorPalette(TriadPalette.FromColor(_selectedColor), "Triadic"); // Triádicos
        AddColorPalette(TetradicPalette.FromColor(_selectedColor), "Tetradic"); // Tetrádicos
    }
}
