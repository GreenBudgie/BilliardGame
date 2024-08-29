using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Sticker : Node2D
{
    private static readonly NodePath ActionsNodePath = "Actions";
    private static readonly NodePath DragNDropComponentNodePath = "DragNDropComponent";

    public Pocket Pocket { get; set; }

    private DragNDropComponent _dragNDropComponent;

    public override void _Ready()
    {
        _dragNDropComponent = GetNode<DragNDropComponent>(DragNDropComponentNodePath);

        _dragNDropComponent.DragUpdate += _HandleDragUpdate;

        foreach (var action in GetActions())
        {
            action.Sticker = this;
        }
    }

    public List<StickerAction> GetActions()
    {
        return GetNode(ActionsNodePath).GetChildren().OfType<StickerAction>().ToList();
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

    private void _HandleDragUpdate(Vector2 relativeGlobalPosition)
    {
        GlobalPosition = relativeGlobalPosition;
    }
}