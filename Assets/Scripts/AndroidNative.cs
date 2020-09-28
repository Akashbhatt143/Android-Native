using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Security.Cryptography;

//http://developer.android.com/reference/android/os/Build.VERSION.html
public class DeviceInfo
{
    public string CODENAME;
    public string INCREMENTAL;
    public string RELEASE;
    public int SDK;
}

//http://developer.android.com/reference/android/content/pm/PackageInfo.html
public class PackageInfo
{
    public long firstInstallTime;
    public long lastUpdateTime;
    public string packageName;
    public int versionCode;
    public string versionName;
}

public class AppInfo
{
    public long firstInstallTime;
    public long lastUpdateTime;
    public string packageName;
    public int versionCode;
    public string versionName;
    public AndroidJavaObject applicationInfo;
    public string AppName;
}

public class AndroidNative : MonoBehaviour
{
    private static bool immersiveMode;
    private static AndroidNative instance;

    private static AndroidJavaObject currentActivity
    {
        get
        {
            return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    private static AndroidJavaObject MyNativeClass
    {
        get
        {
            return new AndroidJavaClass("com.akki.nativeunity.NativeUnity");
        }
    }

    private static void CreateGO()
    {
        if (instance != null)
            return;
        GameObject go = new GameObject("AndroidNative");
        instance = go.AddComponent<AndroidNative>();
    }

    void OnApplicationFocus(bool focusStatus)
    {
        if (immersiveMode && focusStatus)
        {
            ImmersiveMode();
        }
    }

    //=====================================================================================================

    public static void StartApp(string packageName, bool isExitThisApp)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        AndroidJavaObject launch = currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
        currentActivity.Call("startActivity", launch);

        if (isExitThisApp)
        {
            Application.Quit();
        }
    }

    //=====================================================================================================

    public static List<PackageInfo> GetInstalledAppsPackages()
    {
        if (Application.platform != RuntimePlatform.Android)
            return null;

        AndroidJavaObject packages = currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getInstalledPackages", 0);
        int size = packages.Call<int>("size");
        List<PackageInfo> list = new List<PackageInfo>();

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject info = packages.Call<AndroidJavaObject>("get", i);
            PackageInfo packageInfo = new PackageInfo();
            packageInfo.firstInstallTime = info.Get<long>("firstInstallTime");
            packageInfo.packageName = info.Get<string>("packageName");
            packageInfo.lastUpdateTime = info.Get<long>("lastUpdateTime");
            packageInfo.versionCode = info.Get<int>("versionCode");
            packageInfo.versionName = info.Get<string>("versionName");
            list.Add(packageInfo);
        }

        return list;
    }

    public static List<AppInfo> GetInstalledApps()
    {
        if (Application.platform != RuntimePlatform.Android)
            return null;

        AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        int flag = pm.GetStatic<int>("GET_META_DATA");

        AndroidJavaObject packages = pm.Call<AndroidJavaObject>("getInstalledPackages", flag);
        int size = packages.Call<int>("size");
        List<AppInfo> list = new List<AppInfo>();

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject info = packages.Call<AndroidJavaObject>("get", i);//PackageInfo
            AppInfo appInfo = new AppInfo();

            if (info != null)
            {
                try
                {
                    appInfo.applicationInfo = info.Get<AndroidJavaObject>("applicationInfo");

                    if (appInfo.applicationInfo != null)
                    {
                        if (!MyNativeClass.CallStatic<bool>("isSystem", appInfo.applicationInfo))
                        {
                            appInfo.firstInstallTime = info.Get<long>("firstInstallTime");
                            appInfo.packageName = info.Get<string>("packageName");
                            appInfo.lastUpdateTime = info.Get<long>("lastUpdateTime");
                            appInfo.versionCode = info.Get<int>("versionCode");
                            appInfo.versionName = info.Get<string>("versionName");
                            appInfo.AppName = pm.Call<string>("getApplicationLabel", appInfo.applicationInfo);
                            list.Add(appInfo);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        return list;
    }

    public static byte[] GetAppIcon(string packageName)
    {
        if (Application.platform != RuntimePlatform.Android)
            return null;

        sbyte[] decodedBytes = null;

        AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        if (pm == null)
        {
            Debug.LogError("PM is null");
        }
        int flag = pm.GetStatic<int>("GET_META_DATA");

        AndroidJavaObject packages = pm.Call<AndroidJavaObject>("getInstalledPackages", flag);
        int size = packages.Call<int>("size");

        for (int i = 0; i < size; i++)
        {
            AndroidJavaObject info = packages.Call<AndroidJavaObject>("get", i);

            if (info != null)
            {
                try
                {
                    string getPackage = info.Get<string>("packageName");

                    if (string.IsNullOrEmpty(getPackage))
                    {
                        Debug.LogError("Package: Is Null");
                    }

                    if (getPackage == packageName)
                    {
                        AndroidJavaObject appInfo = info.Get<AndroidJavaObject>("applicationInfo");

                        if (appInfo == null)
                        {
                            Debug.LogError("AppInfo is Null");
                        }

                        decodedBytes = MyNativeClass.CallStatic<sbyte[]>("getIcon", currentActivity, pm, appInfo);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Icon: " + e.ToString());
                }
            }
        }

        return (byte[])(Array)decodedBytes;
    }

    //=====================================================================================================

    public static PackageInfo GetAppInfo()
    {
        return GetAppInfo(currentActivity.Call<string>("getPackageName"));
    }

    public static PackageInfo GetAppInfo(string packageName)
    {
        if (Application.platform != RuntimePlatform.Android)
            return null;
        AndroidJavaObject info = currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
        PackageInfo packageInfo = new PackageInfo();
        packageInfo.firstInstallTime = info.Get<long>("firstInstallTime");
        packageInfo.packageName = info.Get<string>("packageName");
        packageInfo.lastUpdateTime = info.Get<long>("lastUpdateTime");
        packageInfo.versionCode = info.Get<int>("versionCode");
        packageInfo.versionName = info.Get<string>("versionName");
        return packageInfo;
    }

    //=====================================================================================================

    public static DeviceInfo GetDeviceInfo()
    {
        if (Application.platform != RuntimePlatform.Android)
            return null;
        AndroidJavaClass build = new AndroidJavaClass("android.os.Build$VERSION");
        DeviceInfo deviceInfo = new DeviceInfo();
        deviceInfo.CODENAME = build.GetStatic<string>("CODENAME");
        deviceInfo.INCREMENTAL = build.GetStatic<string>("INCREMENTAL");
        deviceInfo.RELEASE = build.GetStatic<string>("RELEASE");
        deviceInfo.SDK = build.GetStatic<int>("SDK_INT");
        return deviceInfo;
    }

    //=====================================================================================================

    public static string GetAndroidID()
    {
        if (Application.platform != RuntimePlatform.Android)
            return "";
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        return new AndroidJavaClass("android.provider.Settings$Secure").CallStatic<string>("getString", contentResolver, "android_id");
    }

    //=====================================================================================================

    public static void ImmersiveMode()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;
        int sdk = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
        if (sdk < 19)
            return;
        CreateGO();
        immersiveMode = true;
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass cView = new AndroidJavaClass("android.view.View");
            AndroidJavaObject id = currentActivity.Call<AndroidJavaObject>("findViewById", new AndroidJavaClass("android.R$id").GetStatic<int>("content"));
            id.Call("setSystemUiVisibility", cView.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_STABLE") |
            cView.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION") |
            cView.GetStatic<int>("SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN") |
            cView.GetStatic<int>("SYSTEM_UI_FLAG_HIDE_NAVIGATION") |
            cView.GetStatic<int>("SYSTEM_UI_FLAG_FULLSCREEN") |
            cView.GetStatic<int>("SYSTEM_UI_FLAG_IMMERSIVE_STICKY"));
        }));
    }

    //=====================================================================================================

    public static bool isInstalledApp(string packageName)
    {
        if (Application.platform != RuntimePlatform.Android)
            return false;
        try
        {
            currentActivity.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
