using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginTest : MonoBehaviour
{
    const string pluginName = "com.juanrg.unity.MyPlugin";
    static AndroidJavaClass pluginClass;
    static AndroidJavaObject pluginInstance;

    public static AndroidJavaClass PluginClass
    {
        get
        {
            if (pluginClass == null)
            {
                pluginClass = new AndroidJavaClass(pluginName);
            }
            return pluginClass;
        }
    }

    public static AndroidJavaObject PluginInstance
    {
        get
        {
            if (pluginInstance == null)
            {
                pluginInstance = PluginInstance.CallStatic<AndroidJavaObject>("getInstance");
            }
            return pluginInstance;
        }
    }
    void Start()
    {
        pluginClass = new AndroidJavaClass(pluginName);
        pluginInstance = pluginClass.CallStatic<AndroidJavaObject>("getInstance");

        Debug.Log("Elapsed time: " + getElapsedTime());
    }

    float elapsedTime = 0;
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 5)
        {
            elapsedTime -= 5;
            Debug.Log("Tick: " + getElapsedTime());
        }
    }

    double getElapsedTime()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return pluginInstance.Call<double>("getElapsedTime");
        }
        Debug.LogWarning("Wrong platform");
        return 0;
    }
}
