using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveRenderTexture : MonoBehaviour {

    public RenderTexture target;
    int index = 0;

    float timer = 2.0f;
    bool isRenderer = false;

    string sceneName;

    string dateTime;
    private void Start()
    {
        //GameObject.Find("SaveBtn").GetComponent<Button>().onClick.AddListener(() => { OnUserSave(); });
        Scene scene = SceneManager.GetActiveScene();
        sceneName = scene.name;

        dateTime = System.DateTime.Now.Hour.ToString() + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString();
    }

    private void Update()
    {
        if (timer <= 0 && isRenderer == false)
        {
            OnUserSave();
            isRenderer = true;
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_STANDALONE_WIN
                Application.Quit();
            #endif
        }
        else if(isRenderer == false)
        {
            timer -= Time.deltaTime;
            Debug.Log(timer);
        }
           
    }



    public void OnUserSave()
    {
        Debug.Log("准备渲染...");
        var prePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));
        string path = prePath + "/SaveFX/"+ sceneName + "  ["+ dateTime + "].png";
        Save(path, CreateFrom(target)); index++; 
    }
    
    public void Save(string path, Texture2D texture2D)
    {
        Debug.Log("Save Path:" + path);
        var bytes = texture2D.EncodeToPNG(); //var bytes = texture2D.EncodeToJPG(); 
        System.IO.File.WriteAllBytes(path, bytes);
    }

    public Texture2D CreateFrom(RenderTexture renderTexture)
    {
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = previous;
        texture2D.Apply();
        return texture2D;
    }
    

     
}
