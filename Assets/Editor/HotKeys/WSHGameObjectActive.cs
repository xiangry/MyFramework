using UnityEngine;
//引入unity编辑器命名空间
using UnityEditor;
using System.Collections;
 
public class WSHGameObjectActive : ScriptableObject {
 
	public const string KeyName = "WSH/DisableSelectGameObect &d";
 
	//根据当前有没有选中物体来判断可否用快捷键
	[MenuItem(KeyName, true)]
	static bool ValidateSelectEnableDisable()
	{
		GameObject[] go = GetSelectedGameObjects() as GameObject[];
 
		if (go == null || go.Length == 0)
			return false;
		return true;
    }
 
	[MenuItem(KeyName)]
	static void SeletEnable()
	{
		bool enable = false;
        GameObject[] gos = GetSelectedGameObjects() as GameObject[];
 
		foreach (GameObject go in gos)
		{
			enable = !go.activeInHierarchy;
			EnableGameObject(go,enable);
        }
	}
 
	//获得选中的物体
	static GameObject[] GetSelectedGameObjects()
	{
		return Selection.gameObjects;
	}
 
	//激活或关闭当前选中物体
	public static void EnableGameObject(GameObject parent, bool enable)
	{
		parent.gameObject.SetActive(enable);
	}
}
