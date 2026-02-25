using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Enums;
using CodeBase.GamePlay.Enemys;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.States;
using CodeBase.Logic;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Randomizer;
using CodeBase.Services.StaticData;
using CodeBase.StaticData;
using CodeBase.UI.Windows;
using Models.New_Enemy.Scripts;
using UnityEngine;
using Object = UnityEngine.Object;
using CodeBase.StaticData.TowerDefense;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;
        private readonly IRandomService _randomService;
        private readonly IPersistentProgressService _persistentProgressService;
        private readonly IWindowService _windowService;

        private GameObject _enemyHolder;
        private LevelStaticData _currentLevelData;

        private Action _onWin;
        private Action _onLose;

        private LevelReferences _levelReferences;
        private TowerDefenseGameConfig _tdConfig;

        private readonly Dictionary<CreatureTypeId, Queue<Enemy>> _pool = new Dictionary<CreatureTypeId, Queue<Enemy>>();
        private readonly Dictionary<CreatureTypeId, GameObject> _prefabs = new Dictionary<CreatureTypeId, GameObject>();

        private int _enemyOnLevel;
        private readonly List<Enemy> _activeEnemies = new List<Enemy>();

        public GameFactory(
            IAssetProvider assets,
            IStaticDataService staticData,
            IRandomService randomService,
            IPersistentProgressService persistentProgressService,
            IGameStateMachine stateMachine,
            IWindowService windowService)
        {
            _assets = assets;
            _staticData = staticData;
            _randomService = randomService;
            _persistentProgressService = persistentProgressService;
            _windowService = windowService;
        }

        public void SetLevelReferences(LevelReferences references)
        {
            _levelReferences = references;

            _tdConfig = _staticData.TowerDefenseConfig();
            if (_tdConfig == null)
            {
                Debug.LogError("TowerDefenseGameConfig not found. Place it under Resources/Static Data/TowerDefense/");
                return;
            }

            CastleTarget castle = _levelReferences.CastleTarget as CastleTarget;
            if (castle != null)
                castle.Init(_tdConfig.castleMaxHp, _tdConfig.castleDieTime);
        }

        public void CreateEnemyWaves(LevelStaticData levelStaticData, Action onWin, Action onLose)
        {
            _currentLevelData = levelStaticData;
            _onWin = onWin;
            _onLose = onLose;

            if (_enemyHolder == null)
                _enemyHolder = Object.Instantiate(new GameObject("EnemyHolder"));

            _enemyOnLevel = 0;
            _activeEnemies.Clear();

            foreach (EnemyWaveData enemyWaveData in levelStaticData.EnemyWaves)
                CreateEnemyWave(enemyWaveData);
        }

        private async void CreateEnemyWave(EnemyWaveData enemyWaveData)
        {
            await Task.Delay((int)(enemyWaveData.AppearanceTime * 1000));

            foreach (CreatureOnWaveData creatureOnWaveData in enemyWaveData.CreatureOnWaveData)
            {
                // Пока в этом тесте используем только Ork и Golem, остальные просто пропускаем
                if (creatureOnWaveData._typeId != CreatureTypeId.Ork &&
                    creatureOnWaveData._typeId != CreatureTypeId.Golem)
                    continue;

                for (int i = 0; i < creatureOnWaveData.CreatureCount; i++)
                {
                    Enemy enemy = await CreateCreature(creatureOnWaveData._typeId);
                    if (enemy == null)
                        continue;

                    _enemyOnLevel++;

                    enemy.OnDie += coins =>
                    {
                        _enemyOnLevel--;
                        _persistentProgressService.Progress.gameData.PlayerData.BattleCoins += coins;
                        _activeEnemies.Remove(enemy);
                        CheckWin();
                    };

                    _activeEnemies.Add(enemy);
                }
            }
        }

        private void CheckWin()
        {
            if (_enemyOnLevel == 0)
                _onWin?.Invoke();
        }

        private Vector3 SpawnPosition()
        {
            if (_levelReferences != null && _levelReferences.EnemySpawnPoint != null)
                return _levelReferences.EnemySpawnPoint.position;

            return Vector3.zero;
        }

        public async Task<Enemy> CreateCreature(CreatureTypeId typeId)
        {
            if (_levelReferences == null || _levelReferences.CastleTarget == null)
            {
                Debug.LogError("GameFactory: LevelReferences or CastleTarget is not set");
                return null;
            }

            MonsterStaticData data = _staticData.ForMonster(typeId);

            Enemy enemy = TakeFromPool(typeId);
            if (enemy == null)
            {
                GameObject prefab = await GetPrefab(typeId);
                if (prefab == null)
                    return null;

                GameObject go = InstantiateRegistered(prefab, SpawnPosition(), _enemyHolder.transform);

                SetupPooledObject(go, typeId);

                enemy = InitEnemyByType(typeId, go, data, _levelReferences.CastleTarget);
                if (enemy == null)
                    return null;
            }
            else
            {
                Transform t = enemy.transform;
                t.SetParent(_enemyHolder.transform);
                t.position = SpawnPosition();
                enemy.gameObject.SetActive(true);

                
                InitEnemyByType(typeId, enemy.gameObject, data, _levelReferences.CastleTarget);
            }

           
            EnemyGoal goal = enemy.GetComponent<EnemyGoal>();
            if (goal == null)
                goal = enemy.gameObject.AddComponent<EnemyGoal>();

            float reachDistance = _tdConfig != null ? _tdConfig.enemyReachDistance : Mathf.Max(0.25f, data.stopDistance);
            goal.Init(enemy, _levelReferences.CastleTarget, data.attackPower, reachDistance);

            return enemy;
        }

        private Enemy InitEnemyByType(CreatureTypeId typeId, GameObject go, MonsterStaticData data, Destructible target)
        {
            switch (typeId)
            {
                case CreatureTypeId.Ork:
                    return go.GetComponent<EnemyAttacker>().Init(
                        data.speed, data.coinsPerKill, data.health,
                        data.attackPower, data.attackDelay, data.stopDistance,
                        target, data.dieTime);

                case CreatureTypeId.Golem:
                    return go.GetComponent<EnemyShooter>().Init(
                        data.speed, data.coinsPerKill, data.health,
                        data.attackPower, data.attackDelay, data.stopDistance,
                        target, data.dieTime);
            }

            return null;
        }

        private async Task<GameObject> GetPrefab(CreatureTypeId typeId)
        {
            if (_prefabs.TryGetValue(typeId, out GameObject prefab) && prefab != null)
                return prefab;

            string address = AddressByType(typeId);
            GameObject loaded = await _assets.Load<GameObject>(address);
            _prefabs[typeId] = loaded;
            return loaded;
        }

        private string AddressByType(CreatureTypeId typeId)
        {
            switch (typeId)
            {
                case CreatureTypeId.Ork:
                    return AssetAddress.OrkEnemy;
                case CreatureTypeId.Golem:
                    return AssetAddress.GolemEnemy;
            }

            return AssetAddress.OrkEnemy;
        }

        private Enemy TakeFromPool(CreatureTypeId typeId)
        {
            if (_pool.TryGetValue(typeId, out Queue<Enemy> q) && q.Count > 0)
                return q.Dequeue();

            return null;
        }

        private void ReturnToPool(CreatureTypeId typeId, GameObject go)
        {
            if (go == null)
                return;

            Enemy enemy = go.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnDie = null; 
            }

            go.transform.SetParent(_enemyHolder.transform);
            go.SetActive(false);

            if (!_pool.TryGetValue(typeId, out Queue<Enemy> q))
            {
                q = new Queue<Enemy>();
                _pool[typeId] = q;
            }

            q.Enqueue(go.GetComponent<Enemy>());
        }

        private void SetupPooledObject(GameObject go, CreatureTypeId typeId)
        {
            PooledObject pooled = go.GetComponent<PooledObject>();
            if (pooled == null)
                pooled = go.AddComponent<PooledObject>();

            pooled.SetReturnAction(obj => ReturnToPool(typeId, obj));
        }

        public void Cleanup()
        {
            _assets.Cleanup();
        }

        public async Task WarmUp()
        {
            await _assets.Load<GameObject>(AssetAddress.HUDPath);
            
            await _assets.Load<GameObject>(AssetAddress.GolemEnemy);
            await _assets.Load<GameObject>(AssetAddress.OrkEnemy);
        }

        private GameObject InstantiateRegistered(GameObject prefab, Vector3 at, Transform parent)
        {
            GameObject gameObject = Object.Instantiate(prefab, at, Quaternion.identity, parent);
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            return gameObject;
        }
    }
}