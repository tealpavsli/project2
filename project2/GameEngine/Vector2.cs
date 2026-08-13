namespace project2.GameEngine;

public readonly struct Vector2
{
    public double X { get; }
    public double Y { get; }

    public Vector2(double x, double y) => (X, Y) = (x, y);

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 a, double s) => new(a.X * s, a.Y * s);

    public double Length => Math.Sqrt(X * X + Y * Y);

    public Vector2 Normalized()
    {
        var len = Length;
        return len < 1e-9 ? new Vector2(0, 0) : new Vector2(X / len, Y / len);
    }

    // Направление вверх (0,-1) повёрнутое на угол в радианах
    public static Vector2 FromAngleFromUp(double angleRad, double magnitude)
    {
        // угол 0 = строго вверх; положительный = вправо
        return new Vector2(Math.Sin(angleRad) * magnitude, -Math.Cos(angleRad) * magnitude);
    }
}