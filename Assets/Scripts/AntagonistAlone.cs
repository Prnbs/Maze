﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AntagonistAlone : MonoBehaviour {
	float m_speed;
	float m_speed_multi = 0.8f;
	static float yPosn = 0f;

	List<MazeNode> Maze;
	Queue<Vector3> lastTwoNodes;
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
	enum AntagonistStates {IDLE, SEEN, RETURN, DEEP_SEARCH};
	AntagonistStates state = AntagonistStates.IDLE;
	Vector3 targetSeenAt = Vector3.up;
	public GameObject playerObj;
	private Player player;
	Vector3 lastSeenAt;
	bool wentToIdle;
	float timeWhenWentIdle;

	void Start() {

		Maze = grid.GetComponent<GridScript> ().Maze;
		MazeMap = grid.GetComponent<GridScript> ().MazeMap;
		whereIam = transform.position;
		fourSides = new List<Ray> ();
		player = playerObj.GetComponent<Player> ();
		lastTwoNodes = new Queue<Vector3> (2);
	}

	void OnTriggerEnter(Collider other)
	{
		whereINeedToBe = other.transform.position;

		lastTwoNodes.Enqueue (whereINeedToBe);
		while (lastTwoNodes.Count > 2)
			lastTwoNodes.Dequeue ();
	}

	void DFS()
	{
		if (MazeMap.TryGetValue (whereINeedToBe, out currentNode)) {
			//currentNode is the startNode for DFS
			//but among the adjacents of this node we need to prune the Node from which we just came
			Debug.Log(lastTwoNodes.Peek());
		}
	}


	void Update(){
		m_speed = Time.deltaTime * m_speed_multi;
		bool found = false;

		//targetSeenAt should round off to the Cell's collison boundary placed in the centre
		SeekTarget (out targetSeenAt, out found);
//		Debug.Log (targetSeenAt);

		//the player could have run away in which case UP will be returned
		if (!targetSeenAt.Equals (Vector3.up)) {
			lastSeenAt = targetSeenAt;
		} 

		if (found) {
			state = AntagonistStates.SEEN;
			wentToIdle = false;
		}
		else if (transform.position.Equals (lastSeenAt)) {
			state = AntagonistStates.IDLE;
			if(!wentToIdle){
				wentToIdle = true;
				timeWhenWentIdle = Time.unscaledTime;
			}
		}
		if (wentToIdle) {
			//perform DFS for 10s
			if((Time.unscaledTime - timeWhenWentIdle) > 10){
				state = AntagonistStates.RETURN;
				wentToIdle = false;
			}
			else{
				//trigger dfs here
				state = AntagonistStates.DEEP_SEARCH;
			}
		}

		switch (state) {
			case AntagonistStates.SEEN:{
				transform.GetComponent<Renderer>().material.color = Color.red;
				GetToLastSeenTargetPosn(lastSeenAt);
				break;
			}
			case AntagonistStates.IDLE:
			{
				transform.GetComponent<Renderer>().material.color = Color.white;
				break;
			}
			case AntagonistStates.RETURN:
			{
				transform.GetComponent<Renderer>().material.color = Color.white;
				GetToExit();
				break;
			}
			case AntagonistStates.DEEP_SEARCH:{
				transform.GetComponent<Renderer>().material.color = Color.green;
				DFS();
				break;
			}
			default:
			{
				transform.GetComponent<Renderer>().material.color = Color.yellow;
				break;
			}

		}
	}

	//TODO: DELETE
	bool HaveArrived(Vector3 antagonistAt, Vector3 targetLastSeenAt)
	{
		if (targetLastSeenAt.Equals(Vector3.up))
			return true;
		float dist = Vector3.Distance (antagonistAt, targetLastSeenAt);
	//	Debug.Log (dist);
		return (dist <= 0.09);
	}


	void GetToLastSeenTargetPosn(Vector3 target)
	{
		transform.position = Vector3.MoveTowards (transform.position, target, 2.0f * Time.deltaTime);
	}

	void SeekTarget(out Vector3 location, out bool found)
	{
		whereIam = transform.position;
		transform.GetComponent<Renderer>().material.color = Color.yellow;
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
			Debug.DrawRay(rayDir.origin,rayDir.direction*10f);
			if(Physics.Raycast(rayDir, out hit)){

				if(hit.collider.tag == "Target"){
					location =  player.whereHeSawMe;   //hit.collider.transform.position;
					location.y = 0.4f;
//					Debug.Log (location);
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
			Vector3 goal = currentGoal.thisEdge.position;
			goal.y = 0.4f;

			transform.position = Vector3.MoveTowards (transform.position, goal, 2.0f * Time.deltaTime);
		}
	}
}
		
	
