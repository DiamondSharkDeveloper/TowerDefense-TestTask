using System;

namespace CodeBase.Menu
{
    public class MenuButtons
    {
        public string Name;
        public Action Action;

        public MenuButtons(string name, Action action)
        {
            Name = name;
            Action = action;
        }
    }
}