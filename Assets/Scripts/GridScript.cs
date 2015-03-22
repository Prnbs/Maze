using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EdgeNode :  IComparable<EdgeNode>{
	public Transform thisEdge;
	public int edgeWeight;
	public List<Transform> Adjacents;
	public EdgeNode parent;

	public EdgeNode(Transform edge, int weight, EdgeNode forfather){
		thisEdge = edge;
		edgeWeight = weight;
		parent = forfather;
		Adjacents = new List<Transform> ();
	}

	public int CompareTo(EdgeNode right)
	{
		return this.edgeWeight.CompareTo (right.edgeWeight);
	}
}

public class GridScript : MonoBehaviour {
	public Transform CellPrefab;
	public Vector3 GridSize;
	public Transform[,] Grid;

	//The graph will be a list of EdgeNodes
	//nVerices = sizeof(Maze)
	private List<EdgeNode> Maze;
	//Since unity won't support SortedSet we are forced to use SortedList
	//The weight can't be the key since there will be duplicates
	private List<EdgeNode> Heap;

	private EdgeNode ed;

	void Start()
	{
		CreateGrid ();
		SetRandomCellWeights ();
		SetAdjacents ();
		Maze = new List<EdgeNode> ();
		Heap = new List<EdgeNode> ();
		Prim ();
	}

	void Prim(){
		//Lets just assume Start cell is Grid[0,0]
		Transform cell = Grid[0,0];
		Grid[0,0].GetComponent<Renderer>().material.color = Color.green;
		EdgeNode start = new EdgeNode (cell, 0, null);//no edge weight for start cell
		start.thisEdge.GetComponent<CellScript>().seen = true;
		Maze.Add (start);
		foreach (Transform adj in cell.GetComponent<CellScript>().Adjacents) {
			int parentWeight   = cell.GetComponent<CellScript> ().Weight; 
			int adjacentWeight = adj.GetComponent<CellScript> ().Weight;
			Heap.Add (new EdgeNode (adj, Mathf.Abs (parentWeight - adjacentWeight), start));
		}
		Heap.Sort ();
		//stop when maze contains all the vertices
		while (Maze.Count < GridSize.x * GridSize.z) {
			//get edge with lowest edge weight
			EdgeNode minNode = Heap[0];
			Heap.RemoveAt(0);
			//Add this vertex if it isn't already in the maze
			if(!minNode.thisEdge.GetComponent<CellScript>().seen){
				minNode.thisEdge.GetComponent<CellScript>().seen = true;
				Maze.Add(minNode);
				minNode.parent.Adjacents.Add(minNode.thisEdge);
				minNode.thisEdge.GetComponent<Renderer>().material.color = Color.red;
			}
			else 
				continue;
			//Now add the adjacents of this new vertex into the heap
			foreach (Transform adj in minNode.thisEdge.GetComponent<CellScript>().Adjacents) {
				//provided it isn't in the maze
				if(!adj.GetComponent<CellScript>().seen){
					int parentWeight   = minNode.thisEdge.GetComponent<CellScript> ().Weight; 
					int adjacentWeight = adj.GetComponent<CellScript> ().Weight;
					Heap.Add (new EdgeNode (adj, Mathf.Abs (parentWeight - adjacentWeight), minNode));
				}
			}
			Heap.Sort ();
		}
		PrintMaze ();
	}

	void PrintMaze(){
		foreach (EdgeNode ed in Maze) {
			Debug.Log(ed.thisEdge.GetComponent<CellScript> ().name);
		}
	}

	void CreateGrid()
	{
		Grid = new Transform[(int)GridSize.x,(int)GridSize.z];
		for(int i = 0; i < GridSize.x; i++){
			for(int j = 0; j < GridSize.z; j++){
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

	void SetRandomCellWeights(){
		foreach (Transform child in transform) {
			TextMesh childText = child.GetComponentInChildren<TextMesh>();
			int weight = UnityEngine.Random.Range(0,10);
			childText.text = weight.ToString();
			child.GetComponent<CellScript>().Weight = weight;
		}
	}

	void SetAdjacents(){
		for(int x = 0; x < GridSize.x; x++){
			for(int z = 0; z < GridSize.z; z++){
				Transform cell = Grid[x,z];
				CellScript cScript = cell.GetComponent<CellScript>();
				if(x-1 >= 0)
					cScript.Adjacents.Add(Grid[x-1,z]);
				if(x+1 < GridSize.x)
					cScript.Adjacents.Add(Grid[x+1,z]);
				if(z-1 >= 0)
					cScript.Adjacents.Add(Grid[x,z-1]);
				if(z+1 < GridSize.z)
					cScript.Adjacents.Add(Grid[x,z+1]);
				cScript.Adjacents.Sort(SortAdjacentsByWeight);
			}
		}
	}

	int SortAdjacentsByWeight(Transform itemA, Transform itemB){
		int weightA = itemA.GetComponent<CellScript> ().Weight;
		int weightB = itemB.GetComponent<CellScript> ().Weight;
		return weightA.CompareTo (weightB);
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.R))
		   Application.LoadLevel(0);
	}

}
