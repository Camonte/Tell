using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryControl : MonoBehaviour
{
    // Easy mode values
    private List<int> easyGraphemeOrImage = new List<int> {0, 0, 0, 1, 1, 1};
    private List<int> easyDisplayedGraphemes = new List<int> {0, 1, 2};
    private List<int> easyDisplayedImages = new List<int> {0, 1, 2};
    private const int easyNumberOfPairs = 3;
    private const int easyStartX = -125;
    private int[] easySwitchLineIndices = new int[] {0, 1, 3};
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
    
    GameObject card;
    List<int> graphemesIndices;
    List<int> pickedGraphemesIndices;
    List<int> pickedImagesIndices;
    List<int> displayedGraphemes;
    List<int> displayedImages;
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

    public Card firstCardUp;
    public Card secondCardUp;

    private GameObject circle;

    private void Awake()
    {
        card = GameObject.Find("Card");
        circle = GameObject.Find("Circle");
    }
    
    void Start()
    {
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
        // Pick the three pairs that are gonna be used
        xPosition = startX;
        firstCardUp = null;
        secondCardUp = null;
        shuffleNum = 0;
        shuffleNumBis = 0;
        visibleFaces = new int[] { -1, -2 };
        pickedGraphemesIndices = new List<int>();
        pickedImagesIndices = new List<int>();
        yPosition = 290;
        if(Config.consonantsActive & Config.vowelsActive)
            graphemesIndices = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29};
        else if (Config.consonantsActive)
            graphemesIndices = new List<int> {5, 6, 7, 8, 12, 13, 15, 16, 17, 18, 22, 23, 24, 25, 5, 6, 7, 8, 12, 13, 15, 16, 17, 18, 22, 23, 24, 25};
        else 
            graphemesIndices = new List<int> {0, 1, 2, 3, 4, 9, 10, 11, 14, 19, 20, 21, 26, 0, 1, 2, 3, 4, 9, 10, 11, 14, 19, 20, 21, 26};
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
            // Grapheme or Image ?
            shuffleNum = rnd.Next(0, (graphemeOrImage.Count));
            if (graphemeOrImage[shuffleNum] == 0)
            {
                shuffleNumBis = rnd.Next(0, (displayedGraphemes.Count));
                var temp = Instantiate(card, new Vector3(0, 0, 0),Quaternion.identity);
                temp.layer = card.layer;
                temp.transform.SetParent(GameObject.Find("Canvas").transform);
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
            if (i == indices[2])
            {
                yPosition = -210;
                xPosition = startX;
            }
            circle.transform.SetAsLastSibling();
        }
    }

    public void TwoCardsUp()
    {
        firstCardUp.matched = true;
        secondCardUp.matched = true;
        if (firstCardUp.sprite.name.Substring(0, 2) != secondCardUp.sprite.name.Substring(0, 2))
        {
            SoundManager.Instance.Failure();
            firstCardUp.Bounce();
            secondCardUp.Bounce();
            Invoke("FlipWrongPair", 2);
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
        firstCardUp.matched = false;
        secondCardUp.matched = false;
        firstCardUp.TurnOverBack();
        secondCardUp.TurnOverBack();
        firstCardUp = null;
        secondCardUp = null;
    }
}