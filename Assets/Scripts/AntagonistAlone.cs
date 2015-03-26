using UnityEngine;
using System.Collections;

public class AntagonistAlone : MonoBehaviour {
			
	private Rigidbody rb;
	public float speed;
	bool reached;

	float m_speed;
	float m_speed_multi = 2;

	private Vector3 goal = new Vector3(4f,0.425f, 1f);
	
	// All physics code goes here
	void FixedUpdate () {
		float moveHori = Input.GetAxis ("Horizontal");
		float moveVert = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3 (moveHori, -100.0f, moveVert);
		
		rb.AddForce (movement * speed * Time.deltaTime);
		
	}
	
	void Start() {
		rb = GetComponent<Rigidbody>();
		reached = false;
	}

	void BFS()
	{

	}

	void Update()
	{
		m_speed = Time.deltaTime * m_speed_multi;
		if (!reached) {
			Vector3 whereIam = transform.position;
			Vector3 distanceLeft = goal - whereIam;
			distanceLeft.Normalize();
			if (Mathf.Abs(distanceLeft.z) <= 0.1f)
			{
				reached = true;
				transform.position = goal;
			}
			else{
				whereIam += distanceLeft * m_speed;
				transform.position = whereIam;
			}
		}

	}

			
}
		
	
