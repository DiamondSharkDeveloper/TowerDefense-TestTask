using System;
using CodeBase.Data;
using CodeBase.Services.PersistentProgress;
using UnityEngine;
using UnityEngine.UI;

public abstract class WindowBase : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    protected Action _onClose;
    
    protected IPersistentProgressService ProgressService;
    protected PlayerProgress Progress => ProgressService.Progress;

    public void Construct(IPersistentProgressService progressService,Action onClose)
    {
        ProgressService = progressService;
        _onClose = onClose;
    }
       

    private void Awake() => 
        OnAwake();

    private void Start()
    {
        Initialize();
        SubscribeUpdates();
    }

    private void OnDestroy() => 
        Cleanup();

    protected virtual void OnAwake() => 
        closeButton?.onClick.AddListener(Close);

    protected virtual void Initialize(){}
    protected virtual void SubscribeUpdates(){}
    protected virtual void Cleanup(){}

    public void Close()
    {
        _onClose?.Invoke();
        Destroy(gameObject);
    }
}