namespace project2.GameEngine;

public class Paddle
{
    public double CenterX { get; private set; }
    public double Width { get; }
    public double Height { get; }
    private readonly Field _field;

    public Paddle(Field field)
    {
        _field = field;
        Width = field.Width * WidthRatioFor(field.Width);
        Height = field.Height * 0.02;
        CenterX = field.Width / 2;
    }

    // На узких мобильных экранах ракетка (и мяч, как % от неё) визуально крупнее,
    // на широких десктопных полях — прежние 15%, поведение не меняется.
    private static double WidthRatioFor(double fieldWidth)
    {
        const double desktopFieldWidth = 800;
        const double mobileFieldWidth = 380;
        const double desktopRatio = 0.15;
        const double mobileRatio = 0.22;

        if (fieldWidth >= desktopFieldWidth) return desktopRatio;
        if (fieldWidth <= mobileFieldWidth) return mobileRatio;

        var t = (fieldWidth - mobileFieldWidth) / (desktopFieldWidth - mobileFieldWidth);
        return mobileRatio + t * (desktopRatio - mobileRatio);
    }

    public double Y => _field.Height - Height - _field.Height * 0.03; // зазор ~3% от высоты поля
    public void MoveTo(double x)
    {
        var half = Width / 2;
        CenterX = Math.Clamp(x, half, _field.Width - half);
    }

    public double Left => CenterX - Width / 2;
    public double Right => CenterX + Width / 2;
}