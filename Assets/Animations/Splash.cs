using UnityEngine;

/// <summary>
/// A splash of color animation.
/// </summary>
public class Splash : MonoBehaviour
{
    ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Plays the splash animation given an array of max two colors, at a given position (world coordinates).
    /// </summary>
    public void WithColors(Color[] colors, Vector2 pos)
    {
        transform.position = pos;

        var main = ps.main;

        var grad = new Gradient();
        grad.mode = GradientMode.Fixed;
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(colors[0], 0.5f), new GradientColorKey(colors.Length == 2 ? colors[1] : colors[0], 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
        );

        var randomColor = new ParticleSystem.MinMaxGradient(grad);
        randomColor.mode = ParticleSystemGradientMode.RandomColor;
        main.startColor = randomColor;

        ps.Play();
    }

}
