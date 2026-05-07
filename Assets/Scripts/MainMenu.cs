using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public Image fadeImage;
    public GameObject title;
    public GameObject menuPanel;
    public GameObject introPanel;
    public TextMeshProUGUI introText;
    public float fadeDuration = 2f;
    public string gameSceneName = "SampleScene";

    void Start()
    {
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
    // 1. Fade vers titre
    yield return StartCoroutine(FadeIn());
    yield return new WaitForSeconds(2f);

    // 2. Fade vers noir
    yield return StartCoroutine(FadeOut());

    // 3. Cacher titre
    title.SetActive(false);

    // 4. ACTIVER INTRO
    introPanel.SetActive(true);

    // 5. Fade IN intro
    yield return StartCoroutine(FadeIn());

    // 6. TEXTE PROGRESSIF
    yield return StartCoroutine(PlayIntroText());

    // 7. Fade OUT intro
    yield return StartCoroutine(FadeOut());

    introPanel.SetActive(false);

    // 8. AFFICHER MENU
    menuPanel.SetActive(true);

    // 9. Fade final
    yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Color c = fadeImage.color;

        for (float t = 1f; t > 0f; t -= Time.deltaTime / fadeDuration)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        fadeImage.color = c;
    }

    IEnumerator FadeOut()
    {
        Color c = fadeImage.color;

        for (float t = 0f; t < 1f; t += Time.deltaTime / fadeDuration)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
    }

    public void PlayGame()
    {
        StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeOutAndLoad()
    {
        Color c = fadeImage.color;

        for (float t = 0f; t < 1f; t += Time.deltaTime / fadeDuration)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(gameSceneName);
    }
    IEnumerator PlayIntroText()
    {
    introText.text = "";

    introText.text = "République Démocratique du Congo.";
    yield return new WaitForSeconds(2.0f);

    introText.text += "\n\nIci, le cobalt vaut plus que des vies.";
    yield return new WaitForSeconds(3.0f);

    introText.text += "\n\nChaque jour, des jeunes descendent dans des mines instables,";
    yield return new WaitForSeconds(2.0f);

    introText.text += "\n\nOù les effondrements, les gaz toxiques et les explosions\npeuvent survenir à tout moment.";
    yield return new WaitForSeconds(4.0f);

    introText.text += "\n\nAujourd’hui, c’est toi.";
    yield return new WaitForSeconds(3.0f);

    introText.text += "\n\nDescends.";
    yield return new WaitForSeconds(2.0f);

    introText.text += "\n\nMine.";
    yield return new WaitForSeconds(2.0f);

    introText.text += "\n\nÉvite les dangers.";
    yield return new WaitForSeconds(2.0f);

    introText.text += "\n\nSurvis.";
    yield return new WaitForSeconds(5.0f);
    }
    
}