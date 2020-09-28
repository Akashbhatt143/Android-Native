using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppListPanel : MonoBehaviour
{
    public AppItem appItemPrefab;
    public Transform prefabParent;

    WaitForSeconds wait = new WaitForSeconds(0.5f);
    List<AppItem> appItems = new List<AppItem>();

    private void Start()
    {
        SetAppListData(AndroidNative.GetInstalledApps());
    }

    private void OnDisable()
    {
        if (appItems.Count > 0)
        {
            for (int i = 0; i < appItems.Count; i++)
            {
                Destroy(appItems[i].gameObject);
            }
        }

        appItems.Clear();
    }

    public void SetAppListData(List<AppInfo> appInfo)
    {
        if (appInfo.Count > 0)
        {
            StartCoroutine(InitItem(appInfo));
        }
    }

    IEnumerator InitItem(List<AppInfo> appInfo)
    {
        for (int i = 0; i < appInfo.Count; i++)
        {
            AppItem appItem = Instantiate(appItemPrefab, prefabParent);
            appItem.SetData(appInfo[i]);
            appItems.Add(appItem);
            yield return wait;
        }
    }
}
