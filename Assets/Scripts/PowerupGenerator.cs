using UnityEngine;
using System.Collections;

public class PowerupGenerator : MonoBehaviour {
	public float dropRate;
	public GameObject[] ammo;
	public GameObject[] armor;
	public GameObject[] health;
	public float maxDistanceToPlayer;

	private GameObject dropped;
	private float dropCooldown;
	private GameObject[] lastDropList;
	private GameObject player;


	void Start() {
		dropCooldown = dropRate;
		player = GameObject.Find("Player");
	}

	void Update() {
		if (dropCooldown >= 0f && dropped != null) {
			dropCooldown -= Time.deltaTime;
		} else if (!dropped && Methods.distance(transform.position, player.transform.position) > maxDistanceToPlayer) {
			DropItem();
		}
	}

	void DropItem() {
		dropCooldown = dropRate;
		if (lastDropList ==  ammo) {
			lastDropList = Random.value >= 0.5f ? armor : health;
		} else {
			lastDropList = ammo;
		}

		dropped = (GameObject) Instantiate(lastDropList[RandomIndex(lastDropList.Length)], transform.position, Quaternion.identity);
	}

	private int RandomIndex(int length){
		return Mathf.RoundToInt(Random.Range(0, length - 1));
	}
}
