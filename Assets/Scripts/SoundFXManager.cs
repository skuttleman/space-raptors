using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundFXManager : MonoBehaviour {
	public AudioClip explosion_large;
	public AudioClip explosion_medium;
	public AudioClip explosion_small;
	public AudioClip item_pickup_ammo;
	public AudioClip item_pickup_health_max;
	public AudioClip item_pickup_health_25;
	public AudioClip item_pickup_health_50;
	public AudioClip item_pickup_powerup;
	public AudioClip item_pickup_weapons;
	public AudioClip action_player_jump;
	public AudioClip action_raptor_jump;

	private Dictionary<string, Dictionary<string, AudioClip>> sounds;

	void Start () {
		Dictionary<string, AudioClip> explosion = new Dictionary<string, AudioClip>();
		explosion.Add("large", explosion_large);
		explosion.Add("medium", explosion_medium);
		explosion.Add("small", explosion_small);

		Dictionary<string, AudioClip> itemPickup = new Dictionary<string, AudioClip>();
		itemPickup.Add("health_max", item_pickup_health_max);
		itemPickup.Add("health_50", item_pickup_health_50);
		itemPickup.Add("health_25", item_pickup_health_25);
		itemPickup.Add("ammo", item_pickup_ammo);
		itemPickup.Add("powerup", item_pickup_powerup);
		itemPickup.Add("weapons", item_pickup_weapons);

		Dictionary<string, AudioClip> action = new Dictionary<string, AudioClip>();
		action.Add("player_jump", action_player_jump);
		action.Add("raptor_jump", action_raptor_jump);

		sounds.Add("explosion", explosion);
		sounds.Add("item_pickup", itemPickup);
		sounds.Add("action", action);
	}

	public void Play(AudioSource audioSource, string type, string description) {
		if (sounds[type] != null && sounds[type][description] != null) {
			if (type == "explosion") {
				ShakeCamera(description);
			}
			audioSource.PlayOneShot(sounds[type][description]);
		}
	}

	private void ShakeCamera(string amount) {
		CameraController cam = GameObject.Find("Main Camera").GetComponent<CameraController>();
		switch (amount) {
			case "large":
				cam.Shake(6, 0.7f);
				break;
			case "medium":
				cam.Shake(4, 0.4f);
				break;
			case "small":
				cam.Shake(2, 0.1f);
				break;
		}
	}
}
