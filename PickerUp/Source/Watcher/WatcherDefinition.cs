using PickerUp.Source.Colors;
using System;
using System.Drawing;
using Vanara.PInvoke;

namespace PickerUp.Source.Watcher;

public class Watcher {
    private static IColor Color { get; set; } = new();
    public static string Format { get; set; } = "rgb";
    private static Point Position { get; set; }
    public static bool IsWatching { get; set; } = true;
    public static bool IsColorChanged { get; set; } = true;
    private static string _previousColorString { get; set; } = "";
    public static Bitmap Preview { get; set; }
    public static Size viewportSize = new(300, 60);
    private static DateTime _lastExecutionTime = DateTime.MinValue;// Track when WatchPixel was last called
    private readonly static int _executionIntervalMs = 200;// Limit to 5 times per second (1000ms / 5 = 200ms)


    public static IColor GetPixelColor(Point point) {
        User32.SafeReleaseHDC hdc = default;

        try{
            // Obtener el contexto del dispositivo para capturar el color del píxel
            hdc = User32.GetDC();
            IntPtr hdcIntPtr = hdc.DangerousGetHandle();

            uint pixel = Gdi32.GetPixel(hdcIntPtr, point.X, point.Y);

            // Separar los componentes de color desde el valor de píxel
            byte red = (byte) (pixel & 0x000000FF);
            byte green = (byte) ((pixel & 0x0000FF00) >> 8);
            byte blue = (byte) ((pixel & 0x00FF0000) >> 16);

            // Retornar el color en formato ArgbColor
            return new IColor(red, green, blue);
        }
        finally{
            // Liberar el contexto del dispositivo
            if (hdc != IntPtr.Zero){
                User32.ReleaseDC(IntPtr.Zero, hdc);
            }
        }
    }
    public static (IColor color, Point position) WatchPixel() {

        if (!IsWatching)
            return (Color, Position);

        // Esto genera un leak de memoria increible, lol
        // DateTime now = DateTime.UtcNow;
        // if ((now - _lastExecutionTime).TotalMilliseconds < _executionIntervalMs) { // 5 frames por segundo
        //     return (Color, Position);
        // }
        // _lastExecutionTime = now;

        User32.GetCursorPos(out POINT cursorPosition);

        // ArgbColor pixelColor = GetPixelColor(cursorPosition);
        (IColor pixelColor, Preview) = PreviewPixel(cursorPosition, 5, viewportSize);

        // Guarda el color actual antes de cambiar e indica si ha habido cambio
        IsColorChanged = (Color.ToString(Format) != _previousColorString);
        _previousColorString = Color.ToString(Format);

        IColor color = new(pixelColor);
        Point position = cursorPosition;

        Color = color;
        Position = position;

        return (color, position);
    }

    private static (IColor PixelColor, Bitmap PreviewCanvas ) PreviewPixel(Point cursorPosition, int size = 5, Size viewportSize = default) {

        IColor pixelColor = new();

        const int plusSize = 10;

        // Validar que el tamaño sea mayor a 0
        if (size <= 0){
            throw new ArgumentException("El tamaño debe ser mayor que 0.");
        }


        int viewportProportion = viewportSize.Height / size;

        Bitmap zoomBitmap = new(viewportSize.Width / viewportProportion, size);
        Bitmap viewportBitmap = new(viewportSize.Width, viewportSize.Height);

        Point topLeftPoint = new(cursorPosition.X - zoomBitmap.Width / 2, cursorPosition.Y - zoomBitmap.Height / 2);

        // Captura el cuadrado alrededor del cursor
        using Graphics g = Graphics.FromImage(zoomBitmap);
        g.CopyFromScreen(topLeftPoint, Point.Empty, viewportSize);
        
        using Graphics g1 = Graphics.FromImage(viewportBitmap);
        g1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;// Acentúa el aspecto pixelado

        // Dibujar la imagen original al tamaño de 60x60
        g1.DrawImage(zoomBitmap, 0, 0, viewportSize.Width, viewportSize.Height);
        int halfPlus = plusSize / 2;// Mitad del signo de + (longitud de cada línea del "+")

        // Calcula el centro del bitmap
        int centerX = (viewportBitmap.Width / 2) - halfPlus;
        int centerY = (viewportBitmap.Height / 2) - halfPlus;

        // Coordenadas del signo "+" (ajustadas)
        int horizontalStartX = centerX - halfPlus;// Línea horizontal (inicio)
        int horizontalEndX = centerX + halfPlus;// Línea horizontal (fin)

        int verticalStartY = centerY - halfPlus;// Línea vertical (inicio)
        int verticalEndY = centerY + halfPlus;// Línea vertical (fin)

        pixelColor = new IColor(viewportBitmap.GetPixel(centerX, centerY));

        // Dibuja las líneas del "+"
        g1.DrawLine(Pens.Red, horizontalStartX, centerY, horizontalEndX, centerY);// Línea horizontal
        g1.DrawLine(Pens.Red, centerX, verticalStartY, centerX, verticalEndY);// Línea vertical

        // Obtener el color del píxel en la posición (0, 0) del bitmap
        // Color color = bitmap.GetPixel(0, 0);

        zoomBitmap.Dispose();
        // viewportBitmap.Dispose();



        return (pixelColor, viewportBitmap);
    }


    // public static string GetFormattedColor(IColor? color = null) {
    //     if (color == null){
    //         return Color.ToString(Format);
    //     }
    //
    //     return color.ToString(Format);
    // }

    public static void SetFormat(string desiredFormat) {
        Format = desiredFormat;
    }

    public static void SetWatching(bool state) {
        IsWatching = state;
    }

    public static IColor GetColor() {
        return Color;
    }

    public static void SetColor(IColor color) {
        Color = color;
    }

    public static Point GetPosition() {
        return Position;
    }

    public static string GetFormat() {
        return Format;
    }
}
