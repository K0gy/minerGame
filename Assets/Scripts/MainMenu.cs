using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public Image fadeImage;
    public GameObject title;
    public GameObject menuPanel;

    public float fadeDuration = 2f;
    public float titleDuration = 2f;

    public string gameSceneName = "Game";

    void Start()
    {
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // Fade in (noir → visible)
        yield return StartCoroutine(FadeIn());

        // Attendre avec le titre
        yield return new WaitForSeconds(titleDuration);

        // Cacher titre
        title.SetActive(false);

        // Activer menu
        menuPanel.SetActive(true);
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

    public void PlayGame()
    {
        StartCoroutine(FadeOut());
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

        SceneManager.LoadScene(gameSceneName);
    }
}