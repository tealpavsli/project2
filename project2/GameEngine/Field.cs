namespace project2.GameEngine;

public class Field
{
    public double Width { get; }
    public double Height { get; }

    public double GoalWidth => Width * 0.25;
    public double GoalLeft => (Width - GoalWidth) / 2;
    public double GoalRight => GoalLeft + GoalWidth;
    public const double GoalDepth = 4; // условная минимальная толщина линии ворот

    public Field(double width, double height)
    {
        Width = width;
        Height = height;
    }
}