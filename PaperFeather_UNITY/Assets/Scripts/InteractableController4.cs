using UnityEngine;
using DG.Tweening;

public class InteractableController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup uiCanvasGroup; // Pour gérer l'opacité
    public RectTransform uiRectTransform; // Pour le scale
    public Camera mainCamera; // Référence à la caméra (si vide, on prendra Camera.main)

    [Header("Animation")]
    public float showScale = 1f;
    public float hideScale = 0f;
    public float animDuration = 0.3f;
    public Ease animEase = Ease.OutBack;

    private bool uiVisible = false;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // On s'assure que c'est caché au début
        uiRectTransform.localScale = Vector3.one * hideScale;
        uiCanvasGroup.alpha = 0f;
        uiCanvasGroup.interactable = false;
        uiCanvasGroup.blocksRaycasts = false;
    }

    private void LateUpdate()
    {
        // Si l'UI est visible, on la tourne vers la caméra
        if (uiVisible)
        {
            Vector3 lookPos = mainCamera.transform.position;
            lookPos.y = uiRectTransform.position.y; // Bloque la rotation sur l'axe X
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