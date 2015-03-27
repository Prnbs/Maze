using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public Vector3 userInput;

	float m_speed;
	float m_speed_multi = 1.5f;

	Vector3 whereIam;


	// Use this for initialization
	void Start () {
		userInput = Vector3.up;
	}
	
	// Update is called once per frame
		void Update()
		{
			if (userInput == Vector3.up)
				return;
			m_speed = Time.deltaTime * m_speed_multi;
			whereIam = transform.position;

			Vector3 diffVector = userInput - whereIam;
			//TODO: Fix this annoying bug
			diffVector.y = 0;
			/*float distanceLeft = Vector3.Distance(diffVector, origin);
			if(distanceLeft <= 0.1)
			{
			}*/
			whereIam += diffVector * m_speed;
			transform.position = whereIam;

		}
}
