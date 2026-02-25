using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Logic;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class LoadLevelState : IPayloadedState<string>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;
        private readonly IGameFactory _gameFactory;
        private readonly IPersistentProgressService _progressService;
        private readonly IStaticDataService _staticData;
        private PlayerData _playerData;

        private readonly IInputService _inputService;

        public LoadLevelState(GameStateMachine gameStateMachine,
            SceneLoader sceneLoader,
            LoadingCurtain loadingCurtain,
            IGameFactory gameFactory,
            IPersistentProgressService progressService,
            IStaticDataService staticDataService,
            IInputService inputService)
        {
            _stateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _gameFactory = gameFactory;
            _progressService = progressService;
            _staticData = staticDataService;
            _inputService = inputService;
        }

        public void Enter(string isGameRun)
        {
            _loadingCurtain.Show();
            _gameFactory.Cleanup();
            _gameFactory.WarmUp();
            _sceneLoader.Load(isGameRun, OnLoaded);
        }

        public void Exit() =>
            _loadingCurtain.Hide();

        public bool IsOnPause() =>
            true;

        private async void OnLoaded()
        {
            await InitLevel();
            _stateMachine.Enter<GameLoopState>();
        }

        private async Task InitLevel()
        {
            _playerData = _progressService.Progress.gameData.PlayerData;

            LevelStaticData levelData = _staticData.ForLevel(_playerData.CurrentLevel);

            LevelReferences refs = Object.FindObjectOfType<LevelReferences>();
            if (refs == null)
            {
                Debug.LogError("LevelReferences not found on scene");
                return;
            }

            _gameFactory.SetLevelReferences(refs);
            _gameFactory.CreateEnemyWaves(levelData, () => { }, () => { });

            await Task.CompletedTask;
        }
    }
}