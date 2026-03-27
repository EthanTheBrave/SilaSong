using SilksongIC;

namespace SilksongRando.IC.Items
{
    /// <summary>
    /// Grants a Heart Piece. Four pieces = one extra heart of max health.
    /// PlayerData.heartPieces field confirmed from decompiled Assembly-CSharp.
    /// IncrementInt("heartPieces") increments it via the reflection-based setter.
    /// </summary>
    public class HeartPieceItem : AbstractItem
    {
        public override void GiveItem(GiveInfo info)
        {
            PlayerData.instance.IncrementInt("heartPieces");
            RandoPlugin.Logger.LogInfo("[HeartPieceItem] Incremented heartPieces.");
        }
    }
}
