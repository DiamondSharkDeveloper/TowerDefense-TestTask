using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner) =>
            _coroutineRunner = coroutineRunner;

        public void Load(string name, Action onLoaded = null) =>
            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));

        public void LoadAdditive(string name, Action<Scene> onLoaded = null) =>
            _coroutineRunner.StartCoroutine(LoadAdditiveScene(name, onLoaded));

        public void UpLoadAdditive(string name, Action onLoaded = null) =>
            _coroutineRunner.StartCoroutine(UpLoadAdditiveScene(name, onLoaded));

        private IEnumerator LoadScene(string nextScene, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene)
            {
                onLoaded?.Invoke();
                yield break;
            }

            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);

            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke();
        }

        private IEnumerator LoadAdditiveScene(string additiveScene, Action<Scene> onLoaded = null)
        {
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(additiveScene, LoadSceneMode.Additive);

            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke(SceneManager.GetSceneByName(additiveScene));
        }

        private IEnumerator UpLoadAdditiveScene(string additiveScene, Action onUpLoaded = null)
        {
            AsyncOperation waitNextScene = SceneManager.UnloadSceneAsync(additiveScene);

            while (!waitNextScene.isDone)
                yield return null;

            onUpLoaded?.Invoke();
        }
    }
}