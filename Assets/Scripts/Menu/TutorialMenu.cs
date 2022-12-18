using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tutorial Menu logic for buttons, toggles, launching the tutorial etc.
/// </summary>
public class TutorialMenu : MonoBehaviour
{
    [SerializeField] ToggleSpriteSwap tutorialModeToggle;
    [SerializeField] ToggleSpriteSwap vowelsToggle;
    [SerializeField] ToggleSpriteSwap consonantsToggle;

    private Tutorial tutorial;

    private void Awake()
    {
        tutorial = FindObjectOfType<Tutorial>();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    void Start()
    {
        // Tutorial Mode Toggle
        if (tutorialModeToggle != null)
        { tutorialModeToggle.onValueChanged.AddListener(ToggleTutorialMode); tutorialModeToggle.Initialize(Config.tutorialType == Config.TutorialType.Audio); }

        // Consonants & Vowel Toggles
        if (vowelsToggle != null)
        { vowelsToggle.onValueChanged.AddListener(ToggleVowels); vowelsToggle.Initialize(Config.vowelsActive); }
        if (consonantsToggle != null)
        { consonantsToggle.onValueChanged.AddListener(ToggleConsonnants); consonantsToggle.Initialize(Config.consonantsActive); }

        ToggleTutorialMode(Config.tutorialType == Config.TutorialType.Audio);
    }

    public void UpdateUI()
    {
        if (vowelsToggle != null) vowelsToggle.Initialize(Config.vowelsActive);
        if (consonantsToggle != null) consonantsToggle.Initialize(Config.consonantsActive);
    }

    void ToggleTutorialMode(bool b)
    {
        // true => audio | false => visual
        Config.tutorialType = b ? Config.TutorialType.Audio : Config.TutorialType.Visual;
    }

    void ToggleConsonnants(bool b)
    {
        var change = Config.SetConsonantsActive(b);
        if (change)
        {
            UpdateUI();
        }
        tutorial?.OnFilterChange();
    }

    void ToggleVowels(bool b)
    {
        var change = Config.SetVowelsActive(b);
        if (change)
        {
            UpdateUI();
        }
        tutorial?.OnFilterChange();
    }

    private IEnumerator Launch(string sceneName)
    {
        var circle = GameObject.Find("Circle");
        circle.GetComponent<Animator>().SetTrigger("Start");
        yield return new WaitForSeconds(1);
        iTween.Stop();
        SceneManager.LoadScene(sceneName);
    }

    public void LaunchTutorial()
    {
        StartCoroutine(Launch("Tutorial"));
    }

    public void BackToMainMenu()
    {
        StartCoroutine(Launch("Main Menu")); //SceneManager.LoadScene("Main Menu");
    }
}
