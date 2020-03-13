//
// Copyright 2012 Thinksquirrel Software, LLC. All rights reserved.
//
using UnityEngine;
using System.Collections;

[AddComponentMenu("")]
public class CameraShakeExample : MonoBehaviour {
	
	#region Private variables
	private CameraShake shake;
	private bool shakeGUI;
	private bool shake1;
	private bool shake2;
	private bool multiShake;
	#endregion
	
	#region MonoBehaviour methods
	void Start()
	{
		shake = CameraShake.instance;
	}
	void OnGUI()
	{
		if (shake)
		{
			DrawGUIArea1();
			DrawGUIArea2();
		}
	}
	#endregion
	
	#region Cancel shake button
	void DrawGUIArea1()
	{
		GUI.enabled = (CameraShake.isShaking || multiShake) && !CameraShake.isCancelling;
		
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		if (GUILayout.Button("Cancel Shake"))
		{
			this.StopAllCoroutines();
			CameraShake.CancelShake(0.5f);
			shake1 = false;
			shake2 = false;
			multiShake = false;
		}
		GUILayout.EndHorizontal();
		
		GUI.enabled = true;
	}
	#endregion
	
	#region Main interface
	void DrawGUIArea2()
	{
		if (shakeGUI)
		{
			CameraShake.BeginShakeGUILayout();	
		}
		else
		{
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
		}
		
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Space(100);
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		GUI.enabled = !CameraShake.isShaking && !multiShake;
		
		shake1 = GUILayout.Toggle(shake1, "Shake (without GUI)", GUI.skin.button, GUILayout.Width(200), GUILayout.Height(50));
		shake2 = GUILayout.Toggle(shake2, "Shake (with GUI)", GUI.skin.button, GUILayout.Width(200), GUILayout.Height(50));
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		GUILayout.BeginVertical(GUILayout.Width(300));
		
		GUILayout.Label("Number of Shakes: " + shake.numberOfShakes.ToString());
		
		shake.numberOfShakes = (int)GUILayout.HorizontalSlider(shake.numberOfShakes, 1, 10);
		
		GUILayout.Label("Shake Amount: " + shake.shakeAmount.ToString());
		
		float x, y, z;
		
		x = GUILayout.HorizontalSlider(shake.shakeAmount.x, 0, 10);
		y = GUILayout.HorizontalSlider(shake.shakeAmount.y, 0, 10);
		z = GUILayout.HorizontalSlider(shake.shakeAmount.z, 0, 10);
		
		if (x != shake.shakeAmount.x || y != shake.shakeAmount.y || z != shake.shakeAmount.z)
			shake.shakeAmount = new Vector3(x, y, z);
		
        /*
		GUILayout.Label("Rotation Amount: " + shake.rotationAmount.ToString());
		
		x = GUILayout.HorizontalSlider(shake.rotationAmount.x, 0, 10);
		y = GUILayout.HorizontalSlider(shake.rotationAmount.y, 0, 10);
		z = GUILayout.HorizontalSlider(shake.rotationAmount.z, 0, 10);
		
		if (x != shake.rotationAmount.x || y != shake.rotationAmount.y || z != shake.rotationAmount.z)
			shake.rotationAmount = new Vector3(x, y, z);
        */ 
		
		GUILayout.Label("Distance: " + shake.distance.ToString("0.00"));
		
		shake.distance = GUILayout.HorizontalSlider(shake.distance, 0, .1f);
		
		GUILayout.Label("Speed: " + shake.speed.ToString("0.00"));
		
		shake.speed = GUILayout.HorizontalSlider(shake.speed, 1, 100);
		
		GUILayout.Label("Decay: " + shake.decay.ToString("0.00%"));
		
		shake.decay = GUILayout.HorizontalSlider(shake.decay, 0, 1);
		
		GUILayout.Space(5);
		
		GUILayout.Label("Presets:");
		
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Explosion"))
			Explosion();
		if (GUILayout.Button("Footsteps"))
			Footsteps();
		if (GUILayout.Button("Bumpy"))
			Bumpy();
		
		GUILayout.EndHorizontal();
			
		if (GUILayout.Button("Reset"))
			Application.LoadLevel(Application.loadedLevel);
		
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		
		GUI.enabled = true;
		
		if (shakeGUI)
		{
			CameraShake.EndShakeGUILayout();	
		}
		else
		{
			GUILayout.EndArea();
		}
	}	
	#endregion
	
	#region Manual shakes
	void Update()
	{
		//transform.parent.position = new Vector3(transform.parent.position.x, Mathf.Cos(Time.time) * .35f, transform.parent.position.z);
		//transform.localPosition = new Vector3(Mathf.Sin(Time.time) * .5f, transform.localPosition.y, transform.localPosition.z);
		if (shake1)
		{
			shakeGUI = false;
			shake1 = false;
			CameraShake.Shake();
		}
		else if (shake2)
		{
			shakeGUI = true;
			shake2 = false;
			CameraShake.Shake();
		}
	}
	#endregion
	
	#region Preset shakes
	void Explosion()
	{
		multiShake = true;
		shakeGUI = true;
		
		// Single shake
		Vector3 rot = new Vector3(2, .5f, 10);
		CameraShake.Shake(5, Vector3.one, rot, 0.25f, 50.0f, 0.20f, 1.0f, true, () => multiShake = false);
	}
	
	void Footsteps()
	{
		shakeGUI = true;
		multiShake = true;
		
		// Sequential shakes
		
		StartCoroutine(DoFootsteps());
	}
	IEnumerator DoFootsteps()
	{	
		Vector3 rot = new Vector3(2, .5f, 1);
		CameraShake.Shake(3, Vector3.one, rot, 0.02f, 50.0f, 0.50f, 1.0f, true, null);
		yield return new WaitForSeconds(1.0f);
		CameraShake.Shake(3, Vector3.one, rot, 0.03f, 50.0f, 0.50f, 1.0f, true, null);
		yield return new WaitForSeconds(1.0f);
		CameraShake.Shake(3, Vector3.one, rot * 1.5f, 0.04f, 50.0f, 0.50f, 1.0f, true, null);
		yield return new WaitForSeconds(1.0f);
		CameraShake.Shake(3, Vector3.one, rot * 2f, 0.05f, 50.0f, 0.50f, 1.0f, true, null);
		yield return new WaitForSeconds(1.0f);
		CameraShake.Shake(3, Vector3.one, rot * 2.5f, 0.06f, 50.0f, 0.50f, 1.0f, true, () => multiShake = false);
	}
	
	void Bumpy()
	{
		shakeGUI = true;
		multiShake = true;
		
		// Multiple sequential shakes at once
		
		StartCoroutine(DoBumpy());
		StartCoroutine(DoBumpy2());
	}
	IEnumerator DoBumpy()
	{	
		Vector3 rot = new Vector3(2, 2, 2);
		for(int i = 0; i < 50; i++)
		{
			CameraShake.Shake(3, Vector3.one, rot, 0.02f, 50.0f, 0.00f, 1.0f, true, null);
			yield return new WaitForSeconds(0.1f);
		}
		CameraShake.Shake(3, Vector3.one, Vector3.one, 0.02f, 50.0f, 0.00f, 1.0f, true, () => multiShake = false);
	}
	IEnumerator DoBumpy2()
	{	
		Vector3 rot = new Vector3(8, 1, 4);
		for (int i = 0; i < 5; i++)
		{
			yield return new WaitForSeconds(1.0f);
			CameraShake.Shake(10, Vector3.up, rot, 0.50f, 50.0f, 0.20f, 1.0f, true, null);
		}
	}
	#endregion
}
