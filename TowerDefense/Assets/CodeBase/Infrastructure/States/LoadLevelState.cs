using System.Threading.Tasks;
using CodeBase.Enums;
using CodeBase.Infrastructure.Factory;
using CodeBase.Logic;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData;
using CodeBase.UI.Windows;
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
        private readonly IInputService _inputService;
        private readonly IWindowService _windowService;

        private PlayerData _playerData;

        public LoadLevelState(
            GameStateMachine gameStateMachine,
            SceneLoader sceneLoader,
            LoadingCurtain loadingCurtain,
            IGameFactory gameFactory,
            IPersistentProgressService progressService,
            IStaticDataService staticDataService,
            IInputService inputService,
            IWindowService windowService)
        {
            _stateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _gameFactory = gameFactory;
            _progressService = progressService;
            _staticData = staticDataService;
            _inputService = inputService;
            _windowService = windowService;
        }

        public void Enter(string sceneName)
        {
            Time.timeScale = 1f;

            _loadingCurtain.Show();
            _gameFactory.Cleanup();

            _sceneLoader.Load(sceneName, OnLoaded);
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
            if (levelData == null)
            {
                Debug.LogError($"LevelStaticData not found for key: {_playerData.CurrentLevel}");
                return;
            }

            await _gameFactory.WarmUp();

            LevelReferences refs = Object.FindObjectOfType<LevelReferences>();
            if (refs == null)
            {
                Debug.LogError("LevelReferences not found on scene");
                return;
            }

            _gameFactory.SetLevelReferences(refs);
            _gameFactory.CreateEnemyWaves(levelData, HandleWin, HandleLose);
        }

        private void HandleWin()
        {
            Time.timeScale = 0f;
            _windowService.Open(WindowId.MainMenu);
            Debug.Log("WIN: All waves completed");
        }

        private void HandleLose()
        {
            Time.timeScale = 0f;
            _windowService.Open(WindowId.MainMenu);
            Debug.Log("LOSE: Castle destroyed");
        }
    }
}