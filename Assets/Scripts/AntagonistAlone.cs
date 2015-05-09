using UnityEngine;
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

	//DFS data structures
	Transform[,] Grid;
	bool startDFS;
	MazeNode dfsStartPoint;
	Stack<MazeNode> dfsStack;

	void Start() {
		Maze = grid.GetComponent<GridScript> ().Maze;
		MazeMap = grid.GetComponent<GridScript> ().MazeMap;
		whereIam = transform.position;
		fourSides = new List<Ray> ();
		player = playerObj.GetComponent<Player> ();
		lastTwoNodes = new Queue<Vector3> (2);
		//delete this
		Grid = grid.GetComponent<GridScript> ().Grid;
		Transform start = Grid [0, 0];
		dfsStack = new Stack<MazeNode> ();

		MazeNode startNode = new MazeNode (start, 0, null);//no parent for start cell
		if (MazeMap.TryGetValue (startNode.thisEdge.position, out currentNode)) {
			dfsStartPoint = currentNode;
		}
		startDFS = true;
		StartCoroutine	(DFSIterative (dfsStartPoint));
	}

	void OnTriggerEnter(Collider other)
	{
		whereINeedToBe = other.transform.position;
		other.transform.GetComponent<Renderer>().material.color = Color.red;
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

	void GetToThisPosn(Vector3 target)
	{
		target.y = 0.4f;
		transform.position = Vector3.MoveTowards (transform.position, target, 0.04f * Time.deltaTime);
	}

	IEnumerator DFSIterative(MazeNode currentNode)
	{
		currentNode.dfsParent = null;
		dfsStack.Push (currentNode);
		MazeNode lastItemPopped = null;
		float speed = 3.0f;
		while (dfsStack.Count != 0) 
		{
			MazeNode node = dfsStack.Pop();
			if (node.dfsParent != null)
			{
				while(whereINeedToBe != node.dfsParent.thisEdge.position) 
				{
					//try to get to the last item popped
					Vector3 parent = lastItemPopped.thisEdge.position;
					//if I arrive at the lastItemPopped or am there right now
					if(whereINeedToBe.x == lastItemPopped.thisEdge.position.x && 
					   whereINeedToBe.z == lastItemPopped.thisEdge.position.z)
					{
						//then get to last item popped's parent
						if(lastItemPopped.dfsParent != null)
						{
							lastItemPopped = lastItemPopped.dfsParent;
							parent = lastItemPopped.thisEdge.position;
						}
					}
					parent.y =  0.4f;
					while(transform.position != parent)
					{
						transform.position = Vector3.MoveTowards (transform.position, parent, speed * Time.deltaTime);
						yield return null;
					}
				 }
			}
			///End of block to get to parent
			///Block to get to node
			
			Vector3 target = node.thisEdge.position;
			target.y =  0.4f;
			while(transform.position != target)
			{
				transform.position = Vector3.MoveTowards (transform.position, target, speed * Time.deltaTime);
				yield return null;
			}
		    /// End of block to get to node
			foreach(MazeNode adjacentNode in node.Adjacents)
			{
				adjacentNode.dfsParent = node;
				dfsStack.Push(adjacentNode);
			}
			lastItemPopped = node;
		}
		print ("stack empty");
		//use bfs to get back to exit
		//transform from vector to MazeNode
		if (MazeMap.TryGetValue (whereINeedToBe, out currentNode)) {
			while(currentNode.bfsParent != null)
			{
				currentGoal = currentNode.bfsParent;
				currentNode = currentGoal;
				Vector3 goal = currentGoal.thisEdge.position;
				goal.y = 0.4f;
				while(transform.position != goal)
				{
					transform.position = Vector3.MoveTowards (transform.position, goal, speed * Time.deltaTime);
					yield return null;
				}
			}
			if(currentNode.bfsParent == null){
				currentGoal = currentNode;
				transform.rotation = Quaternion.identity;
			}
		}
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

	void Update(){
		m_speed = Time.deltaTime * m_speed_multi;
		
		//targetSeenAt should round off to the Cell's collison boundary placed in the centre
		//		SeekTarget (out targetSeenAt, out found);
		////		Debug.Log (targetSeenAt);
		//
		//		//the player could have run away in which case UP will be returned
		//		if (!targetSeenAt.Equals (Vector3.up)) {
		//			lastSeenAt = targetSeenAt;
		//		} 
		//
		//		if (found) {
		//			state = AntagonistStates.SEEN;
		//			wentToIdle = false;
		//		}
		//		else if (transform.position.Equals (lastSeenAt)) {
		//			state = AntagonistStates.IDLE;
		//			if(!wentToIdle){
		//				wentToIdle = true;
		//				timeWhenWentIdle = Time.unscaledTime;
		//			}
		//		}
		//		if (wentToIdle) {
		//			//perform DFS for 10s
		//			if((Time.unscaledTime - timeWhenWentIdle) > 10){
		//				state = AntagonistStates.RETURN;
		//				wentToIdle = false;
		//			}
		//			else{
		//				//trigger dfs here
		//				state = AntagonistStates.DEEP_SEARCH;
		//			}
		//		}
		//
		//		switch (state) {
		//			case AntagonistStates.SEEN:{
		//				transform.GetComponent<Renderer>().material.color = Color.red;
		//				GetToThisPosn(lastSeenAt);
		//				break;
		//			}
		//			case AntagonistStates.IDLE:
		//			{
		//				transform.GetComponent<Renderer>().material.color = Color.white;
		//				break;
		//			}
		//			case AntagonistStates.RETURN:
		//			{
		//				transform.GetComponent<Renderer>().material.color = Color.white;
		//				GetToExit();
		//				break;
		//			}
		//			case AntagonistStates.DEEP_SEARCH:{
		//				transform.GetComponent<Renderer>().material.color = Color.green;
		//				DFSInit();
		//				break;
		//			}
		//			default:
		//			{
		//				transform.GetComponent<Renderer>().material.color = Color.yellow;
		//				break;
		//			}
		//		}

		
	}
}
		
	
