using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main Menu logic for buttons, toggles, switching between different pannels etc.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject playPanel;
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] GameObject aboutPanel;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject memoryPanel;

    private GameObject currentPanel;

    // menu state
    // 0 = main
    // 1 = play
    // 2 = tutorial
    // 3 = about
    // 4 = memory
    public static int state = 0;

    private void Start()
    {
        // if comming back from a game/tutorial, load previous submenu
        if (state != 0)
        {
            if (state == 1 && playPanel != null) ShowPlayPannel();
            else if (state == 2 && tutorialPanel != null) ShowTutorialPannel();
            else if (state == 3 && aboutPanel != null) ShowAboutPannel();
            else if (state == 4 && memoryPanel != null) ShowMemoryPanel();
        }
    }

    public void ShowTutorialPannel()
    {
        Config.testMode = false;
        Config.freeWriting = false;
        Config.progressiveCorrection = true;
        state = 2;
        currentPanel = tutorialPanel;
        ShowPannel();
    }
    public void ShowAboutPannel()
    {
        Config.testMode = false;
        Config.freeWriting = false;
        Config.progressiveCorrection = true;
        state = 3;
        currentPanel = aboutPanel;
        ShowPannel();
    }
    public void ShowPlayPannel()
    {
        state = 1;
        currentPanel = playPanel;
        ShowPannel();
    }
    
    public void ShowFreeWritingPanel()
    {
        Config.testMode = false;
        Config.freeWriting = true;
        Config.progressiveCorrection = true;
        StartCoroutine(LaunchFreeWriting());
    }
    public void ShowMemoryPanel()
    {
        Config.testMode = false;
        Config.freeWriting = false;
        Config.progressiveCorrection = false;
        state = 4;
        currentPanel = memoryPanel;
        ShowPannel();
    }
    private void ShowPannel()
    {
        StartCoroutine(Fade(backButton, false));
        StartCoroutine(Fade(menuPanel, true));
        StartCoroutine(Fade(currentPanel, false));
    }

    public void Back()
    {
        state = 0;
        StartCoroutine(Fade(backButton, true));
        StartCoroutine(Fade(menuPanel, false));
        StartCoroutine(Fade(currentPanel, true));
        currentPanel = null;
    }

    private IEnumerator Fade(GameObject panel, bool hide)
    {
        if (!hide) panel.SetActive(true);
        var duration = 0.5f;
        var elapsed = 0f;
        var group = panel.GetComponent<CanvasGroup>();
        while (elapsed < duration)
        {
            var alpha = Mathf.Lerp(0, 1, elapsed / duration);
            group.alpha = hide ? (1 - alpha) : alpha;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        group.alpha = 1;
        if (hide) panel.SetActive(false);
    }

    private IEnumerator LaunchFreeWriting()
    {
        var circle = GameObject.Find("Circle");
        circle.GetComponent<Animator>().SetTrigger("Start");
        yield return new WaitForSeconds(1);
        iTween.Stop();
        SceneManager.LoadScene("Free Writing");
    }
}
