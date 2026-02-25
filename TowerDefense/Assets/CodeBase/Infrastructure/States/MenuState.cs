using System.Collections.Generic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Logic;
using CodeBase.Menu;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class MenuState : IPayloadedState<bool>
    {
        private GameStateMachine _gameStateMachine;
        private MainMenuWindow _menuWindow;
        private IUIFactory _uiFactory;
        private readonly LoadingCurtain _loadingCurtain;
        private SceneLoader _sceneLoader;
        private const string Initial = "Initial";

        public MenuState(GameStateMachine gameStateMachine, IUIFactory uiFactory, LoadingCurtain loadingCurtain,
            SceneLoader sceneLoader)
        {
            _gameStateMachine = gameStateMachine;
            _uiFactory = uiFactory;
            _loadingCurtain = loadingCurtain;
            _sceneLoader = sceneLoader;
        }

        public void Enter(bool isGameRun)
        {
            _loadingCurtain.Show();
            List<MenuButtons> menuButtonsList = new List<MenuButtons>();
            if (isGameRun)
            {
                menuButtonsList.Add(new MenuButtons("Continue", () =>
                {
                    _menuWindow.Close();
                    _gameStateMachine.Enter<GameLoopState>();
                    
                }));
            }

            menuButtonsList.Add(new MenuButtons("New Game", () =>
            {
                
                _menuWindow.Close();
                _loadingCurtain.Show();
                _sceneLoader.Load(Initial, onLoaded: () =>
                {
                   
                    _gameStateMachine.Enter<LoadProgressState>();
                }  );
               
            }));

            menuButtonsList.Add(new MenuButtons("Exit Game", () => { Application.Quit(); }));

            _menuWindow = _uiFactory.CreateMainMenu(menuButtonsList, isGameRun);
            _loadingCurtain.Hide();
        }

        public void Exit()
        {
            _loadingCurtain.Show();
        }

        public bool IsOnPause()
        {
            return true;
        }
    }
}