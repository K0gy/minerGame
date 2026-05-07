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

    [Header("Settings")]
    public float fadeDuration = 2f;
    public string gameSceneName = "SampleScene";

    void Start()
    {
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1. Fade IN (show title)
        yield return FadeIn();
        yield return new WaitForSeconds(2f);

        // 2. Fade OUT (to black)
        yield return FadeOut();

        // 3. Hide title
        title.SetActive(false);

        // 4. Show intro panel
        introPanel.SetActive(true);

        // 5. Fade IN intro
        yield return FadeIn();

        // 6. Play intro text
        yield return PlayIntroText();

        // 7. Fade OUT intro
        yield return FadeOut();

        introPanel.SetActive(false);

        // 8. Show main menu
        menuPanel.SetActive(true);

        // 9. Final fade IN
        yield return FadeIn();
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

    
    public void PlayGame()
    {
    Debug.Log("BOUTON CLIQUÉ");
    }

    IEnumerator FadeOutAndLoad()
    {
        yield return FadeOut();
        SceneManager.LoadScene(gameSceneName);
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