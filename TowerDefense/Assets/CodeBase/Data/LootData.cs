using System;

namespace CodeBase.Data
{
    [Serializable]
    public class LootData
    {
        public LootPieceDataDictionary lootPiecesInDataDictionary = new LootPieceDataDictionary();
        public Loot _onHoldLoot;
        public Action<Loot> Changed;

        public void Collect(Loot loot)
        {
            if (lootPiecesInDataDictionary.Dictionary.ContainsKey(loot.name))
            {
                lootPiecesInDataDictionary.Dictionary[loot.name].value += loot.value;
            }
            else
            {
                lootPiecesInDataDictionary.Dictionary[loot.name] = loot;
            }

            Changed?.Invoke(loot);
        }

        public void Hold(string id)
        {
            _onHoldLoot = lootPiecesInDataDictionary.Dictionary[id];
            lootPiecesInDataDictionary.Dictionary[_onHoldLoot.name].value--;
        }

        public void Use()
        {
            _onHoldLoot=null;
        }

        public void LetGo()
        {
            lootPiecesInDataDictionary.Dictionary[_onHoldLoot.name].value++;
            Use();
        }
    }
}