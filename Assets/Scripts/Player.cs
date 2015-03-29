using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public Vector3 userInput;

	float m_speed;
	float m_speed_multi = 2.5f;
	private Rigidbody rb;
	Vector3 whereIam;
	public Vector3 whereHeSawMe;

	void FixedUpdate () {
		float moveHori = Input.GetAxis ("Horizontal");
		float moveVert = Input.GetAxis ("Vertical");
		Vector3 movement = new Vector3 (moveHori, 0f, moveVert);
		rb.AddForce (movement * m_speed_multi * Time.deltaTime);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Cell") {
			whereHeSawMe = other.transform.position;
			//Debug.Log(whereHeSawMe);
		}
	}

	// Use this for initialization
	void Start () {
		userInput = Vector3.up;
		rb = GetComponent<Rigidbody>();
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
