using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Code for the two Tutorial modes.
/// </summary>
public class Tutorial : MonoBehaviour
{
    private const string audioInfoText = "Appuie sur le phonème que tu viens d'entendre !\n\nTu peux réécouter le son en appuyant sur le bouton lecture.";
    private const string visualInfoText = "Prononce le son qui correspond au phonème proposé !\n\nTu peux passer au suivant en appuyant sur la flèche.";

    Phoneme currentPhoneme;
    int currentPhonemeIndex;
    List<Phoneme> potentialPhonemes = new List<Phoneme>();
    System.Random rng = new System.Random();

    [SerializeField] Button playButton;
    [SerializeField] TMPro.TextMeshPro infoText;
    [Header("Sprites")]
    [SerializeField] Sprite playSprite;
    [SerializeField] Sprite nextSprite;
    [SerializeField] Sprite correctSprite;
    [SerializeField] Sprite wrongSprite;
    [SerializeField] Sprite soundSprite;

    private void Start()
    {
        var isAudio = Config.tutorialType == Config.TutorialType.Audio;
        if (isAudio)
        {
            playButton.onClick.AddListener(Play);
            Destroy(playButton.gameObject.GetComponent<ShiftXAnimation>());
            if (infoText != null) infoText.text = audioInfoText;
        }
        else
        {
            playButton.onClick.AddListener(Highlight);
            playButton.GetComponent<Image>().sprite = nextSprite;
            playButton.transform.localScale *= -1;
            if (infoText != null) infoText.text = visualInfoText;

        }
        OnFilterChange();
    }

    public void OnFilterChange()
    {
        potentialPhonemes = Phoneme.All(Config.filter);
        var empty = potentialPhonemes.Count == 0;
        playButton.gameObject.SetActive(!empty);
        if (empty) return;

        ResetHighlight();

        // Shuffle 
        potentialPhonemes = potentialPhonemes.OrderBy(p => rng.Next()).ToList();
        currentPhonemeIndex = 0;
        currentPhoneme = potentialPhonemes[0];
        //currentPhoneme = GetNextPhoneme();
    }

    /// <summary>
    /// Is the selected Phoneme the correct answer ? Am I sick and tired of writing the documentation ?
    /// </summary>
    public bool Check(Phoneme answer)
    {
        var correct = answer == currentPhoneme;
        switch (Config.tutorialType)
        {
            case Config.TutorialType.Audio:
                var waitTime = correct ? 1.2f : 0.3f;
                StartCoroutine(NextSoundCoroutine(correct, waitTime));
                break;
            case Config.TutorialType.Visual:
                break;
        }
        return correct;
    }

    private void ResetHighlight()
    {
        if (currentPhoneme == null) return;

        iTween.StopByName("highlight");
        SidePanel.GeneratorFor(currentPhoneme.id).transform.localScale = Vector3.one;
        foreach (var g in SidePanel.Generators())
        {
            ShapeManager.Instance.SetOpacity(g, 1);
            g.GetComponent<SortingGroup>().sortingOrder = 0;
        }
    }

    private void Highlight()
    {
        ResetHighlight();

        currentPhoneme = GetNextPhoneme();
        foreach (var g in SidePanel.Generators(currentPhoneme.id))
        {
            ShapeManager.Instance.SetOpacity(g, 0.3f);
            g.GetComponent<SortingGroup>().sortingOrder = -1;
        }
        iTween.PunchScale(SidePanel.GeneratorFor(currentPhoneme.id), iTween.Hash("name", "highlight", "amount", Vector3.one * 0.5f, "time", 1f, "looptype", "loop"));
    }

    private void Next()
    {
        currentPhoneme = GetNextPhoneme();
        Play();
    }

    private void Play()
    {
        StartCoroutine(PlayCoroutine(SoundManager.Instance.Play(currentPhoneme.id)));
    }

    private IEnumerator PlayCoroutine(float waitTime)
    {
        playButton.interactable = false;
        playButton.image.sprite = soundSprite;
        
        yield return new WaitForSeconds(waitTime);
        
        playButton.image.sprite = playSprite;
        playButton.interactable = true;
    }

    private IEnumerator NextSoundCoroutine(bool correct, float waitTime)
    {
        playButton.interactable = false;
        if (correct)
        {
            playButton.image.sprite = correctSprite;
            SoundManager.Instance.Success();
        } else
        {
            playButton.image.sprite = wrongSprite;
        }
        
        yield return new WaitForSeconds(waitTime);
        
        playButton.image.sprite = playSprite;
        playButton.interactable = true;
        if (correct)
            Next();
    }

    private Phoneme GetNextPhoneme()
    {
        int index = ++currentPhonemeIndex % potentialPhonemes.Count;
        Debug.Log(potentialPhonemes[index]);

        return potentialPhonemes[index];
    }
}
