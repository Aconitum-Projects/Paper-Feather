using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public enum ButtonAction
    {
        Play,
        SwitchCanvas,
        Quit
    }

    [Header("Action")]
    [SerializeField] private ButtonAction buttonAction;

    [Header("Play")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private GameObject loadingCanvasPrefab;

    [Header("Switch Canvas")]
    [SerializeField] private GameObject canvasToShow;
    [SerializeField] private GameObject[] canvasesToHide;

    private static CoroutineRunner coroutineRunner;

    public void HandleClick()
    {
        switch (buttonAction)
        {
            case ButtonAction.Play:
                GetRunner().StartCoroutine(LoadSceneRoutine());
                break;
            case ButtonAction.SwitchCanvas:
                SwitchCanvas();
                break;
            case ButtonAction.Quit:
                QuitGame();
                break;
        }
    }

    private IEnumerator LoadSceneRoutine()
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            yield break;
        }

        GameObject loadingCanvasInstance = null;
        Slider loadingSlider = null;

        if (loadingCanvasPrefab != null)
        {
            loadingCanvasInstance = Instantiate(loadingCanvasPrefab);
            DontDestroyOnLoad(loadingCanvasInstance);
            loadingSlider = loadingCanvasInstance.GetComponentInChildren<Slider>(true);
            loadingCanvasInstance.SetActive(true);
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = 0f;
        }

        yield return null;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
        {
            if (loadingSlider != null)
            {
                loadingSlider.value = Mathf.Clamp01(loadOperation.progress / 0.9f);
            }

            yield return null;
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = 1f;
        }

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        if (loadingCanvasInstance != null)
        {
            Destroy(loadingCanvasInstance);
        }
    }

    private static CoroutineRunner GetRunner()
    {
        if (coroutineRunner != null)
        {
            return coroutineRunner;
        }

        GameObject runnerObject = new GameObject(nameof(MenuButton) + "CoroutineRunner");
        DontDestroyOnLoad(runnerObject);
        coroutineRunner = runnerObject.AddComponent<CoroutineRunner>();
        return coroutineRunner;
    }

    private void SwitchCanvas()
    {
        if (canvasesToHide != null)
        {
            foreach (GameObject canvas in canvasesToHide)
            {
                if (canvas != null)
                {
                    canvas.SetActive(false);
                }
            }
        }

        if (canvasToShow != null)
        {
            canvasToShow.SetActive(true);
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private sealed class CoroutineRunner : MonoBehaviour
    {
    }
}
