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
        Width = field.Width * 0.15;
        Height = field.Height * 0.02;
        CenterX = field.Width / 2;
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