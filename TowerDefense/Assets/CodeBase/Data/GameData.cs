using System;
using CodeBase.StaticData;

namespace CodeBase.Data
{
    [Serializable]
    public class GameData
    {
        public PlayerData PlayerData;

        public GameData(PlayerData playerData)
        {
            PlayerData = playerData;
        }
    }
}