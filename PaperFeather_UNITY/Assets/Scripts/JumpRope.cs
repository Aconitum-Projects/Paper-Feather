using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Playables;


public class JumpRope : MonoBehaviour
{
    [Header("Rotation Settings")]
    public AnimationCurve speedCurve;
    public float baseRotationSpeed = 360f;
    public string playerTag = "Player";

    [Header("UI - Tours")]
    public TextMeshProUGUI turnsText;

    [Header("UI - Vies")]
    public Image[] lifeImages;
    public string featherCenterName = "FeatherCenter";

    [Header("Gameplay")]
    public int maxLives = 3;
    public float invulnerabilityTime = 1f;
    public Material flashMat;
    
    [Header("Timeline de Game Over")]
    public PlayableDirector gameOverTimeline;

    private float currentRotationSpeed;
    private float rotationProgress = 0f;
    private int completedTurns = 0;
    private int currentLives;
    private bool isInvulnerable = false;
    private bool isGameOver = false;

    void Start()
    {
        currentRotationSpeed = baseRotationSpeed;
        currentLives = maxLives;
        UpdateTurnsUI();
    }

    void Update()
    {
        if (isGameOver) return;
        
        rotationProgress += (Time.deltaTime * currentRotationSpeed) / 360f;

        if (rotationProgress >= 1f)
        {
            rotationProgress -= 1f;
            completedTurns++;
            UpdateTurnsUI();

            if (completedTurns % 5 == 0)
            {
                currentRotationSpeed *= 1.05f;
            }
        }

        float curveMultiplier = speedCurve.Evaluate(rotationProgress);
        float rotationStep = currentRotationSpeed * curveMultiplier * Time.deltaTime;
        transform.Rotate(Vector3.left, rotationStep);
    }

    void UpdateTurnsUI()
    {
        if (turnsText != null)
            turnsText.text = completedTurns.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isInvulnerable)
        {
            LoseLife();
        }
    }

    void LoseLife()
    {
        currentLives--;
        Debug.Log("Vie perdue ! Restantes : " + currentLives);
        UpdateLivesUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(InvulnerabilityCooldown());
        }
    }

    IEnumerator InvulnerabilityCooldown()
    {
        isInvulnerable = true;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
            Material[] defaultMats = new Material[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
                defaultMats[i] = renderers[i].material;

            float elapsed = 0f;
            float blinkInterval = 0.1f;

            while (elapsed < invulnerabilityTime)
            {
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].material = flashMat;

                yield return new WaitForSeconds(blinkInterval);

                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].material = defaultMats[i];

                yield return new WaitForSeconds(blinkInterval);

                elapsed += blinkInterval * 2;
            }

            for (int i = 0; i < renderers.Length; i++)
                renderers[i].material = defaultMats[i];
        }

        isInvulnerable = false;
    }

    void UpdateLivesUI()
    {
        int lifeIndex = Mathf.Clamp(currentLives, 0, lifeImages.Length - 1);

        if (lifeIndex >= 0 && lifeIndex < lifeImages.Length)
        {
            Image parentImg = lifeImages[lifeIndex];

            Transform featherCenter = parentImg.transform.Find(featherCenterName);
            if (featherCenter != null)
            {
                Image featherImg = featherCenter.GetComponent<Image>();
                if (featherImg != null)
                {
                    StartCoroutine(FadeOutFeather(featherImg, 0f, 0.3f));
                }
            }
            
            StartCoroutine(ShrinkImage(parentImg.transform, 0.6f));
        }
    }

    IEnumerator ShrinkImage(Transform target, float duration)
    {
        Vector3 startScale = target.localScale;
        Vector3 endScale = Vector3.zero;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(startScale, endScale, t / duration);
            yield return null;
        }
    }

    IEnumerator FadeOutFeather(Image img, float targetAlpha, float duration)
    {
        Color startColor = img.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            img.color = Color.Lerp(startColor, endColor, t / duration);
            yield return null;
        }
    }
    
    void GameOver()
    {
        Debug.Log("Game Over !");
        isGameOver = true;

        PlayerController playerCtrl = FindAnyObjectByType<PlayerController>();
        if (playerCtrl != null)
            playerCtrl.enabled = false;

        StartCoroutine(SmoothStopRope());

        if (gameOverTimeline != null)
        {
            gameOverTimeline.stopped += OnGameOverTimelineStopped;
            gameOverTimeline.Play();
        }
        else
        {
            Debug.LogWarning("Pas de timeline assignée pour le Game Over !");
        }
    }

    IEnumerator SmoothStopRope()
    {
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(180f, 0f, 0f);
        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t / duration);
            yield return null;
        }
        transform.rotation = targetRot;
    }

    void OnGameOverTimelineStopped(UnityEngine.Playables.PlayableDirector director)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}