using System;
using System.Collections.Generic;
using CodeBase.Enums;
using CodeBase.StaticData.Windows;

namespace CodeBase.StaticData.Windows
{
    [Serializable]
    public class WindowConfig
    {
        public WindowId WindowId;
        public WindowBase Template;
    }
}