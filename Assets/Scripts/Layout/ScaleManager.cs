using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the scaling of the UI, shapes and text.
/// To get notified of a change, a gameobject has to have the <see cref="Scalable"/> script attached.
/// When a property changes (scale, fontsize, layout) all subscribed gameobjects are notified.
/// Scale and fontsize are linked to two UI sliders, and layout change is mostly called from other scripts (i.e. <see cref="SidePanel"/>).
/// It also lets the user activate/deactivate the colliders for scrolling the center pannel and moving the side pannels (terrible design decision...)
/// </summary>
public class ScaleManager : Singleton<ScaleManager>
{
    public bool block = true;

    public List<Collider2D> ignoreColliders = new List<Collider2D>();

    [Range(minScale, maxScale)]
    public float scale = Config.defaultScale;
    public const float minScale = 1f;
    public const float maxScale = 2f;

    [Range(minFontSize, maxFontSize)]
    public int fontSize = Config.defaultFontSize;
    public const int minFontSize = 8;
    public const int maxFontSize = 24;

    [SerializeField] Slider scaleSlider;
    [SerializeField] Slider fontSizeSlider;

    private List<Scalable> scalables = new List<Scalable>();

    private bool initialUpdate;

    public override void Awake()
    {
        scale = Config.defaultScale;
        fontSize = Config.defaultFontSize;

        if (scaleSlider != null)
        {
            scaleSlider.minValue = minScale;
            scaleSlider.maxValue = maxScale;
            scaleSlider.value = scale;
            scaleSlider.onValueChanged.AddListener(SetScale);
        }
        
        if (fontSizeSlider != null)
        {
            fontSizeSlider.minValue = minFontSize;
            fontSizeSlider.maxValue = maxFontSize;
            fontSizeSlider.value = fontSize;
            fontSizeSlider.onValueChanged.AddListener(SetFontSize);
        }
    }

    public void OnBlockScaleChanged(bool b)
    {
        block = b;
        foreach (var c in ignoreColliders)
            c.enabled = !block;
    }

    public void SetScale(float scale)
    {
        var old = this.scale;
        this.scale = scale;
        if (old != this.scale)
            OnUpdateScale();
    }

    public void SetFontSize(float size)
    {
        var old = this.fontSize;
        this.fontSize = Mathf.RoundToInt(size);
        if (old != this.fontSize)
            OnUpdateFontSize();
    }

    public float GetScale()
    {
        return scale;
    }

    public Vector2 GetScaleVector()
    {
        return new Vector2(scale, scale);
    }

    public void Subscribe(Scalable s)
    {
        if (!scalables.Contains(s))
        {
            scalables.Add(s);
            scalables = scalables.OrderByDescending(s => s.priority).ToList();
        }
    }

    public void Unsubscribe(Scalable s)
    {
        if (scalables.Contains(s))
        {
            scalables.Remove(s);
        }
    }

    public void OnUpdateFontSize()
    {
        foreach (var s in scalables)
        {
            s.OnUpdateFontSize(fontSize);
        }
    }

    public void OnUpdateScale()
    {
        foreach(var s in scalables)
        {
            s.OnUpdateScale(scale);
        }
    }

    public void OnUpdateLayout()
    {
        foreach(var s in scalables)
        {
            s.OnUpdateLayout();
        }
    }

    private void Update()
    {
        if (!initialUpdate)
        {
            OnUpdateScale();

            OnUpdateLayout();

            OnUpdateFontSize();

            OnBlockScaleChanged(block);

            initialUpdate = true;
        }
    }
}