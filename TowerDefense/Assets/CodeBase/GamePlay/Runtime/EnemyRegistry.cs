using System;
using System.Collections.Generic;

namespace CodeBase.GamePlay.Enemys
{
    public class EnemyRegistry
    {
        private readonly List<Enemy> _active = new List<Enemy>();

        public IReadOnlyList<Enemy> Active => _active;

        public event Action<Enemy> OnRegistered;
        public event Action<Enemy> OnUnregistered;

        public void Register(Enemy enemy)
        {
            if (enemy == null)
                return;

            if (_active.Contains(enemy))
                return;

            _active.Add(enemy);
            OnRegistered?.Invoke(enemy);
        }

        public void Unregister(Enemy enemy)
        {
            if (enemy == null)
                return;

            if (_active.Remove(enemy))
                OnUnregistered?.Invoke(enemy);
        }

        public void Clear()
        {
            _active.Clear();
        }
    }
}