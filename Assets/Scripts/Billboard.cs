using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	[SerializeField] bool linked = false;

	public Vector3 relativePos;
	public float angle;
	void Update()
	{
		Vector3 p = Camera.main.transform.position;
		//p.y = transform.position.y;
		transform.LookAt(p);

		Vector3 euler = transform.rotation.eulerAngles;

		transform.rotation = Quaternion.Euler(euler.x, euler.y, angle);

		if (linked)
			transform.position = p + relativePos;

	
	}
}