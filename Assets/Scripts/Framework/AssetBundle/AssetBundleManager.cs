using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using System.IO;
using System.Text.RegularExpressions;
using KV;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using PostProcessAttribute = UnityEngine.Rendering.PostProcessing.PostProcessAttribute;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;

#endif

/// <summary>
/// added by wsh @ 2017-12-21
/// 功能：assetbundle管理类，为外部提供统一的资源加载界面、协调Assetbundle各个子系统的运行
/// 注意：
/// 1、抛弃Resources目录的使用，官方建议：https://unity3d.com/cn/learn/tutorials/temas/best-practices/resources-folder?playlist=30089
/// 2、提供Editor和Simulate模式，前者不适用Assetbundle，直接加载资源，快速开发；后者使用Assetbundle，用本地服务器模拟资源更新
/// 3、场景不进行打包，场景资源打包为预设
/// 4、只提供异步接口，所有加载按异步进行
/// 5、采用LZMA压缩方式，性能瓶颈在Assetbundle加载上，ab加载异步，asset加载同步，ab加载后导出全部asset并卸载ab
/// 6、所有公共ab包（被多个ab包依赖）常驻内存，非公共包加载asset以后立刻卸载，被依赖的公共ab包会随着资源预加载自动加载并常驻内存
/// 7、随意卸载公共ab包可能导致内存资源重复，最好在切换场景时再手动清理不需要的公共ab包
/// 8、常驻包（公共ab包）引用计数不为0时手动清理无效，正在等待加载的所有ab包不能强行终止---一旦发起创建就一定要等操作结束，异步过程进行中清理无效
/// 9、切换场景时最好预加载所有可能使用到的资源，所有加载器用完以后记得Dispose回收，清理GC时注意先释放所有Asset缓存
/// 10、逻辑层所有Asset路径带文件类型后缀，且是AssetBundleConfig.ResourcesFolderName下的相对路径，注意：路径区分大小写
/// TODO：
/// 1、区分场景常驻包和全局公共包，切换场景时自动卸载场景公共包
/// 使用说明：
/// 1、由Asset路径获取AssetName、AssetBundleName：ParseAssetPathToNames
/// 2、设置常驻(公共)ab包：SetAssetBundleResident(assebundleName, true)---公共ab包已经自动设置常驻
/// 2、(预)加载资源：var loader = LoadAssetBundleAsync(assetbundleName)，协程等待加载完毕后Dispose：loader.Dispose()
/// 3、加载Asset资源：var loader = LoadAssetAsync(assetPath, TextAsset)，协程等待加载完毕后Dispose：loader.Dispose()
/// 4、离开场景清理所有Asset缓存：ClearAssetsCache()，UnloadUnusedAssetBundles(), Resources.UnloadUnusedAssets()
/// 5、离开场景清理必要的(公共)ab包：TryUnloadAssetBundle()，注意：这里只是尝试卸载，所有引用计数不为0的包（还正在加载）不会被清理
/// </summary>

namespace AssetBundles
{
    [Hotfix]
    [LuaCallCSharp]
    public class AssetBundleManager : MonoSingleton<AssetBundleManager>
    {
        // 最大同时进行的ab创建数量
        const int MAX_ASSETBUNDLE_CREATE_NUM = 5;
        // manifest：提供依赖关系查找以及hash值比对
        Manifest manifest = null;
        // 资源路径相关的映射表
        AssetsPathMapping assetsPathMapping = null;
        
        //独立资源
        Manifest idpManifest = null;
        AssetsPathMapping idpAssetsPathMapping = null;
        
        // 常驻ab包：需要手动添加公共ab包进来，常驻包不会自动卸载（即使引用计数为0），引用计数为0时可以手动卸载
        HashSet<string> assetbundleResident = new HashSet<string>();
        // ab缓存包：所有目前已经加载的ab包，包括临时ab包与公共ab包
        public  Dictionary<string, AssetBundle> assetbundlesCaching = new Dictionary<string, AssetBundle>();
        // ab缓存包引用计数：卸载ab包时只有引用计数为0时才会真正执行卸载
        Dictionary<string, int> assetbundleRefCount = new Dictionary<string, int>(); 
        // asset缓存：给非公共ab包的asset提供逻辑层的复用
        Dictionary<string, UnityEngine.Object> assetsCaching = new Dictionary<string, UnityEngine.Object>();
        // 加载数据请求：正在prosessing或者等待prosessing的资源请求
        Dictionary<string, ResourceWebRequester> webRequesting = new Dictionary<string, ResourceWebRequester>();
        // 等待处理的资源请求
        Queue<ResourceWebRequester> webRequesterQueue = new Queue<ResourceWebRequester>();
        // 正在处理的资源请求
        List<ResourceWebRequester> prosessingWebRequester = new List<ResourceWebRequester>();
        // 逻辑层正在等待的ab加载异步句柄
        List<AssetBundleAsyncLoader> prosessingAssetBundleAsyncLoader = new List<AssetBundleAsyncLoader>();
        // 逻辑层正在等待的asset加载异步句柄
        List<AssetAsyncLoader> prosessingAssetAsyncLoader = new List<AssetAsyncLoader>();
        // 为了消除GC
        List<string> tmpStringList = new List<string>(8);
        
        public bool IsInitialized { get; set; }

        public static string ManifestBundleName
        {
            get
            {
                return UtilityBuild.ManifestBundleName;
            }
        }
        
        // Hotfix测试---用于侧测试资源模块的热修复
        public void TestHotfix()
        {
#if UNITY_EDITOR || CLIENT_DEBUG
            Logger.Log("********** AssetBundleManager : Call TestHotfix in cs...");
#endif
        }

        protected override void Init()
        {
//            StartCoroutine(Initialize());
        }
        
        public IEnumerator Initialize()
        {
//#if UNITY_EDITOR
//            if (AssetBundleConfig.IsEditorMode)
//            {
//                yield break;
//            }
//#endif

            manifest = new Manifest();
            assetsPathMapping = new AssetsPathMapping();
            // 说明：同时请求资源可以提高加载速度
            var manifestRequest = RequestAssetBundleAsync(manifest.AssetbundleName);
            var pathMapRequest = RequestAssetBundleAsync(assetsPathMapping.AssetbundleName);
            
            yield return manifestRequest;
            var assetbundle = manifestRequest.assetbundle;
            manifest.LoadFromAssetbundle(assetbundle);
            assetbundle.Unload(false);
            manifestRequest.Dispose();

            yield return pathMapRequest;
            assetbundle = pathMapRequest.assetbundle;
            var mapContent = assetbundle.LoadAsset<TextAsset>(assetsPathMapping.AssetName);
            if (mapContent != null)
            {
                assetsPathMapping.Initialize(mapContent.text);
            }
            assetbundle.Unload(true);
            pathMapRequest.Dispose();

            {
                idpManifest = new Manifest();
                idpAssetsPathMapping = new AssetsPathMapping();
                var idpManifestRequest = RequestIndependentAssetBundleAsync(idpManifest.AssetbundleName);
                var idpPathMapRequest = RequestIndependentAssetBundleAsync(idpAssetsPathMapping.AssetbundleName);
            
                yield return idpManifestRequest;
                assetbundle = idpManifestRequest.assetbundle;
                idpManifest.LoadFromAssetbundle(assetbundle);
                assetbundle.Unload(false);
                idpManifestRequest.Dispose();

                yield return idpPathMapRequest;
                assetbundle = idpPathMapRequest.assetbundle;
                mapContent = assetbundle.LoadAsset<TextAsset>(idpAssetsPathMapping.AssetName);
                if (mapContent != null)
                {
                    idpAssetsPathMapping.Initialize(mapContent.text);
                }
                assetbundle.Unload(true);
                idpPathMapRequest.Dispose();
            }
            
            // 设置所有公共包为常驻包
            var start = DateTime.Now;
            
            string path = "";
            if(Core.Resource.FileUtil.FileExist("dependencies_res.txt", ref path))
            {
                byte[] bs = Core.Resource.FileUtil.LoadResourceByWWW(path);
                string res = System.Text.Encoding.Default.GetString (bs);

                    string[] strList2 = res.Split('\n');
                    for (int i = 0; i <= strList2.Length - 1; i++)
                    {
                        string[] strList = strList2[i].Split('\t');
                        if (strList[0] != "")
                        {
                            SetAssetBundleResident(strList[0], true);
                        }

                    }

            }
            else
            {
                Debug.LogError("打开 dependencies_res.txt 路径失败！");
            }
            Debug.Log(string.Format("AssetBundleResident Initialize use {0}ms", (DateTime.Now - start).Milliseconds));
            IsInitialized = true;
            yield break;
        }

        public IEnumerator Cleanup()
        {
//#if UNITY_EDITOR
//            if (AssetBundleConfig.IsEditorMode)
//            {
//                yield break;
//            }
//#endif

            // 等待所有请求完成
            // 要是不等待Unity很多版本都有各种Bug
            yield return new WaitUntil(() =>
            {
                return !IsProsessRunning;
            });

            ClearAssetsCache();
            foreach (var assetbunle in assetbundlesCaching.Values)
            {
                if (assetbunle != null)
                {
                    assetbunle.Unload(false);
                }
            }
            assetbundlesCaching.Clear();
            assetbundleRefCount.Clear();
            assetbundleResident.Clear();
            yield break;
        }

        public Manifest curManifest
        {
            get
            {
                return manifest;
            }
        }

        public string DownloadUrl
        {
            get
            {
                return URLSetting.SERVER_RESOURCE_URL;
            }
        }

        public bool IsProsessRunning
        {
            get
            {
                return prosessingWebRequester.Count != 0 || prosessingAssetBundleAsyncLoader.Count != 0 || prosessingAssetAsyncLoader.Count != 0;
            }
        }

        #region 资源缓存
        public void SetAssetBundleResident(string assetbundleName, bool resident)
        {
//            Logger.Log("SetAssetBundleResident : " + assetbundleName + ", " + resident.ToString());
            bool exist = assetbundleResident.Contains(assetbundleName);
            if (resident && !exist)
            {
                assetbundleResident.Add(assetbundleName);
            }
            else if(!resident && exist)
            {
                assetbundleResident.Remove(assetbundleName);
            }
        }

        public bool IsAssetBundleResident(string assebundleName)
        {
            return assetbundleResident.Contains(assebundleName);
        }

        public bool IsAssetBundleLoaded(string assetbundleName)
        {
            return assetbundlesCaching.ContainsKey(assetbundleName);
        }

        public AssetBundle GetAssetBundleCache(string assetbundleName)
        {
            AssetBundle target = null;
            assetbundlesCaching.TryGetValue(assetbundleName, out target);
            return target;
        }

        protected void RemoveAssetBundleCache(string assetbundleName)
        {
            assetbundlesCaching.Remove(assetbundleName);
        }

        protected void AddAssetBundleCache(string assetbundleName, AssetBundle assetbundle)
        {
            assetbundlesCaching[assetbundleName] = assetbundle;
        }

        public bool IsAssetLoaded(string assetName)
        {
            return assetsCaching.ContainsKey(assetName);
        }

        public UnityEngine.Object GetAssetCache(string assetName)
        {
            UnityEngine.Object target = null;
            assetsCaching.TryGetValue(assetName, out target);
            return target;
        }

        public void AddAssetCache(string assetName, UnityEngine.Object asset)
        {
            assetsCaching[assetName] = asset;
        }

        public void AddAssetbundleAssetsCache(string assetbundleName)
        {
#if UNITY_EDITOR
//            if (AssetBundleConfig.IsEditorMode)
//            {
//                return;
//            }
#endif
            if (!IsAssetBundleLoaded(assetbundleName))
            {
                Logger.LogError("Try to add assets cache from unloaded assetbundle : " + assetbundleName);
                return;
            }
            var curAssetbundle = GetAssetBundleCache(assetbundleName);
            if (curAssetbundle.isStreamedSceneAssetBundle)
            {
//                LoadScene(assetbundleName, curAssetbundle);
                return;
            }
            
            var allAssetNames = assetsPathMapping.GetAllAssetNames(assetbundleName);
            if (allAssetNames.Count == 0)
            {
                allAssetNames = idpAssetsPathMapping.GetAllAssetNames(assetbundleName);
            }

            for (int i = 0; i < allAssetNames.Count; i++)
            {
                var assetName = allAssetNames[i];
                if (IsAssetLoaded(assetName))
                {
                    continue;
                }

                var assetPath = AssetBundleUtility.PackagePathToAssetsPath(assetName);
                UnityEngine.Object asset = null;
                asset = curAssetbundle == null ? null : curAssetbundle.LoadAsset(assetPath);
                AddAssetCache(assetName, asset);
                
                var go = asset as GameObject;
                if (go != null)
                {
#if UNITY_EDITOR
                    ResetEditorShader(go); // 说明：在Editor模拟时，Shader要重新指定
#endif
                    //多语言转换
                    TranslateAsset(go);
                    ResetPostProcess(go);
                }
            }
        }
        #endregion

        /// <summary>
        /// TODO 修改方案，增加效率，添加更多多语言文字支持 luaFunc映射到delegate
        /// </summary>
        /// <param name="go"></param>
        [CSharpCallLua]
        [Hotfix]
        public void TranslateAsset(GameObject go)
        {
            UnityEngine.UI.Text[] texts = go.GetComponentsInChildren<UnityEngine.UI.Text>();
            if (texts.Length > 0)
            {
                LuaFunction func = XLuaManager.Instance.GetTranslateTextFunc();
                if (func != null)
                {
                    foreach (var text in texts)
                    {
                        object[] objs = func.Call(text.text);
                        string tr = objs[0] as string;
                        text.text = tr;
                    }
                }
            }
        }

        //设置PostProcess
        public void ResetPostProcess(GameObject obj)
        {
            PostProcessLayer[] postLayers = obj.GetComponentsInChildren<PostProcessLayer>();
            if (postLayers.Length > 0)
            {
                PostProcessResources postRes = Resources.Load<PostProcessResources>("PostProcessResources");
                foreach (var postLayer in postLayers)
                {
                    postLayer.Init(postRes);
                }
            }
        }

        public void ResetEditorShader(GameObject obj)
        {
            UnityEngine.UI.Image[] images = obj.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            for (int j = 0; j < images.Length; j++)
            {
                int que = images[j].material.renderQueue;
                images[j].material.shader = Shader.Find(images[j].material.shader.name);
                if (images[j].material.renderQueue != que)
                {
                    images[j].material.renderQueue = que;
                }
            }

            Renderer[] meshSkinRenderer = obj.GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < meshSkinRenderer.Length; j++)
            {
                if (meshSkinRenderer[j].sharedMaterial == null)
                {
                    continue;
                }

                int que = 0;
                Material[] materials = meshSkinRenderer[j].sharedMaterials;
                foreach (var material in materials)
                {
                    if (!material.IsNull())
                    {
                        que = material.renderQueue;
                        material.shader = Shader.Find(material.shader.name);
                        if (material.renderQueue != que)
                        {
                            material.renderQueue = que;
                        }
                    }
                }
            }
        }
        
        public void ClearAssetsCache()
        {
            assetsCaching.Clear();
        }
        
        public ResourceWebRequester GetAssetBundleAsyncCreater(string assetbundleName)
        {
            ResourceWebRequester creater = null;
            webRequesting.TryGetValue(assetbundleName, out creater);
            return creater;
        }

        protected int GetReferenceCount(string assetbundleName)
        {
            int count = 0;
            assetbundleRefCount.TryGetValue(assetbundleName, out count);
            return count;
        }

        protected int IncreaseReferenceCount(string assetbundleName)
        {
            int count = 0;
            assetbundleRefCount.TryGetValue(assetbundleName, out count);
            count++;
            assetbundleRefCount[assetbundleName] = count;
            return count;
        }

        protected int DecreaseReferenceCount(string assetbundleName)
        {
            int count = 0;
            assetbundleRefCount.TryGetValue(assetbundleName, out count);
            count--;
            if (count <= 0)
            {
                assetbundleRefCount.Remove(assetbundleName);
            }
            else
            {
                assetbundleRefCount[assetbundleName] = count;
            }
            return count;
        }

        protected bool CreateAssetBundleAsync(string assetbundleName, bool isIndependent = false)
        {
            #if UNITY_EDITOR
//            EditorApplication.isPaused = true;
            #endif
            
            
            if (IsAssetBundleLoaded(assetbundleName) || webRequesting.ContainsKey(assetbundleName))
            {
                return false;
            }

            var creater = ResourceWebRequester.Get();
            var url = AssetBundleUtility.GetAssetBundleFileUrl(assetbundleName);
            if (!manifest.GetAssetBundleHash(assetbundleName).isValid && idpManifest.GetAssetBundleHash(assetbundleName).isValid)
            {
                url = AssetBundleUtility.GetIndependentAssetBundleFileUrl(assetbundleName);
            }
            creater.Init(assetbundleName, url);
            webRequesting.Add(assetbundleName, creater);
            webRequesterQueue.Enqueue(creater);
            // 创建器持有的引用：创建器对每个ab来说是全局唯一的
            IncreaseReferenceCount(assetbundleName);
            return true;
        }

        // 异步请求Assetbundle资源，AB是否缓存取决于是否设置为常驻包，Assets一律缓存，处理依赖
        public BaseAssetBundleAsyncLoader LoadAssetBundleAsync(string assetbundleName, string path = null)
        {
#if UNITY_EDITOR        
            if (AssetBundleConfig.IsEditorMode)
            {
                KVDebugHelper.Instance.AddAssetInfo(path, assetbundleName);
                if (path != null && File.Exists(path))
                {
                    return new EditorAssetBundleAsyncLoader(assetbundleName);
                }
            }
#endif
            bool isIndependent = false;
            var loader = AssetBundleAsyncLoader.Get();
            prosessingAssetBundleAsyncLoader.Add(loader);
//            Debug.Log($"{assetbundleName} hash {manifest.GetAssetBundleHash(assetbundleName)} isValid:{manifest.GetAssetBundleHash(assetbundleName).isValid}");
//            Debug.Log($"{assetbundleName} hash {idpManifest.GetAssetBundleHash(assetbundleName)} isValid:{idpManifest.GetAssetBundleHash(assetbundleName).isValid}");
            if (manifest != null && manifest.GetAssetBundleHash(assetbundleName).isValid)
            {
                string[] dependancies = manifest.GetAllDependencies(assetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    if (!string.IsNullOrEmpty(dependance) && dependance != assetbundleName)
                    {
                        CreateAssetBundleAsync(dependance);
                        // A依赖于B，A对B持有引用
                        IncreaseReferenceCount(dependance);
                    }
                }
                loader.Init(assetbundleName, dependancies);
            }
            else if (idpManifest != null && idpManifest.GetAssetBundleHash(assetbundleName).isValid)
            {
                isIndependent = true;
                string[] dependancies = idpManifest.GetAllDependencies(assetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    if (!string.IsNullOrEmpty(dependance) && dependance != assetbundleName)
                    {
                        CreateAssetBundleAsync(dependance);
                        // A依赖于B，A对B持有引用
                        IncreaseReferenceCount(dependance);
                    }
                }
                loader.Init(assetbundleName, dependancies);
            }
            else
            {
                loader.Init(assetbundleName, null);
            }
            CreateAssetBundleAsync(assetbundleName, isIndependent);
            // 加载器持有的引用：同一个ab能同时存在多个加载器，等待ab创建器完成
            IncreaseReferenceCount(assetbundleName);
            return loader;
        }

        // 从服务器下载网页内容，需提供完整url，非AB（不计引用计数、不缓存），Creater使用后记得回收
        public ResourceWebRequester DownloadWebResourceAsync(string url)
        {
            var creater = ResourceWebRequester.Get();
            creater.Init(url, url, true);
            webRequesting.Add(url, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }

        // 从资源服务器下载非Assetbundle资源，非AB（不计引用计数、不缓存），Creater使用后记得回收
        public ResourceWebRequester DownloadAssetFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(DownloadUrl))
            {
                Logger.LogError("You should set download url first!!!");
                return null;
            }

            var creater = ResourceWebRequester.Get();
            var url = DownloadUrl + filePath;
            creater.Init(filePath, url, true);
            webRequesting.Add(filePath, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }

        // 从资源服务器下载Assetbundle资源，非AB（不计引用计数、不缓存），Creater使用后记得回收
        public ResourceWebRequester DownloadAssetBundleAsync(string filePath)
        {
            // 如果ResourceWebRequester升级到使用UnityWebRequester，那么下载AB和下载普通资源需要两个不同的DownLoadHandler
            // 兼容升级的可能性，这里也做一下区分
            return DownloadAssetFileAsync(filePath);
        }

        // 本地异步请求非Assetbundle资源，非AB（不计引用计数、不缓存），Creater使用后记得回收
        public ResourceWebRequester RequestAssetFileAsync(string filePath, bool streamingAssetsOnly = true)
        {
            var creater = ResourceWebRequester.Get();
            string url = null;
            if (streamingAssetsOnly)
            {
                url = AssetBundleUtility.GetStreamingAssetsFilePath(filePath);
            }
            else
            {
                url = AssetBundleUtility.GetAssetBundleFileUrl(filePath);
            }
            creater.Init(filePath, url, true);
            webRequesting.Add(filePath, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }

        // 本地异步请求Assetbundle资源，不计引用计数、不缓存，Creater使用后记得回收
        public ResourceWebRequester RequestAssetBundleAsync(string assetbundleName)
        {
            var creater = ResourceWebRequester.Get();
            var url = AssetBundleUtility.GetAssetBundleFileUrl(assetbundleName);
            creater.Init(assetbundleName, url, true);
            webRequesting.Add(assetbundleName, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }
        
        //TODO 临时资源，后期必须要修改
        // 本地异步请求Assetbundle资源，不计引用计数、不缓存，Creater使用后记得回收
        public ResourceWebRequester RequestIndependentAssetBundleAsync(string assetbundleName)
        {
            var creater = ResourceWebRequester.Get();
            var url = AssetBundleUtility.GetIndependentAssetBundleFileUrl(assetbundleName);
            creater.Init(assetbundleName, url, true);
            webRequesting.Add(assetbundleName, creater);
            webRequesterQueue.Enqueue(creater);
            return creater;
        }

        
        protected bool UnloadAssetBundle(string assetbundleName, bool unloadResident = false, bool unloadAllLoadedObjects = false, bool unloadDependencies = true)
        {
            int count = GetReferenceCount(assetbundleName);
            if (count > 0)
            {
                // 存在引用，还是被需要的，不能卸载
                return false;
            }

            var assetbundle = GetAssetBundleCache(assetbundleName);
            var isResident = IsAssetBundleResident(assetbundleName);
            if (!isResident || (isResident && unloadResident))
            {
                if (assetbundle != null)
                {
                    assetbundle.Unload(unloadAllLoadedObjects);
                }
                
                RemoveAssetBundleCache(assetbundleName);
                if (unloadDependencies && manifest != null)
                {
                    string[] dependancies = manifest.GetAllDependencies(assetbundleName);
                    for (int i = 0; i < dependancies.Length; i++)
                    {
                        var dependance = dependancies[i];
                        if (!string.IsNullOrEmpty(dependance) && dependance != assetbundleName)
                        {
                            // 解除对依赖项持有的引用
                            int dependanceCount = DecreaseReferenceCount(dependance);
                            if (dependanceCount <= 0)
                            {
                                UnloadAssetBundle(dependance, unloadResident, unloadAllLoadedObjects, false);
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        // 用于卸载无用AB包：如果该AB包还在使用，则卸载失败
        public bool UnloadUnusedAssetBundle(string assetbundleName, bool unloadAllLoadedObjects = false, bool unloadDependencies = true)
        {
            int count = GetReferenceCount(assetbundleName);
            if (count > 0)
            {
                return false;
            }

            // 按照目前的设计，只能卸载常驻AB包
            //Logger.Assert(IsAssetBundleResident(assetbundleName) == true, "Only resident abs can exist with ref count 0 !!!");
            return UnloadAssetBundle(assetbundleName, true, unloadAllLoadedObjects, unloadDependencies);
        }

        // 用于卸载所有无用AB包：如果该AB包还在使用，则卸载失败
        public int UnloadAllUnusedResidentAssetBundles(bool unloadAllLoadedObjects = false, bool unloadDependencies = true)
        {
            int unloadCount = 0;
            bool hasDoUnload = false;
            do
            {
                hasDoUnload = false;
                tmpStringList.Clear();
                var iter = assetbundleRefCount.GetEnumerator();
                while (iter.MoveNext())
                {
                    var assetbundleName = iter.Current.Key;
                    var referenceCount = iter.Current.Value;
                    if (referenceCount <= 0)
                    {
                        // 按照目前的设计，只能卸载常驻AB包
                        Logger.Assert(IsAssetBundleResident(assetbundleName) == true, "Only resident abs can exist with ref count 0 !!!");
                        tmpStringList.Add(assetbundleName);
                    }
                }
                for (int i = 0; i < tmpStringList.Count; i++)
                {
                    string toRemoveName = tmpStringList[i];
                    var result = UnloadAssetBundle(toRemoveName, true, unloadAllLoadedObjects, unloadDependencies);
                    if (result)
                    {
                        unloadCount++;
                        hasDoUnload = true;
                    }
                }
                tmpStringList.Clear();
            } while (hasDoUnload);
            return unloadCount;
        }

        public bool MapAssetPath(string assetPath, out string assetbundleName, out string assetName)
        {
            try
            {
                bool flag = assetsPathMapping.MapAssetPath(assetPath, out assetbundleName, out assetName);
                if(!flag)
                {
                    flag = idpAssetsPathMapping.MapAssetPath(assetPath, out assetbundleName, out assetName);
                }
                return flag;
            }
            catch (Exception e)
            {
                assetbundleName = null;
                assetName = null;
                Logger.LogError(">>MapAssetPath Exception: {0}", e.Message);
                return false;
            }
        }
        
        public BaseAssetAsyncLoader LoadAssetAsync(string assetPath, System.Type assetType)
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                string path = AssetBundleUtility.PackagePathToAssetsPath(assetPath);
                KVDebugHelper.Instance.AddAssetInfo(assetPath, path);
                if (File.Exists(path))
                {
                    UnityEngine.Object target = AssetDatabase.LoadAssetAtPath(path, assetType);
                    return new EditorAssetAsyncLoader(target);
                }
            }
#endif

            string assetbundleName = null;
            string assetName = null;
            MapAssetPath(assetPath, out assetbundleName, out assetName);
            KVDebugHelper.Instance.AddAssetInfo(assetPath, assetbundleName);
            if (assetPath.IndexOf("spine") >= 0)
            {
                Debug.LogError($"不应该加载这个资源 assetPath:{assetPath}");
            }
            if (assetName == null)
            {
                Debug.LogError($"没有这个资源 assetPath:{assetPath}");
                return null;
            }
            
            var loader = AssetAsyncLoader.Get();
            prosessingAssetAsyncLoader.Add(loader);
            if (IsAssetLoaded(assetName))
            {
                //Logger.LogError(">>>> IsAssetLoaded {0}", assetName);
                loader.Init(assetName, GetAssetCache(assetName));
                return loader;
            }

            var assetbundleLoader = LoadAssetBundleAsync(assetbundleName, assetPath);
            loader.Init(assetName, assetbundleLoader);
            return loader;
        }
        
        void Update()
        {
            OnProsessingWebRequester();
            OnProsessingAssetBundleAsyncLoader();
            OnProsessingAssetAsyncLoader();
        }

        void OnProsessingWebRequester()
        {
            for (int i = prosessingWebRequester.Count - 1; i >= 0; i--)
            {
                var creater = prosessingWebRequester[i];
                creater.Update();
                if (creater.IsDone())
                {
                    prosessingWebRequester.RemoveAt(i);
                    webRequesting.Remove(creater.assetbundleName);
                    if (creater.noCache)
                    {
                        // 无缓存，不计引用计数、Creater使用后由上层回收，所以这里不需要做任何处理
                    }
                    else
                    {
                        // AB缓存
                        // 说明：有错误也缓存下来，只不过资源为空
                        // 1、避免再次错误加载
                        // 2、如果不存下来加载器将无法判断什么时候结束
                        AddAssetBundleCache(creater.assetbundleName, creater.assetbundle);

                        // 解除创建器对AB持有的引用，一般创建器存在，则一定至少有一个加载器在等待并对该AB持有引用
                        int count = DecreaseReferenceCount(creater.assetbundleName);
                        Logger.Assert(count > 0, "AssetBundle creater done but no one need it!!!");
                        if (count <= 0)
                        {
                            UnloadAssetBundle(creater.assetbundleName);
                        }
                        creater.Dispose();
                    }
                }
            }
            int slotCount = prosessingWebRequester.Count;
            while (slotCount < MAX_ASSETBUNDLE_CREATE_NUM && webRequesterQueue.Count > 0)
            {
                var creater = webRequesterQueue.Dequeue();
                creater.Start();
                prosessingWebRequester.Add(creater);
                slotCount++;
            }
        }
        
        void OnProsessingAssetBundleAsyncLoader()
        {
            for (int i = prosessingAssetBundleAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingAssetBundleAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    // 解除加载器对AB持有的引用
                    int count = DecreaseReferenceCount(loader.assetbundleName);
                    if (count <= 0)
                    {
                        UnloadAssetBundle(loader.assetbundleName);
                    }
                    prosessingAssetBundleAsyncLoader.RemoveAt(i);
                }
            }
        }

        void OnProsessingAssetAsyncLoader()
        {
            for (int i = prosessingAssetAsyncLoader.Count - 1; i >= 0; i--)
            {
                var loader = prosessingAssetAsyncLoader[i];
                loader.Update();
                if (loader.IsDone())
                {
                    prosessingAssetAsyncLoader.RemoveAt(i);
                }
            }
        }

#if UNITY_EDITOR
        [BlackList]
        public HashSet<string> GetAssetbundleResident()
        {
            return assetbundleResident;
        }

        [BlackList]
        public ICollection<string> GetAssetbundleCaching()
        {
            return assetbundlesCaching.Keys;
        }

        [BlackList]
        public Dictionary<string, ResourceWebRequester> GetWebRequesting()
        {
            return webRequesting;
        }

        [BlackList]
        public Queue<ResourceWebRequester> GetWebRequestQueue()
        {
            return webRequesterQueue;
        }

        [BlackList]
        public List<ResourceWebRequester> GetProsessingWebRequester()
        {
            return prosessingWebRequester;
        }

        [BlackList]
        public List<AssetBundleAsyncLoader> GetProsessingAssetBundleAsyncLoader()
        {
            return prosessingAssetBundleAsyncLoader;
        }

        [BlackList]
        public List<AssetAsyncLoader> GetProsessingAssetAsyncLoader()
        {
            return prosessingAssetAsyncLoader;
        }

        [BlackList]
        public string GetAssetBundleName(string assetName)
        {
            return assetsPathMapping.GetAssetBundleName(assetName);
        }

        [BlackList]
        public int GetAssetCachingCount()
        {
            return assetsCaching.Count;
        }

        [BlackList]
        public Dictionary<string, List<string>> GetAssetCaching()
        {
            var assetbundleDic = new Dictionary<string, List<string>>();
            List<string> assetNameList = null;
            
            var iter = assetsCaching.GetEnumerator();
            while (iter.MoveNext())
            {
                var assetName = iter.Current.Key;
                var assetbundleName = assetsPathMapping.GetAssetBundleName(assetName);
                assetbundleDic.TryGetValue(assetbundleName, out assetNameList);
                if (assetNameList == null)
                {
                    assetNameList = new List<string>();
                }
                assetNameList.Add(assetName);
                assetbundleDic[assetbundleName] = assetNameList;
            }
            return assetbundleDic;
        }

        [BlackList]
        public int GetAssetbundleRefrenceCount(string assetbundleName)
        {
            return GetReferenceCount(assetbundleName);
        }

        [BlackList]
        public int GetAssetbundleDependenciesCount(string assetbundleName)
        {
            string[] dependancies = manifest.GetAllDependencies(assetbundleName);
            int count = 0;
            for (int i = 0; i < dependancies.Length; i++)
            {
                var cur = dependancies[i];
                if (!string.IsNullOrEmpty(cur) && cur != assetbundleName)
                {
                    count++;
                }
            }
            return count;
        }

        [BlackList]
        public List<string> GetAssetBundleRefrences(string assetbundleName)
        {
            List<string> refrences = new List<string>();
            var cachingIter = assetbundlesCaching.GetEnumerator();
            while (cachingIter.MoveNext())
            {
                var curAssetbundleName = cachingIter.Current.Key;
                if (curAssetbundleName == assetbundleName)
                {
                    continue;
                }
                string[] dependancies = manifest.GetAllDependencies(curAssetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    if (dependance == assetbundleName)
                    {
                        refrences.Add(curAssetbundleName);
                    }
                }
            }

            var requestingIter = webRequesting.GetEnumerator();
            while (requestingIter.MoveNext())
            {
                var curAssetbundleName = requestingIter.Current.Key;
                if (curAssetbundleName == assetbundleName)
                {
                    continue;
                }

                string[] dependancies = manifest.GetAllDependencies(curAssetbundleName);
                for (int i = 0; i < dependancies.Length; i++)
                {
                    var dependance = dependancies[i];
                    if (dependance == assetbundleName)
                    {
                        refrences.Add(curAssetbundleName);
                    }
                }
            }
            return refrences;
        }

        [BlackList]
        public List<string> GetWebRequesterRefrences(string assetbundleName)
        {
            List<string> refrences = new List<string>();
            var iter = webRequesting.GetEnumerator();
            while (iter.MoveNext())
            {
                var curAssetbundleName = iter.Current.Key;
                var webRequster = iter.Current.Value;
                if (curAssetbundleName == assetbundleName)
                {
                    refrences.Add(webRequster.Sequence.ToString());
                    continue;
                }
            }
            return refrences;
        }

        [BlackList]
        public List<string> GetAssetBundleLoaderRefrences(string assetbundleName)
        {
            List<string> refrences = new List<string>();
            var iter = prosessingAssetBundleAsyncLoader.GetEnumerator();
            while (iter.MoveNext())
            {
                var curAssetbundleName = iter.Current.assetbundleName;
                var curLoader = iter.Current;
                if (curAssetbundleName == assetbundleName)
                {
                    refrences.Add(curLoader.Sequence.ToString());
                }
            }
            return refrences;
        }
#endif
        
//        [LuaCallCSharp()]
//        public void CoLoadScene(string path, Action action)
//        {
//           StartCoroutine(LoadScene(path, action));
//        }
//
//        //这里直接加载AB包形式的场景
//        private IEnumerator LoadScene(string assetPath, Action action)
//        {
//            string assetbundleName = null;
//            string assetName = null;
//            MapAssetPath(assetPath, out assetbundleName, out assetName);
//            
//            string[] dependancies = idpManifest.GetAllDependencies(assetbundleName);
//            for (int i = 0; i < dependancies.Length; i++)
//            {
//                var dependance = dependancies[i];
//                if (!string.IsNullOrEmpty(dependance) && dependance != assetbundleName)
//                {
//                    string url = AssetBundleUtility.GetIndependentAssetBundleFileUrl(dependance);
//                    UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
//                    yield return request.SendWebRequest();
//                    if (request.isHttpError || request.isNetworkError)
//                    {
//                        Debug.Log(request.error);
//                    }
//                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
//                    AddAssetBundleCache(assetbundleName, bundle);
//                }
//            }
//
//            {
//                string url = AssetBundleUtility.GetIndependentAssetBundleFileUrl(assetbundleName);
//                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
//                yield return request.SendWebRequest();
//                if (request.isHttpError || request.isNetworkError)
//                {
//                    Debug.Log(request.error);
//                }
//                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
//                AddAssetBundleCache(assetbundleName, bundle);
//                action();
//            }
//        }
    }
}