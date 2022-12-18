using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    GameObject memoryControl;
    public Sprite sprite;
    public Sprite[] graphemesSprites;
    public Sprite[] imagesSprites;
    public Sprite back;
    public int elementIndex;
    public bool isImage;
    public bool matched = false;

    private void Awake()
    {
        memoryControl = GameObject.Find("MemoryControl");
        sprite = GetComponent<Image>().sprite;
    }

    public void TurnOverBack(){
        sprite = back;
        GetComponent<Image>().sprite = sprite;
    }
    
    public void click()
    {
        if (matched == false && memoryControl.GetComponent<MemoryControl>().secondCardUp == null)
        {
            if (sprite == back)
            {
                matched = true;
                // Flip the card
                if(isImage)
                {
                    sprite = imagesSprites[elementIndex];
                }
                else
                    sprite = graphemesSprites[elementIndex];
                GetComponent<Image>().sprite = sprite;
                // Check whether there is already a flipped card on the board
                if (memoryControl.GetComponent<MemoryControl>().firstCardUp == null)
                {
                    memoryControl.GetComponent<MemoryControl>().firstCardUp = this;
                }
                else
                {
                    memoryControl.GetComponent<MemoryControl>().secondCardUp = this;
                    memoryControl.GetComponent<MemoryControl>().TwoCardsUp();
                }
            }
        }
    }

    public Vector2 startScale;
    public bool bouncing;
    /// <summary>
    /// Perform a bouncing animation. TODO: change this to use iTween.
    /// </summary>
    public void Bounce()
    {
        // not running
        if (!bouncing)
        {
            startScale = transform.localScale;
        }
        // running
        else
        {
            StopCoroutine("Bounce");
            bouncing = false;
            transform.localScale = startScale;
        }
        StartCoroutine(Bounce(0.2f, 1, 0.2f));
    }

    public IEnumerator Bounce(float amount, int repetitions, float duration)
    {
        bouncing = true;
        float elapsedTime = 0;
        Vector2 startScale = transform.localScale;
        while (elapsedTime < duration && bouncing)
        {
            float scaledTime = Mathf.Repeat(elapsedTime * repetitions, duration);
            float delta = Mathf.Lerp(-amount, amount, scaledTime / duration);
            delta = 1 + (amount - Mathf.Abs(delta));
            elapsedTime += Time.deltaTime;
            transform.localScale = startScale * delta;
            yield return null;
        }
        if (bouncing) // stopped externally, probably the local scale was changed by something else so don't reset it
            transform.localScale = startScale;
        bouncing = false;
    }
}
