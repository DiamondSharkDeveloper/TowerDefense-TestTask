using System.Collections.Generic;
using System.Linq;
using CodeBase.Enums;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.TowerDefense;
using CodeBase.StaticData.Windows;
using UnityEngine;

namespace CodeBase.StaticData
{
    internal class StaticDataService : IStaticDataService
    {
        private const string LevelsDataPath = "Static Data/Levels";
        private const string MonstersDataPath = "Static Data/Monsters";
        private const string StaticDataWindowPath = "Static Data/UI/WindowStaticData";
        private const string TowerDefenseConfigPath = "Static Data/TowerDefense/TowerDefenseGameConfig";

        private Dictionary<string, LevelStaticData> _levels;
        private Dictionary<CreatureTypeId, MonsterStaticData> _monsterStaticDatas;
        private Dictionary<WindowId, WindowConfig> _windowConfigs;

        private TowerDefenseGameConfig _towerDefenseConfig;

        public void Load()
        {
            _windowConfigs = Resources
                .Load<WindowStaticData>(StaticDataWindowPath)
                .configs
                .ToDictionary(x => x.WindowId, x => x);

            _levels = Resources
                .LoadAll<LevelStaticData>(LevelsDataPath)
                .ToDictionary(x => x.levelKey, x => x);

            _monsterStaticDatas = Resources
                .LoadAll<MonsterStaticData>(MonstersDataPath)
                .ToDictionary(x => x.id, x => x);

            _towerDefenseConfig = Resources.Load<TowerDefenseGameConfig>(TowerDefenseConfigPath);
        }

        public MonsterStaticData ForMonster(CreatureTypeId typeId) =>
            _monsterStaticDatas.TryGetValue(typeId, out MonsterStaticData staticData)
                ? staticData
                : null;

        public LevelStaticData ForLevel(string sceneKey) =>
            _levels.TryGetValue(sceneKey, out LevelStaticData staticData)
                ? staticData
                : null;

        public WindowConfig ForWindow(WindowId windowId) =>
            _windowConfigs.TryGetValue(windowId, out WindowConfig windowConfig)
                ? windowConfig
                : null;

        public TowerDefenseGameConfig TowerDefenseConfig() =>
            _towerDefenseConfig;
    }
}