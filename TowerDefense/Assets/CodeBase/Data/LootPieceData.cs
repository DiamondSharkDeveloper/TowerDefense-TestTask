using System;

namespace CodeBase.Data
{
    [Serializable]
    public class LootPieceData
    {
        public Loot loot;

        public LootPieceData( Loot loot)
        {
            this.loot = loot;
        }
    }
}