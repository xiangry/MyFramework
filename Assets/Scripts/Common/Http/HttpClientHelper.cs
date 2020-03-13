using System.IO;
using System;
using System.Net;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using XLua;

public delegate void OnProgressHandler(string url, string filePath, long downloadSize, long totalSize);
public delegate void OnErrorHandler(string url, string filePath, int code, string msg);
public delegate void OnSuccHandler(string url, string filePath, int size, string md5);
public delegate void OnURLRedirectHandler(string url, string filePath, string newUrl, int code, string msg);




public enum HTTP_OP_STEP
{
	unkonw = -1,
	connecting = 0,
	redirect = 1,
	downloading = 2,
	finished = 3,
}

public enum HTTP_OP_RET
{
	unknown = -1,
	succ = 0,
	failed = 1,
	save_failed = 2,
	time_out = 3,
}

[Hotfix()]
[CSharpCallLua]
[LuaCallCSharp()]
public class HttpClientHelper
{
	const int BUFFER_SIZE = 1024 * 10;
	const int TIMEOUT = 10 * 1000;
	const int CONNTECTTIMEOUT = 10 * 1000;

	int m_Id = 0;
	bool m_alive = false;
	bool m_OPComplete = false;

	string m_url;
	string m_filePath;
	string m_saveDirectory;
	HttpWebRequest m_request;
	HttpWebResponse m_reponse;
	Stream m_responseStream;
	private byte[] m_buffer = new byte[BUFFER_SIZE];
	private MemoryStream m_fileBuffStream = null;


	string m_redirectURL = null;

	long m_downloadSize = 0;
	long m_totalSize = 0;
	string m_fileMd5 = "";

	
	//Stream m_fileStream;

	
	OnProgressHandler m_onProgressHandler;
	OnErrorHandler m_onErrorHandler;
	OnSuccHandler m_onSuccHandler;
	OnURLRedirectHandler m_onRedirectHandler;

	bool m_onSuccCalled = false;
	bool m_onRedirectCalled = false;

	private HTTP_OP_STEP m_opStep = HTTP_OP_STEP.unkonw;
	private HTTP_OP_RET m_opRet = HTTP_OP_RET.unknown;
	private int m_httpStatusCode = -1;
	private string m_msg;


	private float m_updateDeltaTime = 0;
	
	public HttpClientHelper(string filePath, string url, int id)
	{
		m_filePath = filePath;
		m_url = url;
		m_Id = id;
	}
		
	~HttpClientHelper()
	{
	}
	
	public void SetCallback(OnProgressHandler onProgress, OnErrorHandler onError, OnSuccHandler onSucc, OnURLRedirectHandler onRedirect)
	{
		m_onProgressHandler = onProgress;
		m_onErrorHandler = onError;
		m_onSuccHandler = onSucc;
		m_onRedirectHandler = onRedirect;
	}

	
	public void Start()
	{
		m_alive = true;
		m_OPComplete = false;

		m_opRet = HTTP_OP_RET.unknown;

		Uri uri = new Uri(m_url);
		try
		{
			m_opStep = HTTP_OP_STEP.connecting;

			m_request = (HttpWebRequest)WebRequest.Create(uri);
			m_request.AllowAutoRedirect = false;
			IAsyncResult iar = (IAsyncResult)m_request.BeginGetResponse(new AsyncCallback(GetResponseCallback), this);
			ThreadPool.RegisterWaitForSingleObject(iar.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), this, CONNTECTTIMEOUT, true);
		}
		catch (Exception e)
		{	
			Close (HTTP_OP_RET.failed, -1, e.ToString());
		}
	}
	
	private static void TimeoutCallback(object state, bool timeOut)
	{
		HttpClientHelper dlf = (HttpClientHelper)state;
		if(timeOut)
		{
			dlf.Close(HTTP_OP_RET.time_out, (int)HttpStatusCode.RequestTimeout, "");
		}
	}
	
	private static void GetResponseCallback(IAsyncResult ar)
	{
		HttpClientHelper dlf = (HttpClientHelper)ar.AsyncState;
		try
		{
			dlf.m_reponse = (HttpWebResponse)dlf.m_request.EndGetResponse(ar);

			HttpStatusCode code = dlf.m_reponse.StatusCode;
			if (code == HttpStatusCode.Found||
			    code == HttpStatusCode.Redirect ||
			    code == HttpStatusCode.Moved ||
			    code == HttpStatusCode.MovedPermanently)
			{
				dlf.m_redirectURL = dlf.m_reponse.Headers["Location"];
				dlf.Close(HTTP_OP_RET.failed, (int)code, dlf.m_reponse.StatusDescription);
				return;
			}


			dlf.m_totalSize = dlf.m_reponse.ContentLength;	
			dlf.m_responseStream = dlf.m_reponse.GetResponseStream();

			dlf.m_fileBuffStream = new MemoryStream();
			dlf.m_opStep = HTTP_OP_STEP.downloading;

			IAsyncResult iarRead = dlf.m_responseStream.BeginRead(dlf.m_buffer, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), dlf);
			ThreadPool.RegisterWaitForSingleObject(iarRead.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), dlf, TIMEOUT, true);
		}
		catch (WebException e)
		{
			HttpStatusCode code = HttpStatusCode.BadRequest;

			if(e.Status == WebExceptionStatus.Timeout)
			{
				code = HttpStatusCode.RequestTimeout;
			}
			else if (e.Status == WebExceptionStatus.ConnectFailure)
			{
				code = HttpStatusCode.InternalServerError;
			}
			else
			{
				HttpWebResponse response = e.Response as HttpWebResponse;
				code = response.StatusCode;
			}

			dlf.Close(HTTP_OP_RET.failed, (int)code, e.ToString());
		}
		catch (Exception e)
		{
			dlf.Close(HTTP_OP_RET.failed, -1, e.ToString());
		}
	}

	private static void ReadCallback(IAsyncResult asyncResult)
	{
		HttpClientHelper dlf = (HttpClientHelper)asyncResult.AsyncState;
		
		if(dlf.m_responseStream == null)
		{
			return;
		}
		
		try
		{
			int read = dlf.m_responseStream.EndRead(asyncResult);
			if (read > 0)
			{
				dlf.m_fileBuffStream.Write(dlf.m_buffer, 0, read);
				dlf.m_downloadSize += read;

				if (dlf.m_downloadSize < dlf.m_totalSize)
				{
					IAsyncResult iarRead = dlf.m_responseStream.BeginRead(dlf.m_buffer, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), dlf);
					ThreadPool.RegisterWaitForSingleObject(iarRead.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), dlf, TIMEOUT, true);
				}
				else
				{
					dlf.Close(HTTP_OP_RET.succ, (int)HttpStatusCode.OK, "");
				}
			}
			else
			{
				//TODO: kevin. will it raise an excpetion ?
				//dlf.Close(HTTP_OP_RET.failed, -1, "ReadCallback");
			}
		}
		catch (WebException e)
		{
			HttpWebResponse response = e.Response as HttpWebResponse;
			dlf.Close(HTTP_OP_RET.failed, (int)response.StatusCode, e.ToString());
		} 
		catch(Exception e)
		{
			dlf.Close(HTTP_OP_RET.failed, -1, e.ToString());
		}
	}
	
	public void Close(HTTP_OP_RET ret, int httpCode, string msg)
	{
		this.m_opRet = ret;
		this.m_httpStatusCode = httpCode;
		this.m_msg = msg;

		/*
		switch (ret) {
		case HTTP_OP_RET.succ:
			break;
		case HTTP_OP_RET.failed:
			break;
		case HTTP_OP_RET.time_out:
			break;
		case HTTP_OP_RET.unknown:
			break;		
		}
		*/

		if (m_responseStream != null)
		{
			m_responseStream.Close();
			m_responseStream = null;
		}
		
		if (m_reponse != null)
		{
			m_reponse.Close();
			m_reponse = null;
		}
		
		if (m_request != null)
		{
			m_request.Abort();
			m_request = null;
		}

		this.m_buffer = null;

		m_OPComplete = true;
	}

	public static bool Save(MemoryStream m, string path)
	{
		if (m != null)
		{
			string CompletePath = Core.Resource.FileUtil.GetWritePath(path);
			if (Core.Resource.FileUtil.CreateFolderByFile(CompletePath))
			{
				try
				{
					if (File.Exists(CompletePath))
						File.Delete(CompletePath);
					FileStream fs = File.Create(CompletePath);
					fs.Write(m.GetBuffer(), Convert.ToInt32(m.Position), Convert.ToInt32(m.Length - m.Position));
					fs.Flush();
					fs.Close();
					return true;
				}
				catch (System.Exception ex)
				{
					Logger.LogError(ex.ToString());
				}
			}
		}
		return false;
	}
	
	public void Update()
	{
		if (!m_alive)
			return;

		if (m_OPComplete) {
			if (m_opRet == HTTP_OP_RET.succ )
			{
				m_fileBuffStream.Position = 0;
				string md5Code = UpdateUtils.Md5(this.m_fileBuffStream);
				m_fileBuffStream.Position = 0;
				if (!Save(m_fileBuffStream, m_filePath))
				{
					this.m_opRet = HTTP_OP_RET.failed;
					this.m_httpStatusCode = -1;
					this.m_msg = "Save File Failed";
					return;
				}

				if (null != this.m_onProgressHandler){
					m_onProgressHandler(this.m_url, this.m_filePath, this.m_downloadSize, this.m_totalSize);
				}

				if (null != m_onSuccHandler){
					m_onSuccHandler(m_url, m_filePath, (int)m_totalSize, md5Code);
				}
			}
			else
			{
				if (m_redirectURL != null)
				{
					if (null != this.m_onRedirectHandler)
						m_onRedirectHandler(m_url, m_filePath, m_redirectURL, m_httpStatusCode, m_msg);
				}
				else
				{
					if (null != this.m_onErrorHandler)
						m_onErrorHandler(m_url, m_filePath, m_httpStatusCode, m_msg);
				}

			}

			m_alive = false;
		} else {
			if (m_opStep == HTTP_OP_STEP.downloading)
			{
				m_updateDeltaTime += Time.deltaTime;
				if (m_updateDeltaTime > 0.5)
				{
					m_updateDeltaTime = 0;
					if (null != this.m_onProgressHandler){
						m_onProgressHandler(this.m_url, this.m_filePath, this.m_downloadSize, this.m_totalSize);
					}
				}
			}
		}
	}
	
	public string Md5(string source)
	{
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(source));
		return MakeMD5String(result);
	}
	
	public string MakeMD5String(byte[] md5)
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < md5.Length; ++i)
		{
			sb.Append(md5[i].ToString("x2"));
		}
		return sb.ToString();
	}
}
