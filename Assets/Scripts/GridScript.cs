using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EdgeNode :  IComparable<EdgeNode>
{
	public Transform thisEdge;
	public int edgeWeight;
	public List<EdgeNode> Adjacents;
	public EdgeNode parent;
	public Walls childWrtParent;

	public enum Walls { NORTH, SOUTH, EAST, WEST };

	public EdgeNode(Transform edge, int weight, EdgeNode forefather)
	{
		thisEdge = edge;
		edgeWeight = weight;
		parent = forefather;
		Adjacents = new List<EdgeNode> ();
	}

	public int CompareTo(EdgeNode right)
	{
		return this.edgeWeight.CompareTo (right.edgeWeight);
	}
}

public class GridScript : MonoBehaviour 
{
	public Transform CellPrefab;
	public Vector3 GridSize;
	public Transform[,] Grid;

	public string[] wallNames = {"North", "South", "East", "West"};

	//The graph will be a list of EdgeNodes
	//nVerices = sizeof(Maze)
	private List<EdgeNode> Maze;
	//Since unity won't support SortedSet we are forced to use SortedList
	//The weight can't be the key since there will be duplicates
	private List<EdgeNode> Heap;

	void Start()
	{
		CreateGrid ();
		SetRandomCellWeights ();
		SetAdjacents ();
		Maze = new List<EdgeNode> ();
		Heap = new List<EdgeNode> ();
		Prim ();
		CreateMaze ();
	}

	void Prim()
	{
		//Lets just assume Start cell is Grid[0,0]
		Transform cell = Grid[0,0];
		//Grid[0,0].GetComponent<Renderer>().material.color = Color.green;
		EdgeNode start = new EdgeNode (cell, 0, null);//no parent for start cell
		//stupid repeat of same code happens below
		//add start node
		Maze.Add (start);
		start.thisEdge.GetComponent<CellScript>().seen = true;
		foreach (Transform adj in cell.GetComponent<CellScript>().Adjacents)
		{
			int parentWeight   = cell.GetComponent<CellScript> ().Weight; 
			int adjacentWeight = adj.GetComponent<CellScript> ().Weight;
			//now add this vertex to the adjacent list of it's parent
			Heap.Add (new EdgeNode (adj, Mathf.Abs (parentWeight - adjacentWeight), start));
		}
		Heap.Sort ();
		//stop when maze contains all the vertices
		while (Maze.Count < GridSize.x * GridSize.z) 
		{
			//get edge with lowest edge weight
			EdgeNode minNode = Heap[0];
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
					Heap.Add (new EdgeNode (adj, Mathf.Abs (parentWeight - adjacentWeight), minNode));
				}
			}
			Heap.Sort ();
		}
		//PrintMaze ();
	}

	//get position of child wrt parent
	//eg. if child is above parent this returns NORTH
	EdgeNode.Walls WhereIsChild(Vector3 parentPos, Vector3 childPos)
	{
		Vector3 diffVector = childPos - parentPos;
		if (diffVector.x == 0) //North or South
		{
			if(diffVector.z > 0)
				return EdgeNode.Walls.EAST;
			else
				return EdgeNode.Walls.WEST;
		}
		if (diffVector.z == 0) //East or West
		{
			if(diffVector.x > 0)
				return EdgeNode.Walls.NORTH;
			else
				return EdgeNode.Walls.SOUTH;
		}
		return EdgeNode.Walls.EAST;
	}


	void CreateMaze()
	{
		foreach (EdgeNode vertex in Maze)
		{	
			//vertex will be parent and adjacent will be child
			//adjacent already knows where it is wrt parent
			foreach(EdgeNode adjacent in vertex.Adjacents)
			{
				//TODO: FIX ME!!
				//USING THIS REVERSED FOR NOW, NEED TO GET WHO IS WHERE WRT EACH OTHER
				string child = wallNames[(int)WhereIsParent(adjacent.childWrtParent)];
				string parent  = wallNames[(int)adjacent.childWrtParent];
				Transform parentWall = vertex.thisEdge.FindChild(parent);
				Transform childWall  = adjacent.thisEdge.FindChild(child);
				Destroy(parentWall.gameObject);
				Destroy(childWall.gameObject);
			//	String tag = adjCell.tag;
				Debug.Log(tag);
			}
		}
	}

	EdgeNode.Walls WhereIsParent(EdgeNode.Walls childPosn)
	{
		switch (childPosn) 
		{
			case EdgeNode.Walls.EAST:
				return EdgeNode.Walls.WEST;
			case EdgeNode.Walls.WEST:
				return EdgeNode.Walls.EAST;
			case EdgeNode.Walls.NORTH:
				return EdgeNode.Walls.SOUTH;
			case EdgeNode.Walls.SOUTH:
				return EdgeNode.Walls.NORTH;
			default:
				return EdgeNode.Walls.NORTH;
		}
	}

	void PrintMaze()
	{
		foreach (EdgeNode ed in Maze) 
		{
			Debug.Log(ed.thisEdge.GetComponent<CellScript> ().name);
		}
	}

	void CreateGrid()
	{
		Grid = new Transform[(int)GridSize.x,(int)GridSize.z];
		for(int i = 0; i < GridSize.x; i++)
		{
			for(int j = 0; j < GridSize.z; j++)
			{
				Transform newCell;
				newCell = (Transform)Instantiate (CellPrefab, new Vector3(i, 0, j), Quaternion.identity);
				newCell.parent = transform;
				newCell.name = string.Format("({0},0,{1})",i,j);
				newCell.GetComponent<CellScript>().Position = new Vector3(i, 0 , j);
				Grid[i,j] = newCell;
			}
		}
		Camera.main.transform.position = Grid [(int)(GridSize.x / 2), (int)(GridSize.z / 2)].position + Vector3.up * 40f;
		Camera.main.orthographicSize = Mathf.Max (GridSize.x/2, GridSize.z/2);
	}

	void SetRandomCellWeights()
	{
		foreach (Transform child in transform)
		{
			TextMesh childText = child.GetComponentInChildren<TextMesh>();
			int weight = UnityEngine.Random.Range(0,10);
			childText.text = weight.ToString();
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
