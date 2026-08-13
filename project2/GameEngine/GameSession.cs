namespace project2.GameEngine;

public enum GameState { WaitingToLaunch, Playing, Paused, GameOver }

public class GameSession
{
    public Field Field { get; }
    public Paddle Paddle { get; }
    public Heart Heart { get; }
    public GameState State { get; private set; } = GameState.WaitingToLaunch;

    public int Score { get; private set; }
    public int Record { get; private set; }
    public int Lives { get; private set; } = 5;

    private const double BaseSpeed = 900; // условных единиц/сек
    public double CurrentSpeed { get; private set; } = BaseSpeed;
    public FieldColor CurrentFieldColor { get; private set; } = FieldColor.White;
    public BallSkin CurrentSkin { get; private set; } = BallSkin.RedHeart;
    private bool _touchedPaddleSinceLaunch;
    private const double MinSpeedRatio = 0.5; // не даём скорости упасть ниже 50% от базовой

    public GameSession(double fieldWidth, double fieldHeight)
    {
        Field = new Field(fieldWidth, fieldHeight);
        Paddle = new Paddle(Field);
        Heart = new Heart(Paddle);
        Heart.AttachTo(Paddle);
    }

    public void MovePaddle(double x) => Paddle.MoveTo(x);

    public void Launch(Random rng)
    {
        if (State != GameState.WaitingToLaunch) return;

        _touchedPaddleSinceLaunch = false;

        // Исключаем узкий диапазон около вертикали (-15..+15°),
        // чтобы сердечко не летело сразу в ворота, а уходило в стены —
        // тогда гол возможен только через осознанный отскок от ракетки.
        const double minAngle = 15;
        const double maxAngle = 45;

        var side = rng.Next(2) == 0 ? -1 : 1; // влево или вправо
        var angleDeg = side * (minAngle + rng.NextDouble() * (maxAngle - minAngle));

        var angleRad = angleDeg * Math.PI / 180;
        Heart.Velocity = Vector2.FromAngleFromUp(angleRad, CurrentSpeed);
        Heart.State = HeartState.Flying;
        State = GameState.Playing;
    }

    public CollisionResult Tick(double dtSeconds)
    {
        if (State != GameState.Playing) return CollisionResult.None;

        Heart.Position = Heart.Position + Heart.Velocity * dtSeconds;
        var result = CollisionService.Resolve(Heart, Paddle, Field, allowGoal: _touchedPaddleSinceLaunch);

        if (result == CollisionResult.PaddleBounce)
        {
            _touchedPaddleSinceLaunch = true;
        }

        switch (result)
        {
            case CollisionResult.Goal:
                Score++;
                if (Score > Record) Record = Score;
                CurrentSpeed *= 1.05;
                PauseForMessage();
                break;

            case CollisionResult.BottomLoss:
                Lives--;
                CurrentSpeed = Math.Max(CurrentSpeed * 0.95, BaseSpeed * MinSpeedRatio);
                if (Lives <= 0)
                    State = GameState.GameOver;
                else
                    PauseForMessage();
                break;
        }

        return result;
    }

    private void PauseForMessage()
    {
        State = GameState.Paused;
        // Blazor-компонент сам поставит таймер на 2 сек и вызовет ResetForNextAttempt()
    }

    public void ResetForNextAttempt()
    {
        if (State == GameState.GameOver) return;
        Heart.AttachTo(Paddle);
        State = GameState.WaitingToLaunch;
    }

    public void Restart()
    {
        Score = 0;
        Lives = 5;
        CurrentSpeed = BaseSpeed;
        Heart.AttachTo(Paddle);
        State = GameState.WaitingToLaunch;
    }

    public bool SetSkin(BallSkin skin)
    {
        if (State != GameState.WaitingToLaunch) return false;
        CurrentSkin = skin;
        return true;
    }

        public bool SetFieldColor(FieldColor color)
    {
        if (State != GameState.WaitingToLaunch) return false;
        CurrentFieldColor = color;
        return true;
    }
}