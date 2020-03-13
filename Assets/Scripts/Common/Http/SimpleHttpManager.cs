using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using XLua;

[Hotfix()]
[LuaCallCSharp()]
public class LoopCheckHelper : UnityEngine.MonoBehaviour
{
	private SimpleHttpManager target = null;
	
	public void SetTarget(SimpleHttpManager dl)
	{
		target = dl;
	}
	
	void FixedUpdate()
	{
		if (null != target)
			target.Update();
	}
}

[Hotfix()]
[LuaCallCSharp()]
public class SimpleHttpManager {

	private int m_workerSerNum = 0;
	private static SimpleHttpManager m_instance = null;
	public static SimpleHttpManager GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new SimpleHttpManager();
			m_instance.InitOnce();
		}
		return m_instance;
	}
	
	private bool hasInit = false;

	//TODO: only one worker yet.
	HttpClientHelper m_httpWorker;
	
	OnProgressHandler m_onProgressHandler;
	OnErrorHandler m_onErrorHandler;
	OnSuccHandler m_onSuccHandler;
	OnURLRedirectHandler m_onRedirectHandler;

	string m_fileName;
	MemoryStream m_downLoadContent;

	private GameObject m_loopCheckHelper;

	public void InitOnce()
	{
		m_loopCheckHelper = new GameObject("__SimpleHttpManagerLoopHelper__");
		LoopCheckHelper lch = m_loopCheckHelper.AddComponent<LoopCheckHelper>();
		lch.SetTarget(this);
		UnityEngine.Object.DontDestroyOnLoad(m_loopCheckHelper);
		m_loopCheckHelper.SetActive (false);
	}

	public bool DownLoadFile(string url, string saveFile, OnProgressHandler onProgress, OnErrorHandler onError, OnSuccHandler onSucc, OnURLRedirectHandler onRedirect)
	{
		bool hasTask = m_httpWorker != null;

		m_downLoadContent = null;

		m_onProgressHandler = onProgress;
		m_onErrorHandler = onError;
		m_onSuccHandler = onSucc;
		m_onRedirectHandler = onRedirect;

		m_fileName = saveFile;
		m_httpWorker = new HttpClientHelper(saveFile, url, ++m_workerSerNum);
		m_loopCheckHelper.SetActive (true);

		m_httpWorker.SetCallback(new OnProgressHandler(this.OnProgress), new OnErrorHandler(this.OnError), new OnSuccHandler(this.OnSucc),
		                       new OnURLRedirectHandler(this.OnReidrect));
		m_httpWorker.Start();

		return hasTask;
	}
	

	public void OnProgress(string url, string filePath, long size, long totalSize)
	{
		if (m_onProgressHandler != null)
		{
			m_onProgressHandler(url, filePath, size, totalSize);
		}
	}

	public void OnError(string url, string filePath, int code, string msg)
	{
		m_httpWorker = null;
		m_loopCheckHelper.SetActive (false);

		if (m_onErrorHandler != null)
		{
			m_onErrorHandler(url, filePath, code, msg);
		}
	}

	public void OnSucc(string url, string filePath, int size, string md5)
	{
		m_httpWorker = null;
		m_loopCheckHelper.SetActive (false);

		if (m_onSuccHandler != null)
		{
			m_onSuccHandler(url, filePath, size, md5);
		}
	}

	public void OnReidrect(string url, string filePath, string newUrl, int code, string msg)
	{	
		m_httpWorker = null;
		m_loopCheckHelper.SetActive (false);

		if (m_onRedirectHandler != null)
		{
			m_onRedirectHandler(url, filePath, newUrl, code, msg);
		}
	}

	public void Update()
	{
		if (m_httpWorker != null) {
			m_httpWorker.Update();
		}
	}
}

#if UNITY_EDITOR
public static class SimpleHttpManagerExporter
{
	[LuaCallCSharp]
	public static List<Type> LuaCallCSharp = new List<Type>()
	{
		typeof(LoopCheckHelper),
		typeof(SimpleHttpManager),
	};
	
	[CSharpCallLua]
	public static List<Type> CSharpCallLua = new List<Type>()
	{
		typeof(OnProgressHandler),
		typeof(OnErrorHandler),
		typeof(OnSuccHandler),
		typeof(OnURLRedirectHandler),
	};
}
#endif