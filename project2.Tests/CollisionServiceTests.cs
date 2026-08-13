using project2.GameEngine;
using Xunit;

namespace project2.Tests;

public class CollisionServiceTests
{
    private static (Field field, Paddle paddle, Heart heart) CreateSetup(double w = 400, double h = 600)
    {
        var field = new Field(w, h);
        var paddle = new Paddle(field);
        var heart = new Heart(paddle);
        return (field, paddle, heart);
    }

    [Fact]
    public void Heart_BouncesOffLeftWall()
    {
        var (field, paddle, heart) = CreateSetup();
        heart.Position = new Vector2(2, 300);
        heart.Velocity = new Vector2(-50, -50);

        var result = CollisionService.Resolve(heart, paddle, field);

        Assert.Equal(CollisionResult.WallBounce, result);
        Assert.True(heart.Velocity.X > 0);
    }

    [Fact]
    public void Heart_BouncesOffRightWall()
    {
        var (field, paddle, heart) = CreateSetup();
        heart.Position = new Vector2(field.Width - 2, 300);
        heart.Velocity = new Vector2(50, -50);

        var result = CollisionService.Resolve(heart, paddle, field);

        Assert.Equal(CollisionResult.WallBounce, result);
        Assert.True(heart.Velocity.X < 0);
    }

    [Fact]
    public void Heart_ScoresGoal_WhenCrossingGoalArea()
    {
        var (field, paddle, heart) = CreateSetup();
        heart.Position = new Vector2(field.Width / 2, 2);
        heart.Velocity = new Vector2(0, -50);

        var result = CollisionService.Resolve(heart, paddle, field);

        Assert.Equal(CollisionResult.Goal, result);
    }

    [Fact]
    public void Heart_BouncesOffTopWall_OutsideGoal()
    {
        var (field, paddle, heart) = CreateSetup();
        heart.Position = new Vector2(5, 2);
        heart.Velocity = new Vector2(0, -50);

        var result = CollisionService.Resolve(heart, paddle, field);

        Assert.Equal(CollisionResult.WallBounce, result);
        Assert.True(heart.Velocity.Y > 0);
    }

    [Fact]
    public void Heart_TouchingBottom_ReturnsLoss()
    {
        var (field, paddle, heart) = CreateSetup();
        heart.Position = new Vector2(field.Width / 2, field.Height - 1);
        heart.Velocity = new Vector2(0, 50);

        var result = CollisionService.Resolve(heart, paddle, field);

        Assert.Equal(CollisionResult.BottomLoss, result);
    }

    [Fact]
    public void PaddleBounce_AtCenter_GoesNearlyStraightUp()
    {
        var (field, paddle, heart) = CreateSetup();
        heart.Position = new Vector2(paddle.CenterX, paddle.Y - heart.Radius);
        heart.Velocity = new Vector2(0, 100);

        var result = CollisionService.Resolve(heart, paddle, field);

        Assert.Equal(CollisionResult.PaddleBounce, result);
        Assert.True(heart.Velocity.Y < 0);
        Assert.True(Math.Abs(heart.Velocity.X) < 5);
    }

    [Fact]
    public void PaddleBounce_AtRightEdge_DeflectsRight()
    {
        var (field, paddle, heart) = CreateSetup();
        heart.Position = new Vector2(paddle.Right - 1, paddle.Y - heart.Radius);
        heart.Velocity = new Vector2(0, 100);

        CollisionService.Resolve(heart, paddle, field);

        Assert.True(heart.Velocity.X > 0);
    }
}