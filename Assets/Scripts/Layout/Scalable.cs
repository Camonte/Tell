using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lets the gameobject subscribe to events and link any behaviour to each of the events.
/// The linked behaviours are called automatically by the <see cref="ScaleManager"/>.
/// </summary>
public class Scalable : MonoBehaviour
{
    [SerializeField] UnityEvent<float> onUpdateScale;
    [SerializeField] UnityEvent onUpdateLayout;
    [SerializeField] UnityEvent<int> onUpdateFontSize;
    public int priority;
    
    private void Start()
    {
        ScaleManager.Instance.Subscribe(this);    
    }

    public virtual void OnUpdateScale(float scale)
    {
        onUpdateScale?.Invoke(scale);
    }

    public virtual void OnUpdateLayout()
    {
        onUpdateLayout?.Invoke();
    }

    internal void OnUpdateFontSize(int fontSize)
    {
        onUpdateFontSize?.Invoke(fontSize);
    }
}
