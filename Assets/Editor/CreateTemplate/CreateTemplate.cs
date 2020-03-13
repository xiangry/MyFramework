using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CreateTemplate
{
    
    //创建模版UI方法（暂时未完成）
    [UnityEditor.MenuItem("SotFont/CreateUITemplate", false, 1)]
    public static void CReateUITemplate()
    {
        
        
        
    }


    private static void CreateFloder(string name, string path)
    {
        string fullpath = path + "/";
        Directory.CreateDirectory(fullpath + name);
    }


}
