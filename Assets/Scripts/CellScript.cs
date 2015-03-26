using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellScript : MonoBehaviour {
	public List<Transform> Adjacents;
	public Vector3 Position;
	public int Weight;
	public bool seen = false;

	public CellScript()
	{
		Adjacents = new List<Transform> ();
	}

	void OnTriggerEnter(Collider other)
	{
		string name = gameObject.tag;
		Debug.Log (Position);
	}
}
