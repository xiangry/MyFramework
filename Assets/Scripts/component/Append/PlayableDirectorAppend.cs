using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using XLua;

[Hotfix()]
[LuaCallCSharp()]
public class PlayableDirectorAppend : MonoBehaviour
{
    public PlayableDirector playableDirector = null;
    
    private readonly Dictionary<string, PlayableBinding> bindingDict = new Dictionary<string, PlayableBinding>();
    
    // Start is called before the first frame update
    void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
        
        foreach (var bind in playableDirector.playableAsset.outputs)
        {
            if (!bindingDict.ContainsKey(bind.streamName))
            {
                bindingDict.Add(bind.streamName, bind);
            }
        }
    }

    /// <summary>
    /// 动态设置轨道
    /// </summary>
    /// <param name="trackName"></param>
    /// <param name="gameObject"></param>
    public void SetTrackDynamic(string trackName, GameObject gameObject)
    {
        if (bindingDict.TryGetValue(trackName, out PlayableBinding pb))
        {
            playableDirector.SetGenericBinding(pb.sourceObject, gameObject);
        }
    }
}
