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
	enum AntagonistStates {IDLE, SEEN, RETURN, DEEP_SEARCH, WALK};
	AntagonistStates state = AntagonistStates.IDLE;
	Vector3 targetSeenAt = Vector3.up;
	public GameObject playerObj;
	private Player player;
	Vector3 lastSeenAt;
	bool wentToIdle;
	float timeWhenWentIdle;

	//DFS data structures
	Stack<MazeNode> dfsStack;

	//delete this
	bool dfsOneTime = true;

	void Start() {
		Maze = grid.GetComponent<GridScript> ().Maze;
		MazeMap = grid.GetComponent<GridScript> ().MazeMap;
		whereIam = transform.position;
		fourSides = new List<Ray> ();
		player = playerObj.GetComponent<Player> ();
		lastTwoNodes = new Queue<Vector3> (2);
		dfsStack = new Stack<MazeNode> ();
	}

	void OnTriggerEnter(Collider other)
	{
		whereINeedToBe = other.transform.position;
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

	IEnumerator GetToThisPosn(Vector3 target)
	{
		target.y = 0.4f;
		while (target != transform.position) 
		{
			transform.position = Vector3.MoveTowards (transform.position, target, 0.04f * Time.deltaTime);
			yield return null;
		}
		state = AntagonistStates.DEEP_SEARCH;
	}

	IEnumerator DFSIterative(Vector3 startPos)
	{
		MazeMap.TryGetValue (startPos, out currentNode);
		print ("CurrNode " + currentNode.thisEdge.position);
		float startTime = Time.unscaledTime;
		currentNode.dfsParent = null;
		dfsStack.Push (currentNode);
		MazeNode lastItemPopped = null;
		float speed = 3.0f;
		float searchFor = 6.0f;
		while (dfsStack.Count != 0) 
		{
			if((Time.unscaledTime - startTime) > searchFor)
			{
				dfsStack.Clear();
				break;
			}
			MazeNode node = dfsStack.Pop();
			print ("Popped " + node.thisEdge.position);
			if (node.dfsParent != null)
			{
				print ("Finally Get to parent at " + node.dfsParent.thisEdge.position);
				while(whereINeedToBe != node.dfsParent.thisEdge.position) 
				{
					//try to get to the last item popped
					Vector3 parent = lastItemPopped.thisEdge.position;
					//if I arrive at the lastItemPopped or am there right now
					print ("For now get to parent at " + parent);
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
						print ("Blocking to get to parent " + parent);
						print ("Currently at " + transform.position);
						print ("Rounded off " + whereINeedToBe);
//						if(transform.position != parent)
//							print ("NEQ");
//						else
//							print ("isEQ");
						transform.position = Vector3.MoveTowards (transform.position, parent, speed * Time.deltaTime);
						yield return null;
					}
				 }
			}
			///End of block to get to parent
			///Block to get to node
			
			Vector3 target = node.thisEdge.position;
			target.y =  0.4f;
			print ("Go to popped item " + target);
			while(transform.position != target)
			{
				print ("Blocking to get to target " + target);
				print ("Currently at " + transform.position);
				print ("Rounded off " + whereINeedToBe);
//				if(transform.position.x != target.x &&
//				   transform.position.z != target.z)
//					print ("NEQ");
//				else
//					print ("isEQ");
				transform.position = Vector3.MoveTowards (transform.position, target, speed * Time.deltaTime);
				yield return null;
			}
		    /// End of block to get to node
			foreach(MazeNode adjacentNode in node.Adjacents)
			{
				print ("Pushed " + adjacentNode.thisEdge.transform);
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
				currentNode = currentNode.bfsParent;
				Vector3 goal = currentNode.thisEdge.position;
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
		bool found = false;
		//targetSeenAt should round off to the Cell's collison boundary placed in the centre
		SeekTarget (out targetSeenAt, out found);
		if (found) {
			state = AntagonistStates.SEEN;
			lastSeenAt = targetSeenAt;
		}
		switch (state) {
			case AntagonistStates.SEEN:{
				transform.GetComponent<Renderer>().material.color = Color.red;
				StartCoroutine	(GetToThisPosn(lastSeenAt));
				state = AntagonistStates.WALK;
				break;
			}
			case AntagonistStates.WALK:
			{
				transform.GetComponent<Renderer>().material.color = Color.yellow;
				break;
			}
//			case AntagonistStates.RETURN:
//			{
//				transform.GetComponent<Renderer>().material.color = Color.yellow;
//				GetToExit();
//				break;
//			}
			case AntagonistStates.DEEP_SEARCH:{
				transform.GetComponent<Renderer>().material.color = Color.green;
				//delete this
				lastSeenAt.y = 0.0f;
				print ("Deep searching from "+lastSeenAt);
				StartCoroutine	(DFSIterative (lastSeenAt));
				state = AntagonistStates.WALK;
				break;
			}
			default:
			{
				transform.GetComponent<Renderer>().material.color = Color.white;
				break;
			}
		}
	}
}
		
	
