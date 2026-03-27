using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Grants a Heart Piece. Four pieces = one extra heart of max health.
    /// Increments heartPiecesCollected; the game handles the threshold internally.
    /// </summary>
    public class HeartPieceItem : AbstractItem
    {
        public override void GiveItem(GiveInfo info)
        {
            PlayerData.instance.IncrementInt("heartPiecesCollected");
            RandoPlugin.Logger.LogInfo("[HeartPieceItem] Gave heart piece.");
        }
    }
}
