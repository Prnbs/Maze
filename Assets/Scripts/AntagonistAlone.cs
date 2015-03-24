using UnityEngine;
using System.Collections;

public class AntagonistAlone : MonoBehaviour {
			
	private Rigidbody rb;
	public float speed;
	
	// All physics code goes here
	void FixedUpdate () {
		float moveHori = Input.GetAxis ("Horizontal");
		float moveVert = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3 (moveHori, -100.0f, moveVert);
		
		rb.AddForce (movement * speed * Time.deltaTime);
		
	}
	
	void Start() {
		rb = GetComponent<Rigidbody>();
	}
	

			
}
		
	
