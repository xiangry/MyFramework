using UnityEngine;

using System.Collections;



public class ShakeCamera : MonoBehaviour

{
	public GameObject shockCam;
	public float shackPos;
	private Vector3 deltaPos = Vector3.zero;

	//public 


	public enum shockDis
	{
		None,
		light,
		middle,
		large
	}
	public shockDis status;

	// Use this for initialization

	void Start ()

	{

	}


	// Update is called once per frame

	void Update ()
	{
		if (status == shockDis.None)
			{
				//Apply poison effect
			shackPos = -1.0f;
				return;
			}
		else if (status == shockDis.light)
			{
				//Apply slow effect
			shackPos = 20.0f;
			}
		else if (status == shockDis.middle)
			{
				//Apply mute effect
			shackPos = 10.0f;
			}
		else if (status == shockDis.large)
			{
				//Apply mute effect
			shackPos = 5.0f;
			}
			shockCam.transform.localPosition -= deltaPos;
			deltaPos = Random.insideUnitSphere / shackPos;
			shockCam.transform.localPosition += deltaPos;

	}

}