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
using CodeBase.StaticData.TowerDefense;
using CodeBase.UI.Windows;
using Models.New_Enemy.Scripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;
        private readonly IRandomService _randomService;
        private readonly IPersistentProgressService _persistentProgressService;
        private readonly IWindowService _windowService;

        private LevelReferences _levelReferences;
        private TowerDefenseGameConfig _tdConfig;

        private GameObject _enemyHolder;
        private GameObject _waveRunnerGo;

        private Action _onWin;
        private Action _onLose;

        private readonly Dictionary<CreatureTypeId, Queue<Enemy>> _pool = new Dictionary<CreatureTypeId, Queue<Enemy>>();
        private readonly Dictionary<CreatureTypeId, GameObject> _prefabs = new Dictionary<CreatureTypeId, GameObject>();

        private readonly List<Enemy> _activeEnemies = new List<Enemy>();
        private readonly EnemyRegistry _registry = new EnemyRegistry();

        private int _aliveEnemies;
        private bool _allWavesSpawned;
        private bool _isGameOver;

        private CastleTarget _castleCached;

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

            CacheAndInitCastle();
        }

        public void CreateEnemyWaves(LevelStaticData levelStaticData, Action onWin, Action onLose)
        {
            _onWin = onWin;
            _onLose = onLose;

            EnsureEnemyHolder();
            EnsureWaveRunner();

            ResetWaveState();

            WaveRunner runner = _waveRunnerGo.GetComponent<WaveRunner>();
            runner.Init(levelStaticData, SpawnFromWave, OnAllWavesSpawned);
            runner.StartWaves();
        }

        private void ResetWaveState()
        {
            _aliveEnemies = 0;
            _activeEnemies.Clear();
            _registry.Clear();

            _allWavesSpawned = false;
            _isGameOver = false;
        }

        private void CacheAndInitCastle()
        {
            if (_levelReferences == null)
                return;

            _castleCached = _levelReferences.CastleTarget as CastleTarget;

            if (_castleCached != null)
            {
                _castleCached.Init(_tdConfig.castleMaxHp, _tdConfig.castleDieTime);

                _castleCached.OnDie -= HandleCastleDie;
                _castleCached.OnDie += HandleCastleDie;
            }
        }

        private void HandleCastleDie(int _)
        {
            TriggerLose();
        }

        private void TriggerLose()
        {
            if (_isGameOver)
                return;

            _isGameOver = true;
            StopWaves();
            _onLose?.Invoke();
        }

        private void TriggerWin()
        {
            if (_isGameOver)
                return;

            _isGameOver = true;
            StopWaves();
            _onWin?.Invoke();
        }

        private void StopWaves()
        {
            if (_waveRunnerGo == null)
                return;

            WaveRunner runner = _waveRunnerGo.GetComponent<WaveRunner>();
            if (runner != null)
                runner.StopWaves();
        }

        private void OnAllWavesSpawned()
        {
            _allWavesSpawned = true;
            CheckWin();
        }

        private void SpawnFromWave(CreatureTypeId typeId)
        {
            if (_isGameOver)
                return;

            _ = CreateCreature(typeId);
        }

        public async Task<Enemy> CreateCreature(CreatureTypeId typeId)
        {
            if (_isGameOver)
                return null;

            if (_levelReferences == null || _levelReferences.CastleTarget == null)
            {
                Debug.LogError("GameFactory: LevelReferences or CastleTarget is not set");
                return null;
            }

            MonsterStaticData data = _staticData.ForMonster(typeId);
            if (data == null)
            {
                Debug.LogError($"GameFactory: MonsterStaticData not found for {typeId}");
                return null;
            }

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
                {
                    Debug.LogError($"GameFactory: failed to init enemy for {typeId}. Check prefab components.");
                    return null;
                }
            }
            else
            {
                Transform t = enemy.transform;
                t.SetParent(_enemyHolder.transform);
                t.position = SpawnPosition();
                enemy.gameObject.SetActive(true);

                Enemy reinit = InitEnemyByType(typeId, enemy.gameObject, data, _levelReferences.CastleTarget);
                if (reinit == null)
                {
                    Debug.LogError($"GameFactory: failed to re-init enemy for {typeId}. Check prefab components.");
                    return null;
                }

                enemy = reinit;
            }

            RegisterEnemy(enemy);

            EnemyGoal goal = enemy.GetComponent<EnemyGoal>();
            if (goal == null)
                goal = enemy.gameObject.AddComponent<EnemyGoal>();

            float reachDistance = _tdConfig != null ? _tdConfig.enemyReachDistance : Mathf.Max(0.25f, data.stopDistance);
            goal.Init(enemy, _levelReferences.CastleTarget, data.attackPower, reachDistance, HandleEnemyReachedCastle);

            return enemy;
        }

        private void RegisterEnemy(Enemy enemy)
        {
            _aliveEnemies++;
            _activeEnemies.Add(enemy);
            _registry.Register(enemy);

            EnemyLifecycle lifecycle = enemy.GetComponent<EnemyLifecycle>();
            if (lifecycle == null)
                lifecycle = enemy.gameObject.AddComponent<EnemyLifecycle>();

            lifecycle.Init(this, enemy);
        }

        internal void HandleEnemyDied(Enemy enemy, int coins)
        {
            if (_isGameOver)
                return;

            _persistentProgressService.Progress.gameData.PlayerData.BattleCoins += coins;

            UnregisterEnemy(enemy);
            CheckWin();
        }

        private void HandleEnemyReachedCastle(Enemy enemy)
        {
            if (_isGameOver)
                return;

            UnregisterEnemy(enemy);
            CheckWin();
        }

        private void UnregisterEnemy(Enemy enemy)
        {
            if (enemy == null)
                return;

            if (_activeEnemies.Remove(enemy))
            {
                _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
                _registry.Unregister(enemy);
            }
        }

        private void CheckWin()
        {
            if (_isGameOver)
                return;

            if (!_allWavesSpawned)
                return;

            if (_aliveEnemies == 0)
                TriggerWin();
        }

        private void EnsureEnemyHolder()
        {
            if (_enemyHolder == null)
                _enemyHolder = Object.Instantiate(new GameObject("EnemyHolder"));
        }

        private void EnsureWaveRunner()
        {
            if (_waveRunnerGo != null)
                return;

            _waveRunnerGo = Object.Instantiate(new GameObject("WaveRunner"));
            _waveRunnerGo.AddComponent<WaveRunner>();
        }

        private Vector3 SpawnPosition()
        {
            if (_levelReferences != null && _levelReferences.EnemySpawnPoint != null)
                return _levelReferences.EnemySpawnPoint.position;

            return Vector3.zero;
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

                case CreatureTypeId.Lancer:
                    return go.GetComponent<EnemyAttacker>().Init(
                        data.speed, data.coinsPerKill, data.health,
                        data.attackPower, data.attackDelay, data.stopDistance,
                        target, data.dieTime);

                case CreatureTypeId.Tree:
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
                case CreatureTypeId.Lancer:
                    return AssetAddress.LancerEnemy;
                case CreatureTypeId.Tree:
                    return AssetAddress.TreeEnemy;
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
            if (_castleCached != null)
                _castleCached.OnDie -= HandleCastleDie;

            StopWaves();

            _assets.Cleanup();

            if (_waveRunnerGo != null)
                Object.Destroy(_waveRunnerGo);

            _waveRunnerGo = null;
        }

        public async Task WarmUp()
        {
            await _assets.Load<GameObject>(AssetAddress.HUDPath);

            await _assets.Load<GameObject>(AssetAddress.GolemEnemy);
            await _assets.Load<GameObject>(AssetAddress.OrkEnemy);
            await _assets.Load<GameObject>(AssetAddress.LancerEnemy);
            await _assets.Load<GameObject>(AssetAddress.TreeEnemy);
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