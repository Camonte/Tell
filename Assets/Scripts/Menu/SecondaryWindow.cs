/// <summary>
/// Script that listens for the closing of the secondary window (teacher input).
/// </summary>
public class SecondaryWindow : Singleton<SecondaryWindow>
{
    // called from javascript
    public void SecondaryWindowClosed()
    {
        Config.secondaryWindowOpen = false;
    }
}
