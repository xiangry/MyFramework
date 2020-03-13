using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sword;
using UnityEngine;
using XLua;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Util
{
    [LuaCallCSharp()]
    [Hotfix()]
    public class UtiltyLuaCallCS
    {
        #region gameObject

        

        #endregion

        #region Animator
        public static float GetClipLength(Animator animator, string clip)
        {
            if (null != animator && ! string.IsNullOrEmpty(clip) && null != animator.runtimeAnimatorController)
            {
                RuntimeAnimatorController ac = animator.runtimeAnimatorController;
                AnimationClip[] tAnimationClips = ac.animationClips;
                if( null == tAnimationClips || tAnimationClips.Length <= 0) return 0;
                AnimationClip tAnimationClip ;
                for (int tCounter = 0 ,tLen = tAnimationClips.Length; tCounter < tLen ; tCounter ++) 
                {
                    tAnimationClip = ac.animationClips[tCounter];
                    if(null != tAnimationClip && tAnimationClip.name == clip)
                        return tAnimationClip.length;
                }
            }
            else
            {
                Logger.LogError("GetClipLength not found " + clip);
            }     
            return 0F;
        }
        #endregion
        
        #region touch
        
        #region Touch
        public static TouchLayerLua CreateTouchLayer(int key, LuaTable module)
        {
            return new TouchLayerLua(key, module);
        }
        public static void AddTouchLayer(TouchLayerLua layer)
        {
            TouchManager.instance.AddLayer(layer.Layer);
        }
        public static void RemoveTouchLayer(TouchLayerLua layer)
        {
            TouchManager.instance.RemoveLayer(layer.Layer);
        }
        #endregion

        public static void AddCameraToEasyTouch(GameObject cameraGo)
        {
            if (cameraGo)
            {
                var camera = cameraGo.transform.GetComponent<Camera>();
                if (camera)
                {
                    var  sourceCam = HedgehogTeam.EasyTouch.EasyTouch.GetCamera();
                    HedgehogTeam.EasyTouch.EasyTouch.AddCamera(camera);
                }
            }
        }

        public static void RemoveCameraFromEasyTouch(GameObject cameraGo)
        {
            if (cameraGo)
            {
                var camera = cameraGo.transform.GetComponent<Camera>();
                if (camera) HedgehogTeam.EasyTouch.EasyTouch.RemoveCamera(camera);
            }
        }
        
        #endregion

        [Hotfix()]
        [LuaCallCSharp()]
        public static void ResetParticleSortOrder(GameObject obj, int baseOrder)
        { 
            ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>(true);
            Renderer renderer;
            foreach (var particle in particles)
            {
                renderer = particle.GetComponent<Renderer>();
                if (renderer)
                {
                    renderer.sortingOrder += baseOrder;
                }
            }
        }

        [Hotfix()]
        [LuaCallCSharp()]
        public static RaycastHit ScenePointToRayObj(Vector2 pos)
        {
            GameObject go;
//            go.GetComponent<BoxCollider>();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            return hit;
        }

        #region 位置转换
        public static Vector2 ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out localPoint);
            return localPoint;
        }
        #endregion
        
        
        [Hotfix()]
        [LuaCallCSharp()]
        public static bool IsRayCastPass(Vector3 pos, float x, float y, float z, int layerMask = -5)
        {
            layerMask = 1 << 15;
            bool hit1 = false;
            if (x > Mathf.Epsilon)
            {
                hit1 = Physics.Raycast(pos, Vector3.right, x, layerMask);
            }

            if (!hit1 && x < -Mathf.Epsilon)
            {
                hit1 = Physics.Raycast(pos, Vector3.left, -x, layerMask);    
            }
            
            if (!hit1 && y > Mathf.Epsilon || y < -Mathf.Epsilon)
            {
                hit1 = Physics.Raycast(pos, Vector3.up, y, layerMask);
            }
            
            if (!hit1 && y < -Mathf.Epsilon)
            {
                hit1 = Physics.Raycast(pos, Vector3.down, -y, layerMask);    
            }
            
            if (!hit1 && z > Mathf.Epsilon || z < -Mathf.Epsilon)
            {
                hit1 = Physics.Raycast(pos, Vector3.forward, z, layerMask);
            }
            
            if (!hit1 && z < -Mathf.Epsilon)
            {
                hit1 = Physics.Raycast(pos, Vector3.back, -z, layerMask);    
            }

            return hit1;
        }


        [Hotfix()]
        [LuaCallCSharp()]
        public static RaycastHit GetRayCastObj(Vector3 position, Vector3 direction, int layerMask = -5)
        {
            RaycastHit hitInfo;
            Physics.Raycast(position, direction, out hitInfo, float.PositiveInfinity, layerMask);
            return hitInfo;
        }

        #region  材质处理
        [Hotfix()]
        [LuaCallCSharp()]
        public static Material[] AppendMaterial(Renderer render, Material mat)
        {
            mat = Resources.Load<Material>("m_xuruo_body_0002");
            
            Material[] materials = render.materials;
            Material[] newMats = new Material[materials.Length + 1];
            for (int i = 0; i < materials.Length; i++)
            {
                newMats[i] = materials[i];
            }
            newMats[materials.Length] = mat;
            render.materials = newMats;
            return materials;
        }
        
        
        [Hotfix()]
        [LuaCallCSharp()]
        public static void AppendMaterial(GameObject gameObject, Material mat)
        {
            Renderer render = gameObject.GetComponent<Renderer>();
#if UNITY_EDITOR
            if (render == null)
            {
                Debug.LogWarning($"{gameObject.name} 没有render组件");
                return;
            }
#endif

            if (HaveMaterialByName(gameObject, mat.name))
            {
                Debug.LogWarning($"{gameObject.name} Append Material mat:{mat.name}, 但是材质本身已经有了这个材质");
                return;
            }
            
#if UNITY_EDITOR
            mat.shader = Shader.Find(mat.shader.name);
#endif

            Material[] newMats = new Material[render.materials.Length + 1];
            for (int i = 0; i < render.materials.Length; i++)
            {
                newMats[i] = render.materials[i];
            }
            newMats[render.materials.Length] = mat;
            render.materials = newMats;
        }
        
        
        [Hotfix()]
        [LuaCallCSharp()]
        public static bool HaveMaterialByName(GameObject gameObject, string name)
        {
            Renderer render = gameObject.GetComponent<Renderer>();
#if UNITY_EDITOR
            if (render == null)
            {
                Debug.LogWarning($"{gameObject.name} 没有render组件");
                return false;
            }
#endif
            
            int index = -1;
            for (int i = 0; i < render.materials.Length; i++)
            {
                if (render.materials[i].name == name)
                {
                    return true;
                }
            }

            return false;
        }
        
        
        [Hotfix()]
        [LuaCallCSharp()]
        public static void RemoveMaterialByName(GameObject gameObject, string name)
        {
            if (!HaveMaterialByName(gameObject, name))
            {
                Debug.LogWarning($"{gameObject.name} Remove Material mat:{name}, 但是obj没有这个材质");
                return;
            }
            
            Renderer render = gameObject.GetComponent<Renderer>();
            Material[] newMats = new Material[render.materials.Length - 1];
            int index = 0;
            for (int i = 0; i < render.materials.Length; i++)
            {
                if (render.materials[i].name != name)
                {
                    newMats[index] = render.materials[i++];
                }
            }
            render.materials = newMats;
        }
        
        [Hotfix()]
        [LuaCallCSharp()]
        public static void ChangeLayer(GameObject obj, int layer, bool includeChildren = true){
            obj.layer = layer;
            if (includeChildren)
            {
                foreach (Transform t in obj.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = layer;
                }
            }
        }
        
        [Hotfix()]
        [LuaCallCSharp()]
        public static void EditorApplicationPause()
        {
#if UNITY_EDITOR
            EditorApplication.isPaused = true;
#else
            Logger.LogError("非编辑器模式不支持暂停游戏");
#endif
        }
        #endregion
    }
}