using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeNode :  IComparable<MazeNode>
{
	public Transform thisEdge;
	public int edgeWeight;
	public List<MazeNode> Adjacents;
	public MazeNode parent;
	public Walls childWrtParent;
	public enum Walls { NORTH, SOUTH, EAST, WEST };

	//these fields are needed are bfs
	public bool bfsSeen;
	public MazeNode bfsParent;

	//needed for dfs
	public MazeNode dfsParent;

	public MazeNode(Transform edge, int weight, MazeNode forefather)
	{
		thisEdge   = edge;
		edgeWeight = weight;
		parent     = forefather;
		Adjacents  = new List<MazeNode> ();
		bfsSeen    = false;

	}

	public int CompareTo(MazeNode right)
	{
		return this.edgeWeight.CompareTo (right.edgeWeight);
	}
}

public class GridScript : MonoBehaviour 
{
	public Transform CellPrefab;
	public Vector3 GridSize;
	public Transform[,] Grid;
	public Transform AntagonistAt;

	public string[] wallNames = {"North", "South", "East", "West"};

	//The graph will be a list of MazeNodes
	//nVerices = sizeof(Maze)
	public List<MazeNode> Maze;
	//Since unity won't support SortedSet we are forced to use SortedList
	//The weight can't be the key since there will be duplicates
	private List<MazeNode> Heap;
	// The shortest route to the exit 
	// this will be populated by BFS
	private List<Transform> parent;
	//this will be the bfs tree with [0,0] as the start node
	private List<MazeNode> BFSTree;
	//need a better way to do this
	public Dictionary<Vector3, MazeNode> MazeMap;
	//queue for bfs
	Queue<MazeNode> bfsQueue;

	void Start()
	{
		CreateGrid ();
		SetRandomCellWeights ();
		SetAdjacents ();
		Maze     = new List<MazeNode> ();
		Heap     = new List<MazeNode> ();
		MazeMap  = new Dictionary<Vector3, MazeNode> ();
		bfsQueue = new Queue<MazeNode> ();
		CreateMST ();
		CreateMaze ();
		BFS (Maze);
	}

	void BFS(List<MazeNode> maze)
	{
		bfsQueue.Enqueue (maze [0]);
		maze [0].bfsSeen = true;
		maze [0].bfsParent = null;
		while (bfsQueue.Count > 0)
		{
			MazeNode queElem = bfsQueue.Dequeue();
			foreach(MazeNode adjacent in queElem.Adjacents)
			{
				if(!adjacent.bfsSeen)
				{
					adjacent.bfsSeen = true;
					bfsQueue.Enqueue(adjacent);
					adjacent.bfsParent = queElem;
				}
			}
		}
	}

	void CreateMST()
	{
		//Lets just assume Start cell is Grid[0,0]
		Transform cell = Grid[0,0];
		//Grid[0,0].GetComponent<Renderer>().material.color = Color.green;
		MazeNode start = new MazeNode (cell, 0, null);//no parent for start cell
		//stupid repeat of same code happens below
		//add start node
		Maze.Add (start);
		MazeMap.Add (start.thisEdge.position, start);
		start.thisEdge.GetComponent<CellScript>().seen = true;
		foreach (Transform adj in cell.GetComponent<CellScript>().Adjacents)
		{
			int parentWeight   = cell.GetComponent<CellScript> ().Weight; 
			int adjacentWeight = adj.GetComponent<CellScript> ().Weight;
			//now add this vertex to the adjacent list of it's parent
			Heap.Add (new MazeNode (adj, Mathf.Abs (parentWeight - adjacentWeight), start));
		}
		Heap.Sort ();
		//stop when maze contains all the vertices
		while (Maze.Count < GridSize.x * GridSize.z) 
		{
			//get edge with lowest edge weight
			MazeNode minNode = Heap[0];
			Heap.RemoveAt(0);
			//Add this vertex if it isn't already in the maze
			if(!minNode.thisEdge.GetComponent<CellScript>().seen)
			{
				//ensure this vertex won'rt get added to the maze in the future
				minNode.thisEdge.GetComponent<CellScript>().seen = true;
				//found a vertex which goes in the MST
				minNode.childWrtParent = WhereIsChild(minNode.parent.thisEdge.GetComponent<CellScript>().Position,
				                                      minNode.thisEdge.GetComponent<CellScript>().Position);
				Maze.Add(minNode);
				MazeMap.Add (minNode.thisEdge.position, minNode);
				//now add this vertex to the adjacent list of it's parent
				minNode.parent.Adjacents.Add(minNode);
			//	minNode.thisEdge.GetComponent<Renderer>().material.color = Color.red;
			} 
			else 
				continue;
			//Now add the adjacents of this new vertex into the heap
			foreach (Transform adj in minNode.thisEdge.GetComponent<CellScript>().Adjacents)
			{
				//provided it isn't in the maze
				if(!adj.GetComponent<CellScript>().seen)
				{
					int parentWeight   = minNode.thisEdge.GetComponent<CellScript> ().Weight; 
					int adjacentWeight = adj.GetComponent<CellScript> ().Weight;
					//calculate edge weight from the given vertex weights
					Heap.Add (new MazeNode (adj, Mathf.Abs (parentWeight - adjacentWeight), minNode));
				}
			}
			Heap.Sort ();
		}
		//save some memory
		Heap.Clear ();
	}

	//get position of child wrt parent
	//eg. if child is above parent this returns NORTH
	MazeNode.Walls WhereIsChild(Vector3 parentPos, Vector3 childPos)
	{
		Vector3 diffVector = childPos - parentPos;
		if (diffVector.x == 0) //North or South
		{
			if(diffVector.z > 0)
				return MazeNode.Walls.NORTH;
			else
				return MazeNode.Walls.SOUTH;
		}
		if (diffVector.z == 0) //East or West
		{
			if(diffVector.x > 0)
				return MazeNode.Walls.WEST;
			else
				return MazeNode.Walls.EAST;
		}
		return MazeNode.Walls.EAST;
	}


	void CreateMaze()
	{
		Transform startWall = Maze[0].thisEdge.FindChild("South");
		Destroy(startWall.gameObject);
		foreach (MazeNode vertex in Maze)
		{	
			//vertex will be parent and adjacent will be child
			//adjacent already knows where it is wrt parent
			foreach(MazeNode adjacent in vertex.Adjacents)
			{
				//TODO: FIX ME!!
				//USING THIS REVERSED FOR NOW, NEED TO GET WHO IS WHERE WRT EACH OTHER
				string child   		 = wallNames[(int)WhereIsParent(adjacent.childWrtParent)];
				string parent  		 = wallNames[(int)adjacent.childWrtParent];
				Transform parentWall = vertex.thisEdge.FindChild(parent);
				Transform childWall  = adjacent.thisEdge.FindChild(child);
				Destroy(parentWall.gameObject);
				Destroy(childWall.gameObject);
			}
		}
	}

	MazeNode.Walls WhereIsParent(MazeNode.Walls childPosn)
	{
		switch (childPosn) 
		{
			case MazeNode.Walls.EAST:
				return MazeNode.Walls.WEST;
			case MazeNode.Walls.WEST:
				return MazeNode.Walls.EAST;
			case MazeNode.Walls.NORTH:
				return MazeNode.Walls.SOUTH;
			case MazeNode.Walls.SOUTH:
				return MazeNode.Walls.NORTH;
			default:
				return MazeNode.Walls.NORTH;
		}
	}

//	void PrintMaze()
//	{
//		foreach (MazeNode ed in Maze) 
//		{
//			Debug.Log(ed.thisEdge.GetComponent<CellScript> ().name);
//		}
//	}

	void CreateGrid()
	{
		Grid = new Transform[(int)GridSize.x,(int)GridSize.z];
		for(int i = 0; i < GridSize.x; i++)
		{
			for(int j = 0; j < GridSize.z; j++)
			{
				Transform newCell;
				newCell 	   = (Transform)Instantiate (CellPrefab, new Vector3(i, 0, j), Quaternion.identity);
				newCell.parent = transform;
				newCell.name   = string.Format("({0},0,{1})",i,j);
				Grid[i,j]      = newCell;
				newCell.GetComponent<CellScript>().Position = new Vector3(i, 0 , j);
			}
		}
		Camera.main.transform.position = Grid [(int)(GridSize.x / 2), (int)(GridSize.z / 2)].position + Vector3.up * 55f;
		Camera.main.orthographicSize   = Mathf.Max (GridSize.x/2, GridSize.z/1.7f);
	}

	void SetRandomCellWeights()
	{
		foreach (Transform child in transform)
		{
			TextMesh childText = child.GetComponentInChildren<TextMesh>();
			int weight		   = UnityEngine.Random.Range(0,100);
			childText.text =  child.position.x.ToString() + "," + child.position.z.ToString();
			child.GetComponent<CellScript>().Weight = weight;
		}
	}

	void SetAdjacents()
	{
		for(int x = 0; x < GridSize.x; x++)
		{
			for(int z = 0; z < GridSize.z; z++)
			{
				Transform cell = Grid[x,z];
				CellScript cScript = cell.GetComponent<CellScript>();

				if(x-1 >= 0)
				{
					cScript.Adjacents.Add(Grid[x-1,z]);
				}
				if(x+1 < GridSize.x)
				{
					cScript.Adjacents.Add(Grid[x+1,z]);
				}
				if(z-1 >= 0)
				{
					cScript.Adjacents.Add(Grid[x,z-1]);
				}
				if(z+1 < GridSize.z)
				{
					cScript.Adjacents.Add(Grid[x,z+1]);
				}
				cScript.Adjacents.Sort(SortAdjacentsByWeight);
			}
		}
	}

	int SortAdjacentsByWeight(Transform itemA, Transform itemB)
	{
		int weightA = itemA.GetComponent<CellScript> ().Weight;
		int weightB = itemB.GetComponent<CellScript> ().Weight;
		return weightA.CompareTo (weightB);
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.R))
		   Application.LoadLevel(0);

	}

}
