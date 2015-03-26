using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AntagonistAlone : MonoBehaviour {
			
	private Rigidbody rb;
	public float speed;

	float m_speed;
	float m_speed_multi = 2;

	List<MazeNode> Maze;
	Dictionary<Vector3, MazeNode> MazeMap;
	Vector3 whereIam;
	Vector3 whereINeedToBe;
	Vector3 intermediatePosn;
	MazeNode currentNode;
	MazeNode currentGoal;
	public GameObject grid;
	
	// All physics code goes here
	void FixedUpdate () {
		float moveHori = Input.GetAxis ("Horizontal");
		float moveVert = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3 (moveHori, -100.0f, moveVert);
		
		rb.AddForce (movement * speed * Time.deltaTime);

	}
	
	void Start() {
		rb = GetComponent<Rigidbody>();
		Maze = grid.GetComponent<GridScript> ().Maze;
		MazeMap = grid.GetComponent<GridScript> ().MazeMap;
	}

	void OnTriggerEnter(Collider other)
	{
		whereINeedToBe = other.transform.position;
	//	intermediatePosn = whereIam;
	//	Debug.Log (whereIam);
	}

	void Update()
	{
		m_speed = Time.deltaTime * m_speed_multi;
		whereIam = transform.position;
		if (MazeMap.TryGetValue (whereINeedToBe, out currentNode)) {
			if(currentNode.bfsParent == null){
				currentGoal = currentNode;
			}
			else{
				currentGoal = currentNode.bfsParent;
			}
			Vector3 distanceLeft = currentGoal.thisEdge.position - whereIam;
			whereIam += distanceLeft * m_speed;
			transform.position = whereIam;
		}
	}

			
}
		
	
