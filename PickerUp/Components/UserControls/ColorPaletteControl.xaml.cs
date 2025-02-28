using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PickerUp.Source.Colors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PickerUp.Components.UserControls;

public sealed partial class ColorPaletteControl : UserControl {
    // DependencyProperty para aceptar dinámicamente el número de columnas
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(
            nameof(Columns),
            typeof(int),
            typeof(ColorPaletteControl),
            new PropertyMetadata(12, OnColumnsChanged)
        );

    // DependencyProperty para aceptar y renderizar los colores
    public static readonly DependencyProperty ColorsProperty =
        DependencyProperty.Register(
            nameof(Colors),
            typeof(IColor[]),
            typeof(ColorPaletteControl),
            new PropertyMetadata(Array.Empty<IColor>(), OnColorsChanged)
        );

    // DependencyProperty para aceptar y mostrar la descripción
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(ColorPaletteControl),
            new PropertyMetadata(string.Empty, OnDescriptionChanged)
        );

    public int Columns {
        get => (int) GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public IColor[] Colors {
        get => (IColor[]) GetValue(ColorsProperty);
        set => SetValue(ColorsProperty, value);
    }

    public string Description {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public ColorPaletteControl() {
        this.InitializeComponent();
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ColorPaletteControl control){
            control.GeneratePalette();
        }
    }

    private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ColorPaletteControl control){
            control.GeneratePalette();
        }
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ColorPaletteControl control){
            control.UpdateDescription();
        }
    }

    // Genera dinámicamente la cuadrícula de colores
    private void GeneratePalette() {
        // Eliminar Bordes (contenido previo)
        var bordersToRemove = _paletteGrid.Children.OfType<Border>().ToList();
        foreach (var border in bordersToRemove){
            _paletteGrid.Children.Remove(border);
        }

        _paletteGrid.ColumnDefinitions.Clear();// Limpia las definiciones de columnas

        // Crea las definiciones de columna basadas en Columns
        for (int i = 0; i < Columns; i++){
            _paletteGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        // Crea filas dinámicas según los colores
        int rows = (int) Math.Ceiling((double) Colors.Length / Columns);
        _paletteGrid.RowDefinitions.Clear();
        for (int i = 0; i < rows; i++){
            _paletteGrid.RowDefinitions.Add(new RowDefinition());
        }

        // Agrega Bordes para cada color
        for (int index = 0; index < Colors.Length; index++){
            var color = Colors[index];
            var border = new Border {
                Background = new SolidColorBrush(color.ToWinColor()),
            };

            ToolTipService.SetToolTip(border, color.ToString());

            // Agregar evento al hacer clic en un color
            border.Tapped += (sender, args) => {
                OnColorSelected?.Invoke(this, color);
            };

            // Posiciona el borde dinámicamente
            int row = index / Columns;
            int column = index % Columns;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);

            _paletteGrid.Children.Add(border);
        }

        // Actualizar la descripción si aplica
        UpdateDescription();
    }

    // Actualiza o elimina la descripción según el valor de la propiedad
    private void UpdateDescription() {
        // Buscar si ya hay un TextBlock para la descripción
        if (string.IsNullOrEmpty(Description)) {
            _textBlock.Visibility = Visibility.Collapsed; // Esconde el TextBlock
        } else {
            _textBlock.Text = Description;
            _textBlock.Visibility = Visibility.Visible; // Asegúrate de que sea visible
        }
    }

    // Evento para manejar la selección de un color
    public event ColorSelectedEventHandler OnColorSelected;
    public delegate void ColorSelectedEventHandler(object sender, IColor color);
}
