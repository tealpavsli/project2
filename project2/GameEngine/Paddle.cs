namespace project2.GameEngine;

public class Paddle
{
    private readonly Field _field;
    private readonly double _baseWidth;

    public double SizeMultiplier { get; private set; } = 1.75;

    public double Width => _baseWidth * SizeMultiplier;
    public double Height { get; }
    public double CenterX { get; private set; }

    public Paddle(Field field)
    {
        _field = field;
        _baseWidth = field.Width * WidthRatioFor(field.Width);
        Height = field.Height * 0.02;
        CenterX = field.Width / 2;
    }

    public double Y => _field.Height - Height - _field.Height * 0.03;

    public void MoveTo(double x)
    {
        var half = Width / 2;
        CenterX = Math.Clamp(x, half, _field.Width - half);
    }

    public double Left => CenterX - Width / 2;
    public double Right => CenterX + Width / 2;

    // Вызывается после каждого гола — ракетка постепенно сужается
    public void ShrinkAfterGoal()
    {
        SizeMultiplier *= 0.99;
    }

    // Вызывается только при полном рестарте игры (Restart), не при потере жизни
    public void ResetSize()
    {
        SizeMultiplier = 1.75;
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
}