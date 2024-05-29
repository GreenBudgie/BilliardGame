using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Sticker : Node2D
{
    
    public Pocket Pocket { get; set; }

    public List<StickerAction> GetActions()
    {
        return GetChildren().OfType<StickerAction>().ToList();
    }

    public async Task Trigger(PocketScoreContext context)
    {
        var actions = GetActions();
        foreach (var action in actions)
        {
            action.Trigger(context);
            Shake();
            await this.Wait(0.25f);
        }
    }

    private void Shake()
    {
        var initialPosition = Position;
        var shakePosition = Position + new Vector2(GD.RandRange(-20, 20), GD.RandRange(-20, -30));
        var shakeTween = CreateTween();
        shakeTween.TweenProperty(this, "position", shakePosition, 0.1)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.InOut);
        shakeTween.TweenProperty(this, "position", initialPosition, 0.1)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.InOut);
    }

}