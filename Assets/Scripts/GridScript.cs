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
	}

	void Prim()
	{
		//Lets just assume Start cell is Grid[0,0]
		Transform cell = Grid[0,0];
		//Grid[0,0].GetComponent<Renderer>().material.color = Color.green;
		EdgeNode start = new EdgeNode (cell, 0, null);//no edge weight for start cell
		start.thisEdge.GetComponent<CellScript>().seen = true;
		Maze.Add (start);
		foreach (CellScript.CellNode adj in cell.GetComponent<CellScript>().Adjacents)
		{
			int parentWeight   = cell.GetComponent<CellScript> ().Weight; 
			int adjacentWeight = adj.cell.GetComponent<CellScript> ().Weight;
			Heap.Add (new EdgeNode (adj.cell, Mathf.Abs (parentWeight - adjacentWeight), start));
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
				minNode.thisEdge.GetComponent<CellScript>().seen = true;
				Maze.Add(minNode);
				minNode.parent.Adjacents.Add(minNode);

			//	minNode.thisEdge.GetComponent<Renderer>().material.color = Color.red;
			}
			else 
				continue;
			//Now add the adjacents of this new vertex into the heap
			foreach (CellScript.CellNode adj in minNode.thisEdge.GetComponent<CellScript>().Adjacents)
			{
				//provided it isn't in the maze
				if(!adj.cell.GetComponent<CellScript>().seen)
				{
					int parentWeight   = minNode.thisEdge.GetComponent<CellScript> ().Weight; 
					int adjacentWeight = adj.cell.GetComponent<CellScript> ().Weight;
					Heap.Add (new EdgeNode (adj.cell, Mathf.Abs (parentWeight - adjacentWeight), minNode));
				}
			}
			Heap.Sort ();
		}
		PrintMaze ();
	}


	void CreateMaze()
	{
		foreach (EdgeNode vertex in Maze)
		{
			foreach(EdgeNode adjacent in vertex.Adjacents)
			{

			}
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

				CellScript.CellNode cellNode;
				if(x-1 >= 0)
				{
					cellNode = new CellScript.CellNode(Grid[x-1,z], CellScript.Walls.WEST);
					cScript.Adjacents.Add(cellNode);
				}
				if(x+1 < GridSize.x)
				{
					cellNode = new CellScript.CellNode(Grid[x+1,z], CellScript.Walls.EAST);
					cScript.Adjacents.Add(cellNode);
				}
				if(z-1 >= 0)
				{
					cellNode = new CellScript.CellNode(Grid[x,z-1], CellScript.Walls.SOUTH);
					cScript.Adjacents.Add(cellNode);
				}
				if(z+1 < GridSize.z)
				{
					cellNode = new CellScript.CellNode(Grid[x,z+1], CellScript.Walls.NORTH);
					cScript.Adjacents.Add(cellNode);
				}
				cScript.Adjacents.Sort(SortAdjacentsByWeight);
			}
		}
	}

	int SortAdjacentsByWeight(CellScript.CellNode itemA, CellScript.CellNode itemB)
	{
		int weightA = itemA.cell.GetComponent<CellScript> ().Weight;
		int weightB = itemB.cell.GetComponent<CellScript> ().Weight;
		return weightA.CompareTo (weightB);
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.R))
		   Application.LoadLevel(0);
	}

}
