using UnityEngine;
using UnityEngine.UI;

public class AppItem : MonoBehaviour
{
    public RawImage icon;
    public Text appNameText;
    public Text packageNameText;

    private string packageName;

    public void SetData(AppInfo appInfo)
    {
        packageName = appInfo.packageName;

        appNameText.text = appInfo.AppName;
        packageNameText.text = packageName;

        if (!string.IsNullOrEmpty(packageName))
        {
            byte[] decodedBytes = AndroidNative.GetAppIcon(packageName);
            Texture2D text = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            text.LoadImage(decodedBytes);
            icon.texture = text;
        }
    }

    public void StartApp()
    {
        if (!string.IsNullOrEmpty(packageName))
        {
            AndroidNative.StartApp(packageName, false);
        }
    }
}
