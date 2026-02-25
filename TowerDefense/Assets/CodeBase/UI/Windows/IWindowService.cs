using System;
using CodeBase.Enums;
using CodeBase.Services;
using CodeBase.StaticData;

namespace CodeBase.UI.Windows
{
    public interface IWindowService : IService
    {
        void Open(WindowId windowId);
    }
}