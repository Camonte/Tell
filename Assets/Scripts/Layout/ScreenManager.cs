using UnityEngine;

public class ScreenManager : Singleton<ScreenManager>
{
    int width;
    int height;

    void Start()
    {
        width = Screen.width;
        height = Screen.height;
    }

    void Update()
    {
        if (width != Screen.width || height != Screen.height)
        {
            width = Screen.width;
            height = Screen.height;

            SidePanel[] panels = FindObjectsOfType<SidePanel>();

            foreach (var panel in panels)
            {
                panel.Align();
            }

            ScaleManager.Instance.OnUpdateLayout();
        }
    }
}
