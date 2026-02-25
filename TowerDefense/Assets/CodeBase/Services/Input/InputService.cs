using System;
using CodeBase.Infrastructure.States;
using CodeBase.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Services.Input
{
    public class InputService : MonoBehaviour, IInputService
    {
        private IGameStateMachine _stateMachine;
        private bool _canRayCast;
        public event Action OnMouseButtonDown;
        private Camera _camera;


        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Construct(IGameStateMachine stateMachine, IPersistentProgressService persistentProgressService)
        {
            _stateMachine = stateMachine;
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }
    }
}