using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class InteractableController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup uiCanvasGroup;
    public RectTransform uiRectTransform;
    public Camera mainCamera;

    [Space(20)]
    [Header("Animation")]
    public float showScale = 1f;
    public float hideScale = 0f;
    public float animDuration = 0.3f;
    public Ease animEase = Ease.OutBack;

    [Space(20)]
    [Header("Interaction")]
    public Key interactKey = Key.E;
    public DialogueSequence dialogueToStart;

    [Space(20)]
    private bool uiVisible = false;

    private void Start()
    {

        if (mainCamera == null)
            mainCamera = Camera.main;

        uiRectTransform.localScale = Vector3.one * hideScale;
        uiCanvasGroup.alpha = 0f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        var interactControl = keyboard != null ? keyboard[interactKey] : null;

        if (uiVisible && interactControl != null && interactControl.wasPressedThisFrame)
        {
            DialogueManager dm = FindObjectsByType<DialogueManager>()[0];
            
            if (dm != null)
            {
                dm.StartDialogue(dialogueToStart);
            }
            else
            {
                Debug.LogWarning("Aucun DialogueManager trouvé dans la scène !");
            }
        }
    }

    private void LateUpdate()
    {
        if (uiVisible)
        {
            Vector3 lookPos = mainCamera.transform.position;
            lookPos.y = uiRectTransform.position.y;
            uiRectTransform.LookAt(lookPos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            ShowUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            HideUI();
    }

    private void ShowUI()
    {
        uiVisible = true;
        uiRectTransform.DOScale(showScale, animDuration).SetEase(animEase);
        uiCanvasGroup.DOFade(1f, animDuration);
        uiCanvasGroup.interactable = true;
        uiCanvasGroup.blocksRaycasts = true;
    }

    private void HideUI()
    {
        uiVisible = false;
        uiRectTransform.DOScale(hideScale, animDuration).SetEase(animEase);
        uiCanvasGroup.DOFade(0f, animDuration);
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;
    }
}