using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellScript : MonoBehaviour {
	public List<CellNode> Adjacents;
	public Vector3 Position;
	public int Weight;
	public bool seen = false;
	public enum Walls { NORTH, SOUTH, EAST, WEST };


	public CellScript()
	{
		Adjacents = new List<CellNode> ();
	}

	public class CellNode
	{
		public Transform cell;
		public Walls parentWrtChild;
		//hack to see the enum value
		public int debugView_Walls {get {return (int) parentWrtChild; }}

		public CellNode(Transform _cell, Walls _parentWrtChild)
		{
			cell = _cell;
			parentWrtChild = _parentWrtChild;
		
		}
	}

}
