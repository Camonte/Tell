using UnityEngine;
using Lean.Touch;

/// <summary>
/// Script attached to a Phoneme Generator, that spawns a new Phoneme of the same type when touched.
/// See also <see cref="DropZoneMerge"/>.
/// </summary>
public class Spawn : DropZoneMerge
{
    private LeanSelectByFinger touchManager;
    private Tutorial tutorial;

    private void Awake()
    {
        priority = 2;
        touchManager = FindObjectOfType<LeanSelectByFinger>();
        GetComponent<LeanFingerTap>().OnFinger.AddListener(OnTap);
        tutorial = FindObjectOfType<Tutorial>();
    }

    public void SpawnObject(LeanFinger finger)
    {
        var obj = ShapeManager.Instance.CreatePhoneme(phoneme, transform.position);
        var selectable = obj.GetComponent<LeanSelectableByFinger>();

        touchManager.Select(selectable, finger);
    }

    private void OnTap(LeanFinger finger)
    {
        SoundManager.Instance.Play(phoneme.id);
        tutorial?.Check(phoneme);
        if(Config.testMode && StateManager.Instance.currentSentence != null && GridManager.Instance.FirstNullIndex() != -1)
        {
            if(Config.progressiveCorrection & !StateManager.Instance.TapOk(phoneme))
            {
                StateManager.Instance.OnWrong();
            } else {
                StateManager.Instance.DisplayAnswer((Phoneme)phoneme);
            }
        }
    }

    // no drop on generators
    public override bool CanDrop(Draggable draggable) => false;

    public override void OnDrop(Draggable draggable) { }
}
