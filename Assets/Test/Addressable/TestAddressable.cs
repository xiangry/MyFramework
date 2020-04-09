using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TestAddressable : MonoBehaviour
{
    private Canvas uiRoot;

    private AsyncOperationHandle<GameObject> handle;
    
    // Start is called before the first frame update
    void Start()
    {
        uiRoot = GameObject.Find("Canvas").GetComponent<Canvas>();
        
        DontDestroyOnLoad(gameObject);
        
        DoTest();
    }

    void DoTest()
    {
        Debug.Log($"DoTest -------------");
        handle = Addressables.LoadAssetAsync<GameObject>("test");
        handle.Completed += OnResLoadedHandler;
    }

    void OnResLoadedHandler(AsyncOperationHandle<GameObject> obj)
    {
        Debug.Log($"OnResLoadedHandler Status:{obj.Status} Result:{obj.Result}");
        if (obj.Result)
        {
            GameObject go = obj.Result;
            GameObject uiloading = Instantiate(go, uiRoot.transform);
        }

    }
}
