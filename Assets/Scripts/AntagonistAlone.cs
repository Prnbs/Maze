using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AntagonistAlone : MonoBehaviour {
			
	private Rigidbody rb;
	public float speed;

	float m_speed;
	float m_speed_multi = 1.5f;
	static float yPosn = 0f;

	List<MazeNode> Maze;
	Dictionary<Vector3, MazeNode> MazeMap;
	Vector3 whereIam;
	Vector3 whereINeedToBe;
	Vector3 intermediatePosn;
	MazeNode currentNode;
	MazeNode currentGoal;
	public GameObject grid;
	Vector3 forward  = new Vector3(1f ,yPosn ,0f);
	Vector3 backward = new Vector3(-1f,yPosn ,0f);
	Vector3 left     = new Vector3(0f ,yPosn ,1f);
	Vector3 right    = new Vector3(0f ,yPosn ,-1f);
	List<Ray> fourSides;
	RaycastHit hit;

	// All physics code goes here
	void FixedUpdate () {
		float moveHori = Input.GetAxis ("Horizontal");
		float moveVert = Input.GetAxis ("Vertical");
		Vector3 movement = new Vector3 (moveHori, 0f, moveVert);
		rb.AddForce (movement * speed * Time.deltaTime);
	}
	
	void Start() {
		rb = GetComponent<Rigidbody>();
		Maze = grid.GetComponent<GridScript> ().Maze;
		MazeMap = grid.GetComponent<GridScript> ().MazeMap;
		whereIam = transform.position;
		fourSides = new List<Ray> ();
	}

	void OnTriggerEnter(Collider other)
	{
		whereINeedToBe = other.transform.position;
	}

	void Update(){
		Vector3 targetLoc;
		bool found = false;
		SeekTarget (out targetLoc, out found);
		if (found) {
		//	Debug.Log(targetLoc);
			GetToLastSeenTargetPosn(targetLoc);
		}
	}

	void GetToLastSeenTargetPosn(Vector3 target)
	{
		m_speed = Time.deltaTime * m_speed_multi;
		whereIam = transform.position;
		
		Vector3 diffVector = target - whereIam;
		//TODO: Fix this annoying bug
		diffVector.y = 0;
	
		whereIam += diffVector * m_speed;
		transform.position = whereIam;
		
	}

	void SeekTarget(out Vector3 location, out bool found){
		whereIam = transform.position;
		location = Vector3.up;
		found    = false;
		//TODO:Fix accuracy problem
		Ray rayFwd   = new Ray (whereIam, forward);
		Ray rayBwd   = new Ray (whereIam, backward);
		Ray rayLeft  = new Ray (whereIam, left);
		Ray rayRight = new Ray (whereIam, right);
		fourSides.Clear ();
		fourSides.Add (rayFwd);
		fourSides.Add (rayBwd);
		fourSides.Add (rayLeft);
		fourSides.Add (rayRight);
		foreach (Ray rayDir in fourSides) {
			Debug.DrawRay(rayDir.origin,rayDir.direction);
			//Debug.Log(rayDir);
			if(Physics.Raycast(rayDir, out hit)){
				Debug.Log(hit.collider.tag);
				if(hit.collider.tag == "Target"){
				//	Debug.Log("Hit");
				//	Debug.Log(rayDir);
					location = hit.collider.transform.position;
					found = true;
					break;
				}
			}
		}
	}


	//Uses BFS to get back to the exit
	void GetToExit()
	{
		m_speed = Time.deltaTime * m_speed_multi;
		whereIam = transform.position;
		if (MazeMap.TryGetValue (whereINeedToBe, out currentNode)) {
			if(currentNode.bfsParent == null){
				currentGoal = currentNode;
				transform.rotation = Quaternion.identity;
			}
			else{
				currentGoal = currentNode.bfsParent;
			}
			Vector3 diffVector = currentGoal.thisEdge.position - whereIam;
			//TODO: Fix this annoying bug
			diffVector.y = 0;
			whereIam += diffVector * m_speed;
			transform.position = whereIam;
		}
	}
			
}
		
	
