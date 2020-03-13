using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KVAutoTextureSetting : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        //自动设置类型;
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        if (textureImporter.textureType == TextureImporterType.Default)
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.alphaIsTransparency = true;
//            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.textureFormat = TextureImporterFormat.ARGB16;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.isReadable = false;
            textureImporter.mipmapEnabled = false;
            textureImporter.maxTextureSize = 2048;
//            Debug.LogError("Auto Preprocess Texture ---- ");
        }
    }
}
