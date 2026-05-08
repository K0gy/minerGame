using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Image fadeImage;
    public GameObject title;
    public GameObject menuPanel;
    public GameObject introPanel;
    public TextMeshProUGUI introText;
    public Button playButton; // <-- Assigner le bouton Play ici

    [Header("Settings")]
    public float fadeDuration = 2f;
    public string gameSceneName = "SampleScene";

    void Start()
    {
        // ⚡ Sécuriser le blocage d'UI
        if (introPanel != null)
        {
            introPanel.SetActive(false);
            CanvasGroup cg = introPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
                cg.alpha = 0f;
            }
        }

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            CanvasGroup cgMenu = menuPanel.GetComponent<CanvasGroup>();
            if (cgMenu != null)
            {
                cgMenu.blocksRaycasts = true;
                cgMenu.interactable = true;
                cgMenu.alpha = 1f;
            }
        }

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false; // ne bloque pas les clics
        }

        // ⚡ Assurer que PlayButton est actif et cliquable
        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
            playButton.interactable = true;
        }

        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return FadeIn();
        yield return new WaitForSeconds(2f);

        yield return FadeOut();
        title.SetActive(false);

        // Intro
        introPanel.SetActive(true);
        CanvasGroup cgIntro = introPanel.GetComponent<CanvasGroup>();
        if (cgIntro != null) cgIntro.blocksRaycasts = true;
        yield return FadeIn();
        yield return PlayIntroText();
        yield return FadeOut();
        introPanel.SetActive(false);
        if (cgIntro != null)
        {
            cgIntro.blocksRaycasts = false;
            cgIntro.interactable = false;
            cgIntro.alpha = 0f;
        }

        // Menu
        menuPanel.SetActive(true);
        CanvasGroup cgMenuFinal = menuPanel.GetComponent<CanvasGroup>();
        if (cgMenuFinal != null)
        {
            cgMenuFinal.blocksRaycasts = true;
            cgMenuFinal.interactable = true;
            cgMenuFinal.alpha = 1f;
        }

        // ⚡ S'assurer que le bouton Play est au premier plan
        if (playButton != null)
        {
            playButton.transform.SetAsLastSibling();
            playButton.interactable = true;
        }

        yield return FadeIn();
    }

    public void PlayGame()
    {
        Debug.Log("BOUTON CLIQUÉ");
        StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeOutAndLoad()
    {
        yield return FadeOut();
        SceneManager.LoadScene(gameSceneName);
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        Color c = fadeImage.color;
        for (float t = 1f; t > 0f; t -= Time.deltaTime / fadeDuration)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
    }

    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        Color c = fadeImage.color;
        for (float t = 0f; t < 1f; t += Time.deltaTime / fadeDuration)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
    }

    IEnumerator PlayIntroText()
    {
        if (introText == null) yield break;

        introText.text = "";
        yield return ShowLine("République Démocratique du Congo.", 2f);
        yield return ShowLine("\n\nIci, le cobalt vaut plus que des vies.", 3f);
        yield return ShowLine("\n\nChaque jour, des jeunes descendent dans des mines instables,", 2f);
        yield return ShowLine("\n\nOù les effondrements, les gaz toxiques et les explosions\npeuvent survenir à tout moment.", 4f);
        yield return ShowLine("\n\nAujourd’hui, c’est toi.", 3f);
        yield return ShowLine("\n\nDescends.", 2f);
        yield return ShowLine("\n\nMine.", 2f);
        yield return ShowLine("\n\nÉvite les dangers.", 2f);
        yield return ShowLine("\n\nSurvis.", 5f);
    }

    IEnumerator ShowLine(string text, float delay)
    {
        introText.text += text;
        yield return new WaitForSeconds(delay);
    }
}