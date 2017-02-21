using UnityEngine;
using System.Collections;

public class CheckpointController : MonoBehaviour {
	private class GlobalIdentifier {
		public string name;
		public Vector3 position;

		public GlobalIdentifier(GameObject obj) {
			this.name = obj.name;
			this.position = obj.transform.position;
		}
	}

	public GameObject[] allWeapons;

	private static CheckpointController checkpoint;
	private GameObject player;
	private PlayerController playerController;
	private bool checkpointed;
	private Vector2 position;


	private int currentWeapon;
	private int[] weapons;
	private int armor;
	private float stealthTime;
	private int[] ammo;
	private int score;
	private GlobalIdentifier[] powerups;
	private GlobalIdentifier[] enemies;
	private GlobalIdentifier[] checkpoints;


	void Awake() {
		if (!checkpoint) {
			DontDestroyOnLoad(this.gameObject);
			checkpoint = this;
		} else if (checkpoint != this) {
			Destroy(this.gameObject);
		}
	}

	void SetPlayer() {
		player = GameObject.Find("Player");
		playerController = player.GetComponent<PlayerController>();
	}

	void ClearPlayer() {
		player = null;
		playerController = null;
	}

	public void Save(Vector2 respawnPosition, GameObject lateCheckpoint) {
		SetPlayer();
		checkpointed = true;
		position = respawnPosition;
		currentWeapon = FindInList(allWeapons, playerController.currentWeapon);
		armor = playerController.armor;
		playerController.stealthTime = 20;
		stealthTime = playerController.stealthTime;
		score = playerController.GetScore();

		weapons = Methods.map(playerController.weapons, (_, i) => {
			return FindInList(allWeapons, playerController.weapons[i]);
		});

		ammo = Methods.map(ammo, (_, i) => {
			return playerController.ammo[i];
		});

		powerups = StoreAlive(GameObject.FindGameObjectsWithTag("Powerup"));
		enemies = StoreAlive(GameObject.FindGameObjectsWithTag("Enemy"));
		checkpoints = StoreAlive(GameObject.FindGameObjectsWithTag("Checkpoint"), lateCheckpoint);

		ClearPlayer();
	}

	public void Load() {
		SetPlayer();
		if (checkpointed) {
			player.transform.position = position;
			playerController.weapons = new GameObject[weapons.Length];
			Methods.forEach(weapons, (weapon, i) => {
				playerController.weapons[i] = allWeapons[weapon];
			});
			playerController.currentWeapon = allWeapons[currentWeapon];
//			playerController.SwitchWeapon(playerController.currentWeapon);

			DestroyTheVanquished(powerups, "Powerup");
			DestroyTheVanquished(enemies, "Enemy");
			DestroyTheVanquished(checkpoints, "Checkpoint");

			playerController.armor = armor;
			playerController.stealthTime = stealthTime;
			playerController.ResetScore(score);
			playerController.ammo = ammo;

			playerController.InitHUD();
		}

		playerController.OnSpawn();
		playerController.SwitchWeapon(playerController.currentWeapon);
		ClearPlayer();
	}

	private void DestroyTheVanquished(GlobalIdentifier[] list, string tag) {
		int destroyed = 0;
		GameObject[] inScene = GameObject.FindGameObjectsWithTag(tag);
		Methods.forEach(inScene, sceneItem => {
			GlobalIdentifier item = Methods.find(list, identifier => {
				return sceneItem != null && identifier != null && sceneItem.name == identifier.name &&
					Methods.compareVectors(sceneItem.transform.position, identifier.position, 15f);
			});
			if (item != null) {
				Destroy(sceneItem);
				destroyed ++;
			}
		});
	}

	private GlobalIdentifier[] StoreAlive(GameObject[] list, GameObject filter) {
		GameObject[] filteredList = Methods.filter(list, item => item != filter);
		return Methods.map(filteredList, item => new GlobalIdentifier(item));
	}

	private GlobalIdentifier[] StoreAlive(GameObject[] list) {
		return StoreAlive(list, null);
	}

	private int FindInList(GameObject[] list, GameObject item) {
		for (int i = 0; i < list.Length; i++) {
			if (item.name == list[i].name || item.name == list[i].name + "(Clone)") {
				return  i;
			}
		}
		return -1;
	}
}
