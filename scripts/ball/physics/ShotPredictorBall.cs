using System.Collections.Generic;
using Godot;

public partial class ShotPredictorBall : BallRigidBody
{

    private readonly List<BallRigidBody> _predictorBallList = new();
    
    public override void _Ready()
    {
        base._Ready();
        
        _predictorBallList.Add(this);
    }

    public override bool AwakesOtherBalls()
    {
        return false;
    }

    private List<CollisionData> _predictedCollisions = new();

    public ShotPrediction GetShotPrediction(Vector2 initialVelocity)
    {
        var delta = BallPhysicsServer.Instance.DefaultDelta;
        _predictedCollisions.Clear();
        SetLinearVelocity(initialVelocity);
        while (!IsSleeping)
        {
            BallPhysicsServer.Instance.PerformPhysicsStepForBalls(delta, _predictorBallList);
        }

        return new ShotPrediction(GlobalPosition, _predictedCollisions);
    }
    
    public override void HandleNewCollision(CollisionData collision)
    {
        base.HandleNewCollision(collision);
        
        _predictedCollisions.Add(collision);
    }
    
    public record struct ShotPrediction(
        Vector2 StopPoint,
        List<CollisionData> Collisions
    );

}