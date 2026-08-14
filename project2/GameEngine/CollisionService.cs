namespace project2.GameEngine;

public enum CollisionResult { None, WallBounce, Goal, PaddleBounce, BottomLoss }

public static class CollisionService
{
        public static CollisionResult Resolve(Heart heart, Paddle paddle, Field field, bool allowGoal = true)
    {
        var pos = heart.Position;
        var vel = heart.Velocity;
        var r = heart.Radius;

        var wallBounced = false;

        // Левая/правая стена
        if (pos.X - r <= 0 && vel.X < 0)
        {
            pos = new Vector2(r, pos.Y);
            vel = new Vector2(-vel.X, vel.Y);
            wallBounced = true;
        }
        else if (pos.X + r >= field.Width && vel.X > 0)
        {
            pos = new Vector2(field.Width - r, pos.Y);
            vel = new Vector2(-vel.X, vel.Y);
            wallBounced = true;
        }

        // Верхняя граница: стена, или ворота (только если allowGoal)
        if (pos.Y - r <= 0 && vel.Y < 0)
        {
            bool inGoal = pos.X >= field.GoalLeft && pos.X <= field.GoalRight;

            if (inGoal && allowGoal)
            {
                heart.Position = pos;
                heart.Velocity = vel;
                return CollisionResult.Goal;
            }

            // Либо это верхняя стена вне ворот, либо это зона ворот,
            // но гол пока запрещён (сердечко не касалось ракетки) — отскакиваем как от стены
            pos = new Vector2(pos.X, r);
            vel = new Vector2(vel.X, -vel.Y);
            wallBounced = true;
        }

        heart.Position = pos;
        heart.Velocity = vel;

        // Ракетка — проверяем ДО нижней границы
        if (vel.Y > 0
            && pos.Y + r >= paddle.Y
            && pos.Y - r <= paddle.Y + paddle.Height
            && pos.X >= paddle.Left - r
            && pos.X <= paddle.Right + r)
        {
            BounceOffPaddle(heart, paddle);
            return CollisionResult.PaddleBounce;
        }

        // Нижняя граница — потеря жизни
        if (pos.Y + r >= field.Height)
        {
            heart.Position = new Vector2(pos.X, field.Height - r);
            return CollisionResult.BottomLoss;
        }

        return wallBounced ? CollisionResult.WallBounce : CollisionResult.None;
    }

    private static void BounceOffPaddle(Heart heart, Paddle paddle)
    {
        var hitOffset = (heart.Position.X - paddle.CenterX) / (paddle.Width / 2);
        hitOffset = Math.Clamp(hitOffset, -1, 1);

        var speed = heart.Velocity.Length;
        var maxAngle = Math.PI / 3;
        var angle = hitOffset * maxAngle;

        heart.Velocity = Vector2.FromAngleFromUp(angle, speed);
        heart.Position = new Vector2(heart.Position.X, paddle.Y - heart.Radius - 0.1);
    }
}