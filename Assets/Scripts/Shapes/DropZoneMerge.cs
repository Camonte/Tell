using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Merges two Phonemes together. Always attached to a <see cref="Draggable"/> Phoneme or a Phoneme Generator.
/// Previews the merged Phoneme when hovered.
/// Generates the merged Phoneme on drop or on long hover events.
/// </summary>
public class DropZoneMerge : DropZone
{
    public Draggable currentHover;
    protected enum State { None, Up, Down, Left, Right }
    protected State state = State.None;
    
    public Phoneme phoneme;
    public Phoneme oldPhoneme;
    public Draggable _draggable;

    protected bool isGenerator;

    private void Awake()
    {
        priority = 1;
    }

    private void Start()
    {
        _draggable = GetComponent<Draggable>();
        if (_draggable != null)
        {
            phoneme = (Phoneme)_draggable.element;
        }
        oldPhoneme = phoneme;
        isGenerator = _draggable == null;
    }

    public override bool CanDrop(Draggable draggable) => enabled && CanHover(draggable);

    public override bool CanLongHover(Draggable draggable) => enabled && oldPhoneme != phoneme;

    public override bool CanHover(Draggable draggable) => enabled && Phoneme.CanMerge(oldPhoneme, draggable.element) || Phoneme.CanMerge(draggable.element, oldPhoneme);

    public override void OnDrop(Draggable draggable)
    {
        if (oldPhoneme != phoneme)
        {
            oldPhoneme = phoneme;
            draggable.Destroy();
            _draggable.Bounce();
        }
    }

    public override void LongHover(Draggable draggable)
    {
        if (oldPhoneme != phoneme)
        {
            ShapeManager.Instance.UpdatePhoneme(draggable.gameObject, phoneme);
            draggable.GetComponent<DropZoneMerge>().oldPhoneme = phoneme;

            if (isGenerator)
            {
                // reset generator to old phoneme
                ShapeManager.Instance.UpdatePhoneme(gameObject, oldPhoneme);
                phoneme = oldPhoneme;
            }
            
            _draggable?.Destroy();

            draggable.Bounce();

            if (Config.testMode && StateManager.Instance.currentSentence != null)
            {
                if (Config.progressiveCorrection & !StateManager.Instance.TapOk((Phoneme)draggable.element))
                {
                    StateManager.Instance.OnWrong();
                }
                else
                {
                    StateManager.Instance.DisplayAnswer((Phoneme)draggable.element);
                    draggable.Destroy();
                }
            }
        }
    }

    

    public override void Hover(Draggable draggable)
    {
        if (draggable != currentHover) return;

        var thisIsHigher = transform.position.y > draggable.transform.position.y;
        var isCloseEnough = Vector2.Distance(transform.position, draggable.transform.position) < ScaleManager.Instance.GetScale() * 0.7f;
        var newState = isCloseEnough ? (thisIsHigher ? State.Down : State.Up) : State.None;

        if (state != newState)
        {
            draggable.OnChangeHover();
            state = newState;
            var ph = state switch
            {
                State.Down => Phoneme.Merge(oldPhoneme, draggable.element),
                State.Up => Phoneme.Merge(draggable.element, oldPhoneme),
                _ => null,
            };

            var changed = phoneme != ph;

            if (ph == null)
            {
                ph = oldPhoneme;
                draggable.Show();
            }
            else
            {
                draggable.Hide();
            }

            if (changed)
            {
                ShapeManager.Instance.UpdatePhoneme(gameObject, ph);
                phoneme = ph;

                _draggable?.Bounce();
                draggable.Bounce();
            }
        }
    }

    public override void HoverEnter(Draggable draggable)
    {
        if (currentHover == null)
        {
            currentHover = draggable;
            Hover(draggable);
        }
    }

    public override void HoverExit(Draggable draggable)
    {
        if (currentHover == draggable)
        {
            draggable.Show();
            currentHover = null;
            state = State.None;

            if (oldPhoneme != phoneme)
            {
                ShapeManager.Instance.UpdatePhoneme(gameObject, oldPhoneme);
                phoneme = oldPhoneme;
                _draggable?.Bounce();
                draggable.Bounce();
            }
        }
    }
}
