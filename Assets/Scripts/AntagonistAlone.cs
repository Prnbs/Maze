using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AntagonistAlone : MonoBehaviour {
	float m_speed;
	float m_speed_multi = 0.8f;
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
	enum AntagonistStates {IDLE, SEEN, RETURN, REACHED_LAST_SEEN};
	AntagonistStates state = AntagonistStates.IDLE;
	Vector3 targetSeenAt = Vector3.up;
	public GameObject playerObj;
	private Player player;

	void Start() {

		Maze = grid.GetComponent<GridScript> ().Maze;
		MazeMap = grid.GetComponent<GridScript> ().MazeMap;
		whereIam = transform.position;
		fourSides = new List<Ray> ();
		player = playerObj.GetComponent<Player> ();
	}

	void OnTriggerEnter(Collider other)
	{
		whereINeedToBe = other.transform.position;
	}

	void Update(){
		m_speed = Time.deltaTime * m_speed_multi;
		bool found = false;
		if (HaveArrived (transform.position, targetSeenAt)) {

			SeekTarget (out targetSeenAt, out found);
		} 
		else {
			transform.GetComponent<Renderer>().material.color = Color.green;
		}
		if (found) {
			state = AntagonistStates.SEEN;
		}
		if(HaveArrived(transform.position, targetSeenAt))
			state = AntagonistStates.IDLE;

		switch (state) {
			case AntagonistStates.SEEN:{
				transform.GetComponent<Renderer>().material.color = Color.red;
				GetToLastSeenTargetPosn(targetSeenAt);
				break;
			}
			case AntagonistStates.IDLE:
			{
				transform.GetComponent<Renderer>().material.color = Color.white;
				break;
			}
			default:
			{
				transform.GetComponent<Renderer>().material.color = Color.yellow;
				break;
			}

		}
	}

	bool HaveArrived(Vector3 antagonistAt, Vector3 targetLastSeenAt)
	{
		if (targetLastSeenAt.Equals(Vector3.up))
			return true;
		float dist = Vector3.Distance (antagonistAt, targetLastSeenAt);
		//Debug.Log (dist);
		return (dist <= 0.4);
	}

	void GetToLastSeenTargetPosn(Vector3 target)
	{
	//	m_speed = Time.deltaTime * m_speed_multi;
		whereIam = transform.position;
		
		Vector3 diffVector = target - whereIam;
		//TODO: Fix this annoying bug
		diffVector.y = 0;
	
		whereIam += diffVector * m_speed;
		transform.position = whereIam;
	}

	void SeekTarget(out Vector3 location, out bool found)
	{
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
			//Debug.DrawRay(rayDir.origin,rayDir.direction);
			if(Physics.Raycast(rayDir, out hit)){
				if(hit.collider.tag == "Target"){
					location =  player.whereHeSawMe;   //hit.collider.transform.position;
					Debug.Log(location);
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
		
	
