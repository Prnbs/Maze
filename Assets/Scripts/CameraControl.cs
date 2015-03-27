using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	RaycastHit hit;
	bool leftClickFlag = true;
	Vector3 toGo;

	public GameObject antagonist;

	void Start()
	{
		toGo = antagonist.GetComponent<Player> ().userInput;
	}
	
	void Update () 
	{
		/***Left Click****/
		if (Input.GetKey(KeyCode.Mouse0) && leftClickFlag)
			leftClickFlag = false;
		
		if (!Input.GetKey(KeyCode.Mouse0) && !leftClickFlag)
		{
			leftClickFlag = true;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100))
			{
				float X = hit.point.x;
				float Z = hit.point.z;
				toGo = new Vector3(X, 0f, Z);
				antagonist.GetComponent<Player> ().userInput = toGo;
			}
		}
	}
}
