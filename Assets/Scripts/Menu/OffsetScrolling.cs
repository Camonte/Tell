using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script for infinite scrolling background in the Main Menu.
/// </summary>
public class OffsetScrolling : MonoBehaviour
{
    Image image;
    [SerializeField] float speed = 0.05f;
    void Start() => image = GetComponent<Image>();

    void Update()
    {
        var offset = image.materialForRendering.mainTextureOffset;
        var newVal = Mathf.Repeat(offset.x + Time.deltaTime * speed, 1);
        offset = new Vector2(newVal, newVal);
        image.materialForRendering.mainTextureOffset = offset;
    } 
}
