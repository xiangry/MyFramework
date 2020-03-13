using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class ProjectorShadow: MonoBehaviour
{
	public		float Size;
	public		float							Depth;
	public		LayerMask						CameraLayerMask;
	public		LayerMask						ProjectorLayerMask;
	public		Color							ShadowColor = Color.black;
    public      int                             textureSize = 1024;
    public      Vector3                         ShadowDir = new Vector3(0,-1,0);
    public      Vector3                         rayDirOffset = new Vector3(0, 0, 0);
    public      Transform rayTarget;
	private		Camera							fCamera;
	private		Projector						fProjector;
    public      float rayDistance = 30f;
	private		RenderTexture					fRT;
	private		Material						fMaterial;
    private Camera pCamera;
    public bool forceOn = false;
    void OnEnable()
    {
        if (forceOn == false)
        {
            //tag = "HighEffect";
            if (QualitySettings.GetQualityLevel() < 2)
            {
                enabled = false;
                fCamera = GetComponent<Camera>();
                if (fCamera != null)
                {
                    fRT = fCamera.targetTexture;
                    if (fRT != null)
                    {
                        fCamera.targetTexture = null;
                        fRT.Release();
                        fRT = null;
                    }
                    fCamera.enabled = false;
                }
                fProjector = GetComponent<Projector>();
                if (fProjector != null)
                {
                    DestroyImmediate(fProjector);
                }
                return;
            }
        }
        fCamera = GetComponent<Camera>();
        Shader rs = Shader.Find("DyShader/Shadow");
        fCamera.SetReplacementShader(rs, "RenderType");
        pCamera = transform.parent.GetComponent<Camera>();
        fRT = fCamera.targetTexture;
        if (fRT != null)
        {
            fCamera.targetTexture = null;
            fRT.Release();
        }
        if (fCamera != null)
        {
            fCamera.enabled = true;
            fCamera.clearFlags = CameraClearFlags.SolidColor;
            fCamera.backgroundColor = new Color(0, 0, 0, 0);
            fCamera.cullingMask = this.CameraLayerMask;
            fCamera.orthographic = true;
            fCamera.orthographicSize = this.Size;
            fCamera.nearClipPlane = 0.01f;
            fCamera.farClipPlane = this.Depth;
            fCamera.rect = new Rect(0, 0, 1, 1);
            fCamera.depth = -10;
            fCamera.renderingPath = RenderingPath.Forward;
            fCamera.useOcclusionCulling = false;
            fCamera.allowHDR = false;
        }
        fProjector = GetComponent<Projector>();
        if (fProjector == null)
        {
            fProjector = gameObject.AddComponent<Projector>();
        }
        fProjector.orthographic = true;
        fProjector.orthographicSize = this.Size;
        fProjector.nearClipPlane = 0.01f;
        fProjector.farClipPlane = this.Depth;
        fProjector.aspectRatio = 1;
        fProjector.ignoreLayers = ~this.ProjectorLayerMask;
        fMaterial = new Material(Shader.Find("Projector/Shadow"));
        fProjector.material = fMaterial;
        fRT = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        fRT.wrapMode = TextureWrapMode.Clamp;
        fRT.filterMode = FilterMode.Bilinear;
        fRT.anisoLevel = 0;
        fCamera.targetTexture = fRT;
        fMaterial.SetColor("_Color", ShadowColor);
        fMaterial.SetTexture("_MainTex", fRT);
    }

    void OnDisable()
    {
        fCamera = GetComponent<Camera>();
        if (fCamera != null)
        {
            fRT = fCamera.targetTexture;
            if (fRT != null)
            {
                fCamera.targetTexture = null;
                fRT.Release();
                fRT = null;
            }
            fCamera.enabled = false;
        }
    }
	public void Update()
	{
#if UNITY_EDITOR
		this.SetPara();
#endif
	}
    Ray ray = new Ray();
    public void LateUpdate()
    {
        if (pCamera != null)
        {
            RaycastHit[] hits;
            ray.origin = pCamera.transform.position;
            if (rayTarget != null)
            {
                ray.direction = rayTarget.position - ray.origin + rayDirOffset;
            }
            else
            {
                ray.direction = pCamera.transform.forward + rayDirOffset;
            }
            hits = Physics.RaycastAll(ray, rayDistance, ProjectorLayerMask.value);
#if UNITY_EDITOR
            Debug.DrawLine(ray.origin, ray.origin + rayDistance * ray.direction.normalized, Color.red);
#endif
            bool isfindterrain = false;
            if (hits.Length > 0)
            {
                RaycastHit hit = hits[0];
                Transform t = hit.transform;
                transform.forward = ShadowDir;
                transform.position = hit.point - Vector3.Normalize(ShadowDir) * (Depth - 10);
#if UNITY_EDITOR
                Debug.DrawLine(hit.point, hit.point + Vector3.forward, Color.red);
                Debug.DrawLine(hit.point, hit.point - Vector3.forward, Color.red);
                Debug.DrawLine(hit.point, hit.point + Vector3.left, Color.red);
                Debug.DrawLine(hit.point, hit.point - Vector3.left, Color.red);
                Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.red);
                Debug.DrawLine(hit.point, hit.point - Vector3.up, Color.red);
#endif
                isfindterrain = true;
            }
            if (!isfindterrain)
            {
                ray.origin = pCamera.transform.position;
                if (rayTarget != null)
                {
                    ray.direction = rayTarget.position - ray.origin - pCamera.transform.up * 0.1f + rayDirOffset;
                }
                else
                {
                    ray.direction = pCamera.transform.forward - pCamera.transform.up * 0.1f + rayDirOffset;
                }
                hits = Physics.RaycastAll(ray, rayDistance, ProjectorLayerMask.value);
#if UNITY_EDITOR
                Debug.DrawLine(pCamera.transform.position, pCamera.transform.position + rayDistance * ray.direction.normalized, Color.green);
#endif
                if(hits.Length > 0)
                { 
                    RaycastHit hit = hits[0];
                    Transform t = hit.transform;
                    transform.forward = ShadowDir;
                    transform.position = hit.point - Vector3.Normalize(ShadowDir) * (Depth - 10);
#if UNITY_EDITOR
                    Debug.DrawLine(hit.point, hit.point + Vector3.forward, Color.blue);
                    Debug.DrawLine(hit.point, hit.point - Vector3.forward, Color.blue);
                    Debug.DrawLine(hit.point, hit.point + Vector3.left, Color.blue);
                    Debug.DrawLine(hit.point, hit.point - Vector3.left, Color.blue);
                    Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.blue);
                    Debug.DrawLine(hit.point, hit.point - Vector3.up, Color.blue);
#endif
                    isfindterrain = true;
                }
            }
        }
    }

	public void SetPara()
	{
		this.fCamera							=	this.GetComponent<Camera>();
        if (fCamera)
        {
            this.fCamera.clearFlags = CameraClearFlags.SolidColor;
            this.fCamera.backgroundColor = new Color(0, 0, 0, 0);
            this.fCamera.cullingMask = this.CameraLayerMask;
            this.fCamera.orthographic = true;
            this.fCamera.orthographicSize = this.Size;
            this.fCamera.nearClipPlane = 0.01f;
            this.fCamera.farClipPlane = this.Depth;
            this.fCamera.rect = new Rect(0, 0, 1, 1);
            this.fCamera.depth = -10;
            this.fCamera.renderingPath = RenderingPath.Forward;
            this.fCamera.useOcclusionCulling = false;
            this.fCamera.allowHDR = false;
            this.fRT = this.fCamera.targetTexture;
            if (fRT != null)
            {
                this.fRT.wrapMode = TextureWrapMode.Clamp;
                this.fRT.filterMode = FilterMode.Bilinear;
                this.fRT.anisoLevel = 0;
            }
            this.fProjector = this.GetComponent<Projector>();
            if (fProjector)
            {
                this.fProjector.orthographic = true;
                this.fProjector.orthographicSize = this.Size;
                this.fProjector.nearClipPlane = 0.01f;
                this.fProjector.farClipPlane = this.Depth;
                this.fProjector.aspectRatio = 1;
                this.fProjector.ignoreLayers = ~this.ProjectorLayerMask;
                this.fMaterial = this.fProjector.material;
                this.fMaterial.SetColor("_Color", this.ShadowColor);
                this.fMaterial.SetTexture("_MainTex", this.fRT);
            }
        }
	}
}