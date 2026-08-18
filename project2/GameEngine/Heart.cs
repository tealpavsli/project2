namespace project2.GameEngine;

public enum HeartState { AttachedToPaddle, Flying }

public class Heart
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public double Radius { get; }
    public HeartState State { get; set; } = HeartState.AttachedToPaddle;

    public Heart(Paddle paddle)
    {
        Radius = paddle.BaseWidthForBall * 0.30 / 2; // фиксированный размер, не зависит от текущего размера ракетки
    }

    public void AttachTo(Paddle paddle)
    {
        Position = new Vector2(paddle.CenterX, paddle.Y - Radius);
        Velocity = new Vector2(0, 0);
        State = HeartState.AttachedToPaddle;
    }
}