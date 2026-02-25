using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Enums;
using CodeBase.GamePlay;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.States;
using CodeBase.Logic;
using CodeBase.Services.Enemies;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Randomizer;
using CodeBase.Services.Score;
using CodeBase.Services.StaticData;
using CodeBase.StaticData;
using CodeBase.StaticData.TowerDefense;
using CodeBase.UI.HUD;
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
        private readonly IScoreService _scoreService;
        private readonly IEnemyRegistryService _enemyRegistryService;

        private LevelReferences _levelReferences;
        private TowerDefenseGameConfig _tdConfig;

        private GameObject _enemyHolder;
        private GameObject _waveRunnerGo;

        private GameObject _hudInstance;
        private EndGameHudView _endGameHud;

        private GameObject _tower1Instance;
        private GameObject _tower2Instance;

        private Action _onWin;
        private Action _onLose;

        private readonly Dictionary<CreatureTypeId, Queue<Enemy>> _pool = new Dictionary<CreatureTypeId, Queue<Enemy>>();
        private readonly Dictionary<CreatureTypeId, GameObject> _prefabs = new Dictionary<CreatureTypeId, GameObject>();

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
            IWindowService windowService,
            IScoreService scoreService,
            IEnemyRegistryService enemyRegistryService)
        {
            _assets = assets;
            _staticData = staticData;
            _randomService = randomService;
            _persistentProgressService = persistentProgressService;
            _windowService = windowService;
            _scoreService = scoreService;
            _enemyRegistryService = enemyRegistryService;
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

            CreateHudIfNeeded();
            CreateTowersIfNeeded();
        }

        public void CreateEnemyWaves(LevelStaticData levelStaticData, Action onWin, Action onLose)
        {
            _onWin = onWin;
            _onLose = onLose;

            EnsureEnemyHolder();
            EnsureWaveRunner();

            ResetWaveState();
            _scoreService.Reset();

            WaveRunner runner = _waveRunnerGo.GetComponent<WaveRunner>();
            runner.Init(levelStaticData, SpawnFromWave, OnAllWavesSpawned);
            runner.StartWaves();
        }

        public void ShowEndGameOverlay(bool isWin)
        {
            if (_endGameHud == null)
                return;

            if (isWin)
                _endGameHud.ShowWin();
            else
                _endGameHud.ShowLose();
        }

        private void ResetWaveState()
        {
            _aliveEnemies = 0;
            _allWavesSpawned = false;
            _isGameOver = false;

            if (_enemyRegistryService != null)
                _enemyRegistryService.Registry.Clear();

            if (_endGameHud != null)
                _endGameHud.HideAll();
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
            _enemyRegistryService.Registry.Register(enemy);

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
            _scoreService.Add(coins);

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

            _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
            _enemyRegistryService.Registry.Unregister(enemy);
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
                case CreatureTypeId.Lancer:
                case CreatureTypeId.Tree:
                    return go.GetComponent<EnemyAttacker>().Init(
                        data.speed, data.coinsPerKill, data.health,
                        data.attackPower, data.attackDelay, data.stopDistance,
                        target, data.dieTime);

                case CreatureTypeId.Golem:
                    return go.GetComponent<EnemyAttacker>().Init(
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
                case CreatureTypeId.Ork: return AssetAddress.OrkEnemy;
                case CreatureTypeId.Golem: return AssetAddress.GolemEnemy;
                case CreatureTypeId.Lancer: return AssetAddress.LancerEnemy;
                case CreatureTypeId.Tree: return AssetAddress.TreeEnemy;
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

        private async void CreateHudIfNeeded()
        {
            if (_hudInstance != null)
                return;

            if (_levelReferences == null || _levelReferences.UiRoot == null)
                return;

            GameObject hudPrefab = await _assets.Load<GameObject>(AssetAddress.HUDPath);
            if (hudPrefab == null)
            {
                Debug.LogError("HUD prefab not found by address");
                return;
            }

            _hudInstance = Object.Instantiate(hudPrefab, _levelReferences.UiRoot);

            HUD scoreView = _hudInstance.GetComponentInChildren<HUD>(true);
            if (scoreView != null)
                scoreView.Init(_scoreService, _levelReferences.CastleTarget);

            _endGameHud = _hudInstance.GetComponentInChildren<EndGameHudView>(true);
            if (_endGameHud != null)
                _endGameHud.HideAll();
        }

        private async void CreateTowersIfNeeded()
        {
            if (_tower1Instance != null || _tower2Instance != null)
                return;

            if (_levelReferences == null)
                return;

            if (_levelReferences.Tower1SpawnPoint != null)
                _tower1Instance = await CreateTower(_levelReferences.Tower1SpawnPoint, TowerTypeId.Tower1, AssetAddress.Tower1);

            if (_levelReferences.Tower2SpawnPoint != null)
                _tower2Instance = await CreateTower(_levelReferences.Tower2SpawnPoint, TowerTypeId.Tower2, AssetAddress.Tower2);
        }

        private async Task<GameObject> CreateTower(Transform point, TowerTypeId id, string address)
        {
            GameObject prefab = await _assets.Load<GameObject>(address);
            if (prefab == null)
            {
                Debug.LogError($"Tower prefab not found: {address}");
                return null;
            }

            TowerStaticData data = _staticData.ForTower(id);
            if (data == null)
            {
                Debug.LogError($"TowerStaticData not found for {id}");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, point.position, point.rotation);

            TowerBehaviour tower = instance.GetComponent<TowerBehaviour>();
            if (tower == null)
            {
                Debug.LogError("TowerBehaviour component missing on tower prefab");
                return instance;
            }

            tower.Init(data, _enemyRegistryService.Registry);
            await tower.InitVisuals(_assets);

            return instance;
        }

        public void Cleanup()
        {
            if (_castleCached != null)
                _castleCached.OnDie -= HandleCastleDie;

            StopWaves();

            if (_enemyRegistryService != null)
                _enemyRegistryService.Registry.Clear();

            if (_hudInstance != null)
                Object.Destroy(_hudInstance);
            _hudInstance = null;
            _endGameHud = null;

            if (_tower1Instance != null)
                Object.Destroy(_tower1Instance);
            _tower1Instance = null;

            if (_tower2Instance != null)
                Object.Destroy(_tower2Instance);
            _tower2Instance = null;

            if (_waveRunnerGo != null)
                Object.Destroy(_waveRunnerGo);
            _waveRunnerGo = null;

            _assets.Cleanup();
        }

        public async Task WarmUp()
        {
            await _assets.Load<GameObject>(AssetAddress.HUDPath);

            await _assets.Load<GameObject>(AssetAddress.GolemEnemy);
            await _assets.Load<GameObject>(AssetAddress.OrkEnemy);
            await _assets.Load<GameObject>(AssetAddress.LancerEnemy);
            await _assets.Load<GameObject>(AssetAddress.TreeEnemy);

            await _assets.Load<GameObject>(AssetAddress.Tower1);
            await _assets.Load<GameObject>(AssetAddress.Tower2);
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