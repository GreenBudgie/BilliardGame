using System;

public enum BallType
{
    
    Solid,
    Stripe

}

public static class BallTypeExtension
{

    public static string GetBodySpriteNodeName(this BallType ballType)
    {
        return ballType switch
        {
            BallType.Solid => "SolidBodySprite",
            BallType.Stripe => "StripeBodySprite",
            _ => throw new ArgumentOutOfRangeException(nameof(ballType), ballType, null)
        };
    }
    
    public static string GetOverlaySpriteNodeName(this BallType ballType)
    {
        return ballType switch
        {
            BallType.Solid => "SolidOverlaySprite",
            BallType.Stripe => "StripeOverlaySprite",
            _ => throw new ArgumentOutOfRangeException(nameof(ballType), ballType, null)
        };
    }
    
}