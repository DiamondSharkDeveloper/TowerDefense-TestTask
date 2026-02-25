using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Enums;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.States;
using CodeBase.Menu;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Windows;
using UnityEngine;

namespace CodeBase.UI.Services.Factory
{
    public class UIFactory : IUIFactory
    {
        private const string UIRootPath = "UIRoot";
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;
        private IGameStateMachine _stateMachine;
        private Transform _uiRoot;
        private readonly IPersistentProgressService _progressService;
      
        private readonly IInputService _inputService;

        public UIFactory(IAssetProvider assets, IStaticDataService staticData,
            IPersistentProgressService progressService, IGameStateMachine stateMachine,
            IInputService inputService)
        {
            _assets = assets;
            _staticData = staticData;
            _progressService = progressService;
            _stateMachine = stateMachine;
            _inputService = inputService;
        }

        public async Task CreateUIRoot()
        {
            GameObject root = await _assets.Instantiate(UIRootPath);
            _uiRoot = root.transform;
        }

        public MainMenuWindow CreateMainMenu(List<MenuButtons> menuButtonsList, bool isGameRun)
        {
            WindowConfig config = _staticData.ForWindow(WindowId.MainMenu);
            MainMenuWindow window = Object.Instantiate(config.Template, _uiRoot) as MainMenuWindow;
            if (window != null)
            {
                window.Construct(_progressService, () => { });
                window.Init(menuButtonsList, isGameRun);
            }

            return window;
        }
    }
}