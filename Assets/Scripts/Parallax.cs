using UnityEngine;
using System.Collections;

public class Parallax : MonoBehaviour {
	public GameObject parallax1;
	public GameObject parallax2;
	public GameObject space;

	private GameObject mainCam;

	void Start () {
		mainCam = GameObject.Find("Main Camera");
	}
	
	void Update () {
		parallax2.gameObject.transform.position = new Vector3(mainCam.transform.position.x * 0.9f, mainCam.transform.position.y * 0.9f, parallax2.gameObject.transform.position.z);
		parallax1.gameObject.transform.position = new Vector3(mainCam.transform.position.x * 0.8f, mainCam.transform.position.y * 0.8f, parallax1.gameObject.transform.position.z);
		space.gameObject.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, space.gameObject.transform.position.z);
	}
}
