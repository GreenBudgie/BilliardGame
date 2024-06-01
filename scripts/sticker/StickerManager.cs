using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class StickerManager : Node2D
{

    public void AddSticker(Sticker sticker, StickerPosition position)
    {
        AddChild(sticker);
        sticker.GlobalPosition = position.GlobalPosition;
        sticker.Pocket = position.Pocket;
        
    }

    /// <summary>
    /// Retrieves a list of all stickers in the StickerManager, sorted by their respective pockets' positions.
    /// </summary>
    /// <returns>A list of Sticker objects, sorted by their pockets' positions.</returns>
    public List<Sticker> GetStickers()
    {
        return GetChildren().Cast<Sticker>().OrderBy(sticker => sticker.Pocket.PocketPosition).ToList();
    }

    public List<Sticker> GetStickersForPocket(Pocket pocket)
    {
        return GetStickers().Where(sticker => sticker.Pocket == pocket).ToList();
    }

}