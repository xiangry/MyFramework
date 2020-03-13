using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadingTest : MonoBehaviour
{
    private Texture2D texture2D;
    private int width;
    private int height;

    public RawImage rawImage;
    
    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        texture2D = new Texture2D(width, height);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.LogError("press space ------");
            StartCoroutine(TestShowImage());
        }
    }

    IEnumerator  TestShowImage()
    {
        yield return new WaitForEndOfFrame();
        texture2D.ReadPixels(new Rect(0,0, width, height), 0, 0, true);
        texture2D.Apply();
        rawImage.texture = texture2D;
        Debug.LogError("set raw image texture ------");
        byte[] bytes = texture2D.EncodeToPNG();
        string path = Application.streamingAssetsPath + "/test.png";
        File.WriteAllBytes(path, bytes);
        Debug.LogError($"save texture ------{path}");

            yield return null;
    }
}
