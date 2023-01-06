using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a card in a memory game. Holds all the different possible sprites of cards in graphemesSprites and imagesSprites.
public class Card : MonoBehaviour
{
    // Reference to a <see cref="MemoryControl"/> object in the scene
    GameObject memoryControl;
    // Sprite that is currently shown on the board for this card (either back or front sprite)
    public Sprite sprite;
    // Holds all the possible graphemes front sprites for cards
    public Sprite[] graphemesSprites;
    // Holds all the possible images or phonemes front sprites for cards depending on the set of cards we are currently playing with
    public Sprite[] imagesSprites;
    // Back sprite of the card
    public Sprite back;
    // Index of the sprite in the imagesSprites table that is the front of this card
    public int elementIndex;
    // Indicates whether the front sprite of the card is an image/phoneme or a grapheme
    public bool isImage;
    // Indicates whether the card has already been correctly matched or not
    public bool matched = false;

    private void Awake()
    {
        memoryControl = GameObject.Find("MemoryControl");
        sprite = GetComponent<Image>().sprite;
    }

    /// <summary>
    /// Used to automatically flip the card to its back side when it has been incorrectly matched with another
    public void TurnOverBack(){
        sprite = back;
        GetComponent<Image>().sprite = sprite;
        this.GetComponent<CardSpinAnimation>().Animate();        
    }
    
    /// <summary>
    // Used to handle the behaviour on the card when the user touches it
    public void click()
    {
        // We want to flip a card only if it has not been correctly matched yet, if two cards have not been flipped up yet, and if there is not an animation currently happening
        if (matched == false && memoryControl.GetComponent<MemoryControl>().secondCardUp == null && !memoryControl.GetComponent<MemoryControl>().animationOn1 && !memoryControl.GetComponent<MemoryControl>().animationOn2)
        {
            // We want a user to be able to flip a card only if it is currently with its backside facing up
            if (sprite == back)
            {
                matched = true;
                this.GetComponent<CardSpinAnimation>().Animate();
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
                    // If there is none, firstCardUp is empty and we set firstCardUp to this card
                    memoryControl.GetComponent<MemoryControl>().firstCardUp = this;
                }
                else
                {   
                    // If there is, firstCardUp already holds a relevant value and we set secondCardUp to this card
                    memoryControl.GetComponent<MemoryControl>().secondCardUp = this;
                    memoryControl.GetComponent<MemoryControl>().TwoCardsUp();
                }
            }
        }
    }
    
    public Vector2 startScale;
    public bool bouncing;
    /// <summary>
    /// Perform a bouncing animation.
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
