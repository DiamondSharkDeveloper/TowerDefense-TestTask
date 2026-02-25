using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Enums;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Randomizer;
using CodeBase.Services.SaveLoad;
using CodeBase.Services.StaticData;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.States
{
    public class LoadProgressState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly ISaveLoadService _saveLoadProgress;
        private readonly IStaticDataService _staticDataService;
        private readonly IRandomService _randomService;
        private const string MainScene = "Village";


        public LoadProgressState(GameStateMachine gameStateMachine, IStaticDataService staticDataService,
            IPersistentProgressService progressService,
            ISaveLoadService saveLoadProgress, IRandomService randomService)
        {
            _gameStateMachine = gameStateMachine;
            _progressService = progressService;
            _saveLoadProgress = saveLoadProgress;
            _staticDataService = staticDataService;
            _randomService = randomService;
        }

        public void Enter()
        {
            LoadProgressOrInitNew();
            _gameStateMachine.Enter<LoadLevelState, string>(MainScene);
        }

        public void Exit()
        {
        }

        public bool IsOnPause()
        {
            return true;
        }

        private void LoadProgressOrInitNew()
        {
            _progressService.Progress =
                _saveLoadProgress.LoadProgress()
                ?? NewProgress();
        }

        private PlayerProgress NewProgress()
        {
            PlayerProgress progress = new PlayerProgress();
            return progress;
        }
    }
}