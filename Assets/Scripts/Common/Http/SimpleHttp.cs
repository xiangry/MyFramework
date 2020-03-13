using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp()]
[CSharpCallLua]
public class SimpleHttp : MonoBehaviour
{
    private HttpInfo m_info;
    private WWW m_www;
    private float m_during = 0f;

    public static SimpleHttp newInstance
    {
        get
        {
            GameObject gameObject = new GameObject();
            SimpleHttp result = gameObject.AddComponent<SimpleHttp>();
            DontDestroyOnLoad(gameObject);
            return result;
        }
    }

    void Update()
    {
        if (m_info != null && m_www != null)
        {
            m_during += Time.deltaTime;
            if (m_during >= m_info.timeOut)
            {
                try
                {
                    m_www.Dispose();
                    if (m_info.callbackDel != null)
                    {
                        m_info.callbackDel(null);
                        m_info.callbackDel = null;
                        m_info = null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("http timeout callback got exception " + ex.Message + "\n" + ex.StackTrace);
                }

                DestroyImmediate(gameObject);
            }
        }
    }

    public static void HttpGet(string url, Action<WWW> callback, float timeOut = 10f, Dictionary<string, string> formData = null)
    {
        HttpInfo httpInfo = new HttpInfo();
        httpInfo.callbackDel = callback;
        httpInfo.url = url;
        httpInfo.formData = formData;
        httpInfo.type = HTTP_TYPE.GET;
        httpInfo.timeOut = timeOut;
        SimpleHttp.newInstance.StartHttp(httpInfo);
    }

    public static void HttpPost(string url, Dictionary<string, string> formData, byte[] byteData, Action<WWW> callback, float timeOut = 10f)
    {
        HttpInfo httpInfo = new HttpInfo();
        httpInfo.callbackDel = callback;
        httpInfo.url = url;
        httpInfo.formData = formData;
        httpInfo.byteData = byteData;
        httpInfo.type = HTTP_TYPE.POST;
        httpInfo.timeOut = timeOut;
        SimpleHttp.newInstance.StartHttp(httpInfo);
    }
    
    public static void HttpPost(string url, string postData, Action<WWW> callback, float timeOut = 10f)
    {
        HttpInfo httpInfo = new HttpInfo();
        httpInfo.callbackDel = callback;
        httpInfo.url = url;
        httpInfo.postData = postData;
        httpInfo.type = HTTP_TYPE.POST;
        httpInfo.timeOut = timeOut;
        SimpleHttp.newInstance.StartHttp(httpInfo);
    }

    public void StartHttp(HttpInfo info)
    {
        if (info != null)
        {
            if (info.type == HTTP_TYPE.GET)
            {
                StartCoroutine(DoHttpGet(info));
            }

            if (info.type == HTTP_TYPE.POST)
            {
                StartCoroutine(DoHttpPost(info));
            }
        }
    }

    private IEnumerator DoHttpGet(HttpInfo info)
    {
        m_info = info;
        m_www = new WWW(info.url);
        //TODO 简单get请求
        yield return m_www;

        Complete();
    }

    private IEnumerator DoHttpPost(HttpInfo info)
    {
        m_info = info;
        WWWForm form = new WWWForm();
        form.AddField("key", m_info.postData);
        m_www = new WWW(info.url, form);
        yield return m_www;

        if (m_www.error != null)
        {
            Debug.Log("[DoHttpPostWrong]" + m_www.error);
        }
        else if (m_www.responseHeaders.ContainsKey("STATUS") && m_www.responseHeaders["STATUS"].IndexOf("302") > -1)
        {
            if (m_www.responseHeaders.ContainsKey("LOCATION"))
            {

                Debug.LogWarning("[HttpClient GetWWW] STATUS:" + m_www.responseHeaders["STATUS"] + ",errorMessage:" + m_www.error + ", path:");
            }
        }
        else if (m_www.responseHeaders.ContainsKey("STATUS") && m_www.responseHeaders["STATUS"].IndexOf("301") > -1)
        {
            if (m_www.responseHeaders.ContainsKey("LOCATION"))
            {

                Debug.LogWarning("[HttpClient GetWWW] STATUS:" + m_www.responseHeaders["STATUS"] + ",errorMessage:" + m_www.error  );
            }
        }
        Complete();
    }

    private void Complete()
    {
        try
        {
            if (m_info != null && m_info.callbackDel != null)
            {
                m_info.callbackDel(m_www);
//                Debug.Log("<color=green>====================</color>");
//                Debug.Log(m_www.text);
//                Debug.Log("<color=green>====================</color>");
                m_info.callbackDel = null;
            }
            m_info = null;
            m_www.Dispose();
        }
        catch (Exception ex)
        {
            Logger.LogError("http complete callback got exception " + ex.Message + "\n" + ex.StackTrace);
            Logger.Log("http complete callback got exception " + ex.Message + "\n" + ex.StackTrace);
        }

        DestroyImmediate(gameObject);
    }

}
