using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to control a memory game.
public class MemoryControl : MonoBehaviour
{
    // The easy/medium/difficult modes values are constant values that are used depending on the difficulty of the game. They are assigned to variables at when launching the scene

    // Easy mode values
    // Used to choose whether we assign a grapheme or an image/phoneme to the card for this mode
    private List<int> easyGraphemeOrImage = new List<int> {0, 0, 0, 1, 1, 1};
    // Used to track which cards we have assigned or not for this mode
    private List<int> easyDisplayedGraphemes = new List<int> {0, 1, 2};
    private List<int> easyDisplayedImages = new List<int> {0, 1, 2};
    // Number of pairs of cards on the board for this mode
    private const int easyNumberOfPairs = 3;
    // x coordinate of the first card on the board for this mode
    private const int easyStartX = -125;
    // Ìndices on the board where we have to begin a new lign of cards for this mode
    private int[] easySwitchLineIndices = new int[] {0, 2};
    // Medium mode values
    private List<int> mediumGraphemeOrImage = new List<int> {0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1};
    private List<int> mediumDisplayedGraphemes = new List<int> {0, 1, 2, 3, 4, 5};
    private List<int> mediumDisplayedImages = new List<int> {0, 1, 2, 3, 4, 5};
    private const int mediumNumberOfPairs = 6;
    private const int mediumStartX = -375;
    private int[] mediumSwitchLineIndices = new int[] {1, 3, 7};
    // Difficult mode values
    private List<int> difficultGraphemeOrImage = new List<int> {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1};
    private List<int> difficultDisplayedGraphemes = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8};
    private List<int> difficultDisplayedImages = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8};
    private const int difficultNumberOfPairs = 9;
    private const int difficultStartX = -625;
    private int[] difficultSwitchLineIndices = new int[] {2, 5, 11};

    // Images mode values
    // Indices of the images sprites used when both consonnants and vowels are selected
    private List<int> imagesGraphemesIndices = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29};
    // Indices of the images sprites used when only consonnants are selected
    private List<int> imagesConsonantsGraphemesIndices = new List<int> {5, 6, 7, 8, 12, 13, 15, 16, 17, 18, 22, 23, 24, 25, 5, 6, 7, 8, 12, 13, 15, 16, 17, 18, 22, 23, 24, 25};
    // Indices of the images sprites used when only vowels are selected
    private List<int> imagesVowelsGraphemesIndices = new List<int> {0, 1, 2, 3, 4, 9, 10, 11, 14, 19, 20, 21, 26, 0, 1, 2, 3, 4, 9, 10, 11, 14, 19, 20, 21, 26};

    // Phonemes mode values
    private List<int> phonemesGraphemesIndices = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51};
    private List<int> phonemesConsonantsGraphemesIndices = new List<int> {6, 7, 8, 9, 10, 11, 16, 17, 18, 19, 25, 26, 27, 28, 29, 37, 38, 39, 40, 41, 42, 44, 45, 46, 47};
    private List<int> phonemesVowelsGraphemesIndices = new List<int> {0, 1, 2, 3, 4, 5, 12, 13, 14, 15, 20, 21, 22, 23, 24, 30, 31, 32, 33, 34, 35, 36, 43, 48, 49, 50, 51};
    
    GameObject card;
    // Holds all possible graphemes indices
    List<int> graphemesIndices;
    // Indices of the graphemes sprites that have been picked to be displayed on the board
    List<int> pickedGraphemesIndices;
    // Indices of the images/phonemes sprites that have been picked to be displayed on the board
    List<int> pickedImagesIndices;
    // Tracks which graphemes sprites among the picked ones we have placed on the board yet
    List<int> displayedGraphemes;
    // Tracks which images/phonemes sprites among the picked ones we have placed on the board yet
    List<int> displayedImages;
    // Helps to select whether the next displayed sprite will be a grapheme or an image/phoneme 
    List<int> graphemeOrImage;
    int shuffleNum;
    int shuffleNumBis;
    int numberOfPairs;
    int xPosition;
    int yPosition;
    int startX;
    int[] switchLineIndices;
    
    public static System.Random rnd = new System.Random();
    int[] visibleFaces;

    // Holds the cards that are currently with their front face up on the board, assigned by <see cref="Card"/> themselves
    public Card firstCardUp;
    public Card secondCardUp;

    private GameObject circle;

    // Indicates whether animations are playing. Two animations can play at the same time, so we have two flags.
    public bool animationOn1;
    public bool animationOn2;

    private void Awake()
    {
        card = GameObject.Find("Card");
        circle = GameObject.Find("Circle");
    }
    
    void Start()
    {
        animationOn1 = false;
        animationOn2 = false;
        // Set difficulty settings
        if(Config.memoryDifficulty == 1)
        {
            displayedGraphemes = easyDisplayedGraphemes;
            displayedImages = easyDisplayedImages;
            graphemeOrImage = easyGraphemeOrImage;
            numberOfPairs = easyNumberOfPairs;
            startX = easyStartX;
            switchLineIndices = easySwitchLineIndices;
        }
        else if (Config.memoryDifficulty == 2)
        {
            displayedGraphemes = mediumDisplayedGraphemes;
            displayedImages = mediumDisplayedImages;
            graphemeOrImage = mediumGraphemeOrImage;
            numberOfPairs = mediumNumberOfPairs;
            startX = mediumStartX;
            switchLineIndices = mediumSwitchLineIndices;
        }
        else
        {
            displayedGraphemes = difficultDisplayedGraphemes;
            displayedImages = difficultDisplayedImages;
            graphemeOrImage = difficultGraphemeOrImage;
            numberOfPairs = difficultNumberOfPairs;
            startX = difficultStartX;
            switchLineIndices = difficultSwitchLineIndices;
        }
        xPosition = startX;
        firstCardUp = null;
        secondCardUp = null;
        shuffleNum = 0;
        shuffleNumBis = 0;
        visibleFaces = new int[] { -1, -2 };
        pickedGraphemesIndices = new List<int>();
        pickedImagesIndices = new List<int>();
        yPosition = 290;
        if(Config.memoryMode)
        {
            if(Config.consonantsActive & Config.vowelsActive)
                graphemesIndices = phonemesGraphemesIndices;
            else if (Config.consonantsActive)
                graphemesIndices = phonemesConsonantsGraphemesIndices;
            else 
                graphemesIndices = phonemesVowelsGraphemesIndices;
        }
        else 
        {
            if(Config.consonantsActive & Config.vowelsActive)
                graphemesIndices = imagesGraphemesIndices;
            else if (Config.consonantsActive)
                graphemesIndices = imagesConsonantsGraphemesIndices;
            else 
                graphemesIndices = imagesVowelsGraphemesIndices;
        }
        // Pick the sprites of the pairs that are gonna be used
        for (int i = 0 ; i < numberOfPairs ; ++i)
        {
            shuffleNum = rnd.Next(0, (graphemesIndices.Count));
            pickedGraphemesIndices.Add(graphemesIndices[shuffleNum]);
            pickedImagesIndices.Add(
                System.Array.FindIndex(card.GetComponent<Card>().imagesSprites, s => s.name.StartsWith(
                card.GetComponent<Card>().graphemesSprites[graphemesIndices[shuffleNum]].name.Substring(0, 2)))
            );
            graphemesIndices.Remove(graphemesIndices[shuffleNum]);
        }
        // Instantiate cards
        InstantiateCards(switchLineIndices);
        if (displayedImages.Count > 0)
        {
            card.GetComponent<Card>().elementIndex = pickedImagesIndices[displayedImages[0]];
            card.GetComponent<Card>().isImage = true;
        }
        else
        {
            card.GetComponent<Card>().elementIndex = pickedGraphemesIndices[displayedGraphemes[0]];
            card.GetComponent<Card>().isImage = false;
        }
    }

    /// <summary>
    /// Used to instantiate the cards on the board at the beginning of the game
    private void InstantiateCards(int[] indices)
    {
        // Instantiate cards
        for (int i = 0; i < 2*numberOfPairs; i++)
        {
            if(i == indices[0])
            {
                xPosition = xPosition + 250;
                continue;
            }
            // Decides whether instantiating a grapheme or an image
            shuffleNum = rnd.Next(0, (graphemeOrImage.Count));
            if (graphemeOrImage[shuffleNum] == 0)
            {
                shuffleNumBis = rnd.Next(0, (displayedGraphemes.Count));
                var temp = Instantiate(card, new Vector3(0, 0, 0),Quaternion.identity);
                temp.layer = card.layer;
                temp.transform.SetParent(GameObject.Find("Canvas").transform);
                if (Config.memoryDifficulty == 1)
                    temp.transform.localPosition = new Vector3(xPosition - 125, yPosition - 200, 0);
                else
                    temp.transform.localPosition = new Vector3(xPosition, yPosition, 0);
                temp.transform.localScale = new Vector3(1, 1, 1);
                temp.GetComponent<Card>().isImage = false;
                temp.GetComponent<Card>().elementIndex = pickedGraphemesIndices[displayedGraphemes[shuffleNumBis]];
                displayedGraphemes.Remove(displayedGraphemes[shuffleNumBis]);
            }
            else 
            {
                shuffleNumBis = rnd.Next(0, (displayedImages.Count));
                var temp = Instantiate(card, new Vector3(0, 0, 0),Quaternion.identity);
                temp.layer = card.layer;
                temp.transform.SetParent(GameObject.Find("Canvas").transform);
                if (Config.memoryDifficulty == 1)
                    temp.transform.localPosition = new Vector3(xPosition - 125, yPosition - 200, 0);
                else
                    temp.transform.localPosition = new Vector3(xPosition, yPosition, 0);
                temp.transform.localScale = new Vector3(1, 1, 1);
                temp.GetComponent<Card>().isImage = true;
                temp.GetComponent<Card>().elementIndex = pickedImagesIndices[displayedImages[shuffleNumBis]];
                displayedImages.Remove(displayedImages[shuffleNumBis]);
            }
            graphemeOrImage.Remove(graphemeOrImage[shuffleNum]);
            xPosition = xPosition + 250;
            if (i == indices[1])
            {
                yPosition = 40;
                xPosition = startX;
            }
            if (Config.memoryDifficulty != 1 && i == indices[2])
            {
                yPosition = -210;
                xPosition = startX;
            }
            if (Config.memoryDifficulty == 1)
                card.transform.localPosition = new Vector3(-250, 90, 0);
            circle.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Called when the user has flipped two cards up. Checks whether the match is correct and either flips back the card if it is not or locks them if it is
    public void TwoCardsUp()
    {
        firstCardUp.matched = true;
        secondCardUp.matched = true;
        if (firstCardUp.sprite.name.Substring(0, 2) != secondCardUp.sprite.name.Substring(0, 2) || firstCardUp.isImage == secondCardUp.isImage)
        {
            SoundManager.Instance.Failure();
            firstCardUp.Bounce();
            secondCardUp.Bounce();
            Invoke("FlipWrongPair", 1);
        }
        else
        {
            SoundManager.Instance.Success();
            firstCardUp = null;
            secondCardUp = null;
        }
    }

    private void FlipWrongPair()
    {
        firstCardUp.TurnOverBack();
        secondCardUp.TurnOverBack();
        firstCardUp.matched = false;
        secondCardUp.matched = false;
        firstCardUp = null;
        secondCardUp = null;
    }
}