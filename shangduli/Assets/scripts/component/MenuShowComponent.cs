using UnityEngine;
using System.Collections;

public class MenuShowComponent : MonoBehaviour
{
	private Transform trans;
	private bool isLerp;
	private float runTime;
	private Vector3 targetScale;
	// Use this for initialization
	void Start ()
	{
		trans = transform;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (isLerp) {
			runTime += Time.deltaTime * 5;
			trans.localScale = Vector3.Lerp (trans.localScale, targetScale, runTime);
			if (runTime>1f) {
				isLerp=false;
			}
		}
	}

	public void Show(){
		isLerp = true;
		runTime = 0f;
		targetScale = Vector3.one;
	}

	public void Hide(){
		isLerp = true;
		runTime = 0f;
		targetScale = new Vector3 (1,0,1);
	}
}
