using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Menu;
using CodeBase.Services;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Factory
{
    public interface IUIFactory: IService
    {
        Task CreateUIRoot();
        MainMenuWindow CreateMainMenu(List<MenuButtons> menuButtonsList,bool isGameRun);
    }
}