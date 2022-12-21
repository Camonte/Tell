using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the side panels which display the consonnants (left) and vowels (right).
/// </summary>
public class SidePanel : MonoBehaviour
{

    [SerializeField] Phoneme.Type type;
    [SerializeField] Positionable shapes;
    [SerializeField] Positionable scaleZone;
    private LineRenderer line;
    private Camera _camera;
    private float cameraHeight;
    private Vector2 offset;

    /// <summary>
    /// Maximum Y coordinate of the left panel. 
    /// </summary>
    public static float leftMaxY;
    /// <summary>
    /// Maximum Y coordinate of the right panel.
    /// </summary>
    public static float rightMaxY;
    /// <summary>
    /// Right-most X coordinate of the left panel.
    /// </summary>
    public static float leftPanelX;
    /// <summary>
    /// Left-most X coordinate of the right panel.
    /// </summary>
    public static float rightPanelX;

    private static Dictionary<string,Vector2> vowelPositions = new Dictionary<string, Vector2>() {
        { "y", new Vector2(-1.1f, 4.6f) },
        { "i", new Vector2(0, 4.6f) },
        { "2", new Vector2(-1.1f, 3) },
        { "e", new Vector2(0, 3) },
        { "9", new Vector2(-1.1f, 1.4f) },
        { "E", new Vector2(0, 1.4f) },
        { "e~", new Vector2(1.1f, 1.4f) },
        { "a", new Vector2(-0.5f, -0.2f) },
        { "a~", new Vector2(1.1f, -0.2f) },
        { "O", new Vector2(-0.5f, -1.8f) },
        { "o~", new Vector2(1.1f, -1.8f) },
        { "o", new Vector2(-0.5f, -3.4f) },
        { "u", new Vector2(-0.5f, -5) },
    };

    private static Dictionary<string,Vector2> consonantPositions = new Dictionary<string, Vector2>() {
        { "m", new Vector2(-1.4f, 4.6f) },
        { "f", new Vector2(0.1f, 4.1f) },
        { "v", new Vector2(1.2f, 4.1f) },
        { "p", new Vector2(-0.4f, 3f) },
        { "b", new Vector2(0.7f, 3f) },
        { "n", new Vector2(-1.4f, 1.4f) },
        { "s", new Vector2(0.1f, 1.1f) },
        { "z", new Vector2(1.2f, 1.1f) },
        { "t", new Vector2(-0.4f, 0) },
        { "d", new Vector2(0.7f, 0) },
        { "S", new Vector2(0.1f, -1.1f) },
        { "Z", new Vector2(1.2f, -1.1f) },
        { "l", new Vector2(-1f, -2.4f) },
        { "j", new Vector2(0.1f, -2.5f) },
        { "R", new Vector2(-0.8f, -3.5f) },
        { "k", new Vector2(-0.4f, -5) },
        { "g", new Vector2(0.7f, -5) },
    };

    private static Dictionary<string, Vector2> extraVowelPositions = new Dictionary<string, Vector2>()
    {
        { "", new Vector2(1.1f, -5) },
        { "u a", new Vector2(1.1f, -3.4f)},

    };

    private static Dictionary<string, Vector2> extraConsonantPositions = new Dictionary<string, Vector2>()
    {
        { "n j", new Vector2(-1.4f, -1.1f)},
        { "g z", new Vector2(0.7f, -6.1f)},
        { "k s", new Vector2(-0.4f, -6.1f)},
    };

    private static Dictionary<string, GameObject> generators = new Dictionary<string, GameObject>();

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        _camera = Camera.main;
        cameraHeight = _camera.orthographicSize;
        generators.Clear();
    }

    void Start()
    {
        var expandable = FindObjectOfType<Tutorial>() == null && Config.gameType == Config.GameType.ShapeToWord;

        var dict = type == Phoneme.Type.Vowel ? vowelPositions : consonantPositions;
        var extraDict = type == Phoneme.Type.Vowel ? extraVowelPositions : extraConsonantPositions;

        // scale shapes object and add it's shapes
        var scale = ScaleManager.Instance.GetScaleVector();
        shapes.transform.parent.localScale = scale;
        Physics2D.SyncTransforms();

        // create and add all generators
        foreach(var pair in dict)
        {
            generators[pair.Key] = ShapeManager.Instance.CreatePhonemeGenerator(pair.Key, pair.Value, scale.x, shapes.transform, expandable);
        }
        if (expandable)
        {
            foreach(var pair in extraDict)
                generators[pair.Key] = ShapeManager.Instance.CreatePhonemeGenerator(pair.Key, pair.Value, scale.x, shapes.transform, expandable);
        }
        shapes.Align();

        // update scale zone to fit the shapes
        //var shapesWidth = shapes.Bounds().size.x;
        scaleZone.Align();
        var scaleZonePos = scaleZone.transform.parent.position;
        scaleZonePos.z = 1;
        scaleZone.transform.parent.position = scaleZonePos;

        //scaleZone.transform.parent.localScale = new Vector2(shapesWidth, 2 * cameraHeight);
        //Physics2D.SyncTransforms();

        if (type == Phoneme.Type.Consonant)
        {
            leftPanelX = shapes.Bounds().max.x;
            leftMaxY = shapes.Bounds().max.y;
        }
        else if (type == Phoneme.Type.Vowel)
        {
            rightPanelX = shapes.Bounds().min.x;
            rightMaxY = shapes.Bounds().max.y;
        }
        MoveLine();
    }

    /// <summary>
    /// Is this position contained inside one of the side panels ?
    /// </summary>
    public static bool Contains(Vector2 pos)
    {
        return pos.x < leftPanelX || pos.x > rightPanelX;
    }

    /// <summary>
    /// Generator object for a given Phoneme.
    /// </summary>
    public static GameObject GeneratorFor(string phoneme)
    {
        if (generators.ContainsKey(phoneme))
            return generators[phoneme];
        return null;
    }

    /// <summary>
    /// Generator objects from the side panels.
    /// </summary>
    public static GameObject[] Generators(string except = null)
    {
        if (except == null)
            return generators.Values.ToArray();
        return generators.Where(pair => pair.Key != except).Select(pair => pair.Value).ToArray();
    }

    public void OnUpdateScale(float scale)
    {
        shapes.transform.parent.localScale = new Vector2(scale, scale);
        Physics2D.SyncTransforms();
        scaleZone.transform.parent.localScale = new Vector2(shapes.Bounds().size.x + offset.x, 2 * cameraHeight);

        if (type == Phoneme.Type.Consonant)
        {
            leftPanelX = shapes.Bounds().max.x;
            leftMaxY = shapes.Bounds().max.y;
        }
        else if (type == Phoneme.Type.Vowel)
        {
            rightPanelX = shapes.Bounds().min.x;
            rightMaxY = shapes.Bounds().max.y;
        }
        MoveLine();
    }

    public void UpdatePos(Vector2 delta)
    {
        UpdatePos(delta, true);
    }

    public void UpdatePosOther(Vector2 delta)
    {
        UpdatePos(delta, false);
    }

    /// <summary>
    /// Uglyyyy code linked to the Global Tap that closes currently expanded Expandable Phonemes. tbh don't even remember why this is useful.
    /// </summary>
    public void CollapseAllOpened(Vector2 screenPos)
    {
        if (DropZoneExpand.currentlyExpandedZone == null) return;

        var ray = _camera.ScreenPointToRay(screenPos);
        var hits = Physics2D.GetRayIntersectionAll(ray);
        
        foreach(var hit in hits)
        {
            if (hit.collider.GetComponentInParent<DropZoneExpand>() != null)
            {
                return;
            }
        }
        DropZoneExpand.currentlyExpandedZone.Collapse();
    }

    private void UpdatePos(Vector2 delta, bool self)
    {
        var wasLeft = (self && type == Phoneme.Type.Consonant) || (!self && type == Phoneme.Type.Vowel);
        var originalDeltaX = delta.x;

        // account for symmetry
        delta.x *= self ? 1 : -1;
        shapes.transform.parent.position += new Vector3(delta.x, delta.y, 0);

        // TODO clamp

        // update scale zone
        if (type == Phoneme.Type.Consonant)
        {
            leftPanelX += delta.x;
            leftMaxY += delta.y;
        }
        else if (type == Phoneme.Type.Vowel)
        {
            rightPanelX += delta.x;
            rightMaxY += delta.y;
        }
        scaleZone.transform.parent.localScale += (wasLeft ? Vector3.right : Vector3.left) * originalDeltaX;
        offset.x += wasLeft ? originalDeltaX : -originalDeltaX;
        offset.y += delta.y;
        MoveLine();

        ScaleManager.Instance.OnUpdateLayout();
    }

    private void MoveLine()
    {
        if (type == Phoneme.Type.Consonant)
        {
            line.SetPosition(0, new Vector3(leftPanelX, cameraHeight, 0));
            line.SetPosition(1, new Vector3(leftPanelX, -cameraHeight, 0));
        }
        else if (type == Phoneme.Type.Vowel)
        {
            line.SetPosition(0, new Vector3(rightPanelX, cameraHeight, 0));
            line.SetPosition(1, new Vector3(rightPanelX, -cameraHeight, 0));
        }
    }


    private Vector2 oldOffset;

    /// <summary>
    /// TODO
    /// Aligns the side pannel to the side of the screen (called when the browser's window is resized, makes sure the pannels don't disapear outside of the window).
    /// </summary>
    public void Align()
    {
        /*shapes.Align();
        Physics2D.SyncTransforms();

        // reset scale zone
        scaleZone.transform.parent.position = shapes.transform.parent.position;
        scaleZone.transform.parent.localScale = new Vector2(shapes.Bounds().size.x, 2 * cameraHeight);

        if (type == Phoneme.Type.Consonant)
        {
            leftPanelX = shapes.Bounds().max.x;
            leftMaxY = shapes.Bounds().max.y;
        }
        else if (type == Phoneme.Type.Vowel)
        {
            rightPanelX = shapes.Bounds().min.x;
            rightMaxY = shapes.Bounds().max.y;
        }

        xOffset = 0;
        
        MoveLine();
        */

        /*
        if (type == Phoneme.Type.Consonant)
        {
            var x = leftPanelX - shapes.Bounds().size.x + offset.x;
            if (_camera.WorldToScreenPoint(new Vector2(x, 0)).x < 0f)
            {
                var min = _camera.ScreenToWorldPoint(new Vector2(0, Screen.height)).x;
                var pos = shapes.transform.parent.position;
                pos.x = min;
                shapes.transform.parent.position = pos;
                leftPanelX = shapes.Bounds().max.x;
            }
        }
        else if (type == Phoneme.Type.Vowel)
        {
            var x = rightPanelX + shapes.Bounds().size.x + offset.x;
            if (_camera.WorldToScreenPoint(new Vector2(x, 0)).x > Screen.width)
            {
                var min = _camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)).x;
                var pos = shapes.transform.parent.position;
                pos.x = min;
                shapes.transform.parent.position = pos;
                rightPanelX = shapes.Bounds().min.x;
            }
        }

        scaleZone.transform.parent.position = shapes.transform.parent.position;

        MoveLine();
        */
    }

    public void ClearDict()
    {
        foreach (var pair in generators)
        {
            Destroy(generators[pair.Key]);
        }
        generators.Clear();
    }

    public void ReloadPanel()
    {
        UpdatePos(-offset);
        UpdatePosOther(-offset);
        //scaleZone.c = null;
        //scaleZone.Awake();
        ScaleManager.Instance.SetScale(Config.defaultScale);
        Start();
    }
}
