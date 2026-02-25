using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Randomizer;
using CodeBase.Services.SaveLoad;
using CodeBase.Services.Score;
using CodeBase.Services.StaticData;
using CodeBase.StaticData;
using CodeBase.UI.Services.Factory;
using CodeBase.UI.Windows;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class BootstrapState : IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _services;
        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices allServices)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _services = allServices;
            RegisterServices();
        }

        public void Enter() =>
            _sceneLoader.Load(Initial, onLoaded:EnterMainMenu );

        public void Exit()
        {
        }

        public bool IsOnPause()
        {
            return true;
        }

        private void RegisterServices()
        {
            _services.RegisterSingle<IGameStateMachine>(_stateMachine);

            RegisterInputService();
            RegisterStaticDataService();
            RegisterAssetProvider();
            _services.RegisterSingle<IRandomService>(new RandomService());
            _services.RegisterSingle<IPersistentProgressService>(new PersistentProgressService());
            _services.RegisterSingle<IScoreService>(new ScoreService());
            _services.RegisterSingle<IUIFactory>(new UIFactory(
                _services.Single<IAssetProvider>(),
                _services.Single<IStaticDataService>(),
                _services.Single<IPersistentProgressService>(),_stateMachine,_services.Single<IInputService>()));
            _services.RegisterSingle<IWindowService>(new WindowService(_services.Single<IUIFactory>()));
            _services.RegisterSingle<IGameFactory>(new GameFactory(
                _services.Single<IAssetProvider>(),
                _services.Single<IStaticDataService>(),
                _services.Single<IRandomService>(),
                _services.Single<IPersistentProgressService>(),
                _services.Single<IGameStateMachine>(),_services.Single<IWindowService>(),_services.Single<IScoreService>()
            ));
            _services.RegisterSingle<ISaveLoadService>(new SaveLoadService(
                _services.Single<IPersistentProgressService>(),
                _services.Single<IGameFactory>()));
            
        }

        private void RegisterInputService()
        {
            _services.RegisterSingle<IInputService>( Object.Instantiate(new GameObject("InputService")).AddComponent<InputService>());
            _services.Single<IInputService>().Construct(_stateMachine,_services.Single<IPersistentProgressService>());
        }

        private void RegisterStaticDataService()
        {
            IStaticDataService staticData = new StaticDataService();
             staticData.Load();
            _services.RegisterSingle(staticData);
        }
        private void RegisterAssetProvider()
        {
            AssetProvider assetProvider = new AssetProvider();
            _services.RegisterSingle<IAssetProvider>(assetProvider);
            assetProvider.Initialize();
        }
        private void EnterMainMenu() =>
            _stateMachine.Enter<MenuState,bool>(false);
    }
}