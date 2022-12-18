using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Memory Menu logic for buttons, toggles etc.
/// Also hosts the logic for moving between the Main Menu and Game scenes.
/// </summary>
public class MemoryMenu : MonoBehaviour
{
    
    [SerializeField] ToggleSpriteSwap vowelsToggle;
    [SerializeField] ToggleSpriteSwap consonantsToggle;
    [SerializeField] Button launchImages;
    [SerializeField] Button launchPhonemes;

    [Space(5)]
    [SerializeField] UnityEvent onActiveChange;

    [DllImport("__Internal")]
    private static extern bool Open();

    void OnEnable()
    {
        UpdateUI();
    }

    private void Start()
    {
        // Consonants & Vowel Toggles
        if (vowelsToggle != null)
        { vowelsToggle.onValueChanged.AddListener(ToggleVowels); vowelsToggle.Initialize(Config.vowelsActive); }
        if (consonantsToggle != null)
        { consonantsToggle.onValueChanged.AddListener(ToggleConsonnants); consonantsToggle.Initialize(Config.consonantsActive); }

        // Launch Images deck Button
        if (launchImages != null)
        { launchImages.onClick.AddListener(LaunchImages); }

        // Launch Phonemes deck Button
        if (launchPhonemes != null)
        { launchPhonemes.onClick.AddListener(LaunchPhonemes); }
    }

    public void UpdateUI()
    {
        if (vowelsToggle != null) vowelsToggle.Initialize(Config.vowelsActive);
        if (consonantsToggle != null) consonantsToggle.Initialize(Config.consonantsActive);
    }

    void ToggleConsonnants(bool b)
    {
        var change = Config.SetConsonantsActive(b);
        if (change)
        {
            UpdateUI();
        }
        onActiveChange?.Invoke();
    }

    void ToggleVowels(bool b)
    {
        var change = Config.SetVowelsActive(b);
        if (change)
        {
            UpdateUI();
        }
        onActiveChange?.Invoke();
    }
    void LaunchImages()
    {
        Config.testMode = false;
        Config.freeWriting = false;
        Config.memoryMode = false;
        Config.progressiveCorrection = true;
        StartCoroutine(Launch("Main Menu 1"));
    }

    void LaunchPhonemes()
    {
        Config.testMode = false;
        Config.freeWriting = false;
        Config.memoryMode = true;
        Config.progressiveCorrection = true;
        StartCoroutine(Launch("Main Menu2"));
    }

    private IEnumerator Launch(string sceneName)
    {
        var circle = GameObject.Find("Circle");
        circle.GetComponent<Animator>().SetTrigger("Start");
        yield return new WaitForSeconds(1);
        iTween.Stop();
        SceneManager.LoadScene(sceneName);
    }

    public void BackToMainMenu()
    {
        StartCoroutine(Launch("Main Menu"));
    }

    public void ReloadLevel()
    {
        StartCoroutine(Launch("Main Menu 1"));
    }

    public void NextLevel()
    {
        if(Config.memoryDifficulty == 1)
        {
            Config.memoryDifficulty = 2;
        } 
        else if (Config.memoryDifficulty == 2)
        {
            Config.memoryDifficulty = 3;
        }
        StartCoroutine(Launch("Main Menu 1"));
    }
}
