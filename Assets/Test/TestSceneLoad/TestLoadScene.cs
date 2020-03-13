using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestLoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Button btn = GameObject.Find("Button").GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            Debug.Log("Press Button ------- ");
            StartCoroutine(LoadSotScene());
        });
    }

    IEnumerator LoadSotScene()
    {
//        var uri = "file:///" + Application.streamingAssetsPath + "/Independent/gamesystem/sot_library/songoftime_unity.assetbundle";
        var uri = "file:///" + Application.streamingAssetsPath + "/Independent/maps/battlescene_unity.assetbundle";
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri, 0);
        
        yield return request.SendWebRequest();
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
//        SceneManager.LoadSceneAsync("songoftime");

        
//        yield return new WaitForSeconds(5);


        StartCoroutine(LoadSceneAsync("battle scene"));
//        string name = "sont of time";
//        AsyncOperation op = SceneManager.LoadSceneAsync(name);
//        
//        while (op.isDone != true) {
//            Debug.Log("LoadScene " + name + " ---------------- progress:" + op.progress);
//            yield return new WaitForEndOfFrame();
//        }
//        op.allowSceneActivation = true;
    }
    

    private IEnumerator LoadSceneAsync(string name)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(name);
        while (op.isDone != true) {
            Debug.Log("LoadScene " + name + " ---------------- progress:" + op.progress);
            yield return new WaitForEndOfFrame();
        }
        op.allowSceneActivation = false;
        Debug.Log("SceneManager current count " + SceneManager.sceneCount);
        yield return new WaitForEndOfFrame();
        
        Debug.Log("重新设置shader----------------------------------");

#if UNITY_EDITOR
        GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var go in gameObjects)
        {
            Image[] images = go.GetComponentsInChildren<Image>(true);
            for (int j = 0; j < images.Length; j++)
            {
                images[j].material.shader = Shader.Find(images[j].material.shader.name);
            }

            Renderer[] meshSkinRenderer = go.GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j< meshSkinRenderer.Length; j++)
            {   if (meshSkinRenderer[j].sharedMaterial == null)
                {
                    continue;
                }
                meshSkinRenderer[j].sharedMaterial.shader = Shader.Find(meshSkinRenderer[j].sharedMaterial.shader.name);
            }
        }
        
        // 天空盒shader
        RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
#endif
    }
    
}
