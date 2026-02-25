using System;
using CodeBase.StaticData;

namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public GameData gameData;


        public PlayerProgress()
        {
            gameData = new GameData(new PlayerData(0,100,100,0,10,"0"));
        }
    }
}