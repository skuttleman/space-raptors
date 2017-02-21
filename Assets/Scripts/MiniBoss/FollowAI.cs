using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FollowAI : MonoBehaviour {
	public class Capabilities {
		public float speed;
		public float jumpForce;
		public float jumpHeight;
		public float jumpWidth;
		public float reactionTime;
	}

	public class Pointers {
		public Rigidbody2D rigidBody;
		public Animator animator;
		public GameObject player;
	}

	public Capabilities capabilities;
	public Pointers pointers;
	public float stuckYarp;
	public bool getem;

	private GameObject playerPlatform;
	private GameObject[] path;
	private float timeUntilReady;
	private bool following = false;
	private float stuckMebbe = 0f;
	private Dictionary<GameObject, bool> collisions;
	private bool pacing;

	void Start() {
		timeUntilReady = 0f;
		pointers.player = GameObject.Find("Player");
		collisions = new Dictionary<GameObject, bool>();
		path = new GameObject[0];
		capabilities = new Capabilities();
		pointers = new Pointers();
	}

	void FixedUpdate() {
		if (stuckMebbe >= stuckYarp || !getem) {
			StopThat();
		}
		if (!getem) {
			return;
		}
		stuckMebbe += Time.deltaTime;

		GameObject playerPlat = Methods.onTaggedObject(pointers.player, 0.1f, "Platform");
		GameObject myPlat = Methods.onTaggedObject(this.gameObject, 0.1f, "Platform");
		if (playerPlat != null && playerPlat != playerPlatform) {
			playerPlatform = playerPlat;
		}
		if (!following && !pacing && timeUntilReady != 0f && myPlat != null && playerPlat != null && !pacing && (path.Length == 0 || myPlat != playerPlat)) {
			GameObject[] newPath = PathFinding.buildSteps(pointers.player, this.gameObject, "Platform", 0.1f, capabilities);
			if (newPath.Length != 0) {
				StopThat();
				path = newPath;
				StartCoroutine(FollowPath(path, 0.1f, this.gameObject, pointers.rigidBody, capabilities));
				timeUntilReady = capabilities.reactionTime;
			} else {
				StopThat();
				StartCoroutine(Pacing(new Methods.ObjectWithCorners(myPlat)));
			}
		}
		if (timeUntilReady != 0f && !following) {
			timeUntilReady = Mathf.Max(timeUntilReady - (capabilities.reactionTime * Time.deltaTime), 0f);
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
		collisions[other.transform.gameObject] = true;
	}

	void OnCollisionExit2D(Collision2D other) {
		collisions[other.transform.gameObject] = false;
	}

	void StopThat() {
		StopAllCoroutines();
		stuckMebbe = 0f;
		following = false;
		path = null;
		pacing = false;
	}

	private void FacePlayer() {
		SetLocalScaleX(pointers.player.transform.position.x > transform.position.x ? -1f : 1f);
	}

	private IEnumerator Pacing(Methods.ObjectWithCorners platform) {
		pacing = true;
		float time;

		SetLocalScaleX(1f);
		time = 0;
		yield return new WaitForFixedUpdate();
		while (time < 1f && (!collisions.ContainsKey(pointers.player) || !collisions[pointers.player]) && transform.position.x > platform.corners.topLeft.x) {
			transform.Translate(new Vector2(-1f * capabilities.speed * Time.deltaTime, 0));
			time += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}

		SetLocalScaleX(-1f);
		time = 0;
		yield return new WaitForFixedUpdate();
		while (time < 1f && (!collisions.ContainsKey(pointers.player) || !collisions[pointers.player]) && transform.position.x < platform.corners.topRight.x) {
			transform.Translate(new Vector2(capabilities.speed * Time.deltaTime, 0));
			time += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}

		FacePlayer();
		time = 0;
		yield return new WaitForFixedUpdate();
		while (time < 1f && (!collisions.ContainsKey(pointers.player) || !collisions[pointers.player])) {
			time += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}

		pacing = false;
	}

	public IEnumerator FollowPath(GameObject[] path, float timeout, GameObject me, Rigidbody2D body, Capabilities stats) {
		following = true;
		for (int i = 1; i < path.Length && following; i++) {
			GameObject plat = Methods.onTaggedObject(me, 0.1f, "Platform");
			if (plat != null && plat != path[i - 1]) {
				i = path.Length;
			} else {
				PathFinding.Instructions howTo = PathFinding.howToGetThere(me, path[i - 1], path[i], 0.1f, stats);
				if (howTo.instructions.Length > 0 && getem) {
					yield return StartCoroutine(Move(howTo, me, body, stats));
					yield return new WaitForSeconds(0.1f);
				} else {
					i = path.Length;
				}
			}
		}
		following = false;
	}

	private IEnumerator Move(PathFinding.Instructions step, GameObject me, Rigidbody2D body, Capabilities stats) {
		stuckMebbe = 0;
		Func<Vector2[], GameObject, Rigidbody2D, Capabilities, IEnumerator> Action = DetermineAction(step.action);
		yield return StartCoroutine(Action(step.instructions, me, body, stats));

	}

	private Func<Vector2[], GameObject, Rigidbody2D, Capabilities, IEnumerator> DetermineAction(string action) {
		switch(action) {
			case "FallLeft":
				return FallLeft;
			case "FallRight":
				return FallRight;
			case "FallAroundLeft":
				return FallAroundLeft;
			case "FallAroundRight":
				return FallAroundRight;
			case "MoveLeft":
				return GoLeft;
			case "MoveRight":
				return GoRight;
			case "JumpLeft":
				return JumpLeft;
			case "JumpRight":
				return JumpRight;
			case "JumpAroundLeft":
				return JumpAroundLeft;
			case "JumpAroundRight":
				return JumpAroundRight;
			default:
				return DoNothing;
		}
	}

	private void Jump(Rigidbody2D body, Capabilities stats) {
		body.AddForce(new Vector2(0, stats.jumpForce));
	}

	private IEnumerator DoUntil(Func<Void> action, Func<float?, bool> condition, float? arg) {
		while (condition(arg) && following) {
			action();
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator DoNothing(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		yield return new WaitForFixedUpdate();
	}

	private IEnumerator GoLeftOrRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats, float factor) {
		me.transform.localScale = new Vector3(factor, me.transform.localScale.y, me.transform.localScale.z);
		yield return StartCoroutine(DoUntil(() => {
			me.transform.Translate(new Vector3(factor * -1f * stats.speed * Time.deltaTime, 0, 0));
		}, _ => {
			return me.transform.position.x > instructions[0].x - (stats.speed * Time.deltaTime);
		}, null));
	}

	private IEnumerator GoLeft(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		return GoLeftOrRight(instructions, me, body, stats, 1f);
	}

	private IEnumerator GoRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		return GoLeftOrRight(instructions, me, body, stats, -1f);
	}

	private IEnumerator FallLeftOrRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats, float factor) {
		me.transform.localScale = new Vector3(factor, me.transform.localScale.y, me.transform.localScale.z);
		yield return StartCoroutine(DoUntil(() => {
			me.transform.Translate(new Vector3(factor * -1f * stats.speed * Time.deltaTime, 0, 0));
		}, untilY => {
			return me.transform.position.y >= untilY - 0.1 ||
				me.transform.position.x >= instructions[0].x;
		}, me.transform.position.y));
	}

	private IEnumerator FallLeft(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		return FallLeftOrRight(instructions, me, body, stats, 1f);
	}

	private IEnumerator FallRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		return FallLeftOrRight(instructions, me, body, stats, -1f);
	}

	private IEnumerator FallAroundLeftOrRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats, string fall, string go) {
		Func<Vector2[], GameObject, Rigidbody2D, Capabilities, IEnumerator> Fall = DetermineAction(fall);
		Func<Vector2[], GameObject, Rigidbody2D, Capabilities, IEnumerator> Go = DetermineAction(go);
		yield return StartCoroutine(Fall(instructions, me, body, stats));
		yield return new WaitForFixedUpdate();
		yield return DoUntil(() => {}, untilY => {
			return me.transform.position.y >= untilY - 1f && following;
		}, me.transform.position.y);
		yield return StartCoroutine(Go(instructions, me, body, stats));
		yield return new WaitForFixedUpdate();
	}

	private IEnumerator FallAroundLeft(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		return FallAroundLeftOrRight(instructions, me, body, stats, "FallLeft", "GoRight");
	}

	private IEnumerator FallAroundRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		return FallAroundLeftOrRight(instructions, me, body, stats, "FallRight", "GoLeft");
	}

	private IEnumerator JumpLeftOrRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats, string action) {
		Func<Vector2[], GameObject, Rigidbody2D, Capabilities, IEnumerator> Action = DetermineAction(action);
		if (instructions[0].x < me.transform.position.x) {
			yield return StartCoroutine(GoLeft(instructions, me, body, stats));
		} else {
			yield return StartCoroutine(GoRight(instructions, me, body, stats));
		}
		float currentY = me.transform.position.y;
		Jump(body, stats);
		yield return new WaitForFixedUpdate();
		yield return StartCoroutine(DoUntil(() => {}, untilY => {
			Methods.ObjectWithCorners objWCorn = new Methods.ObjectWithCorners(me);
			return objWCorn.corners.topLeft.y <= untilY;
		}, me.transform.position.y));
		yield return new WaitForFixedUpdate();
		yield return StartCoroutine(Action(Methods.slice(instructions, 1), me, body, stats));
		yield return new WaitForFixedUpdate();
	}

	private IEnumerator JumpLeft(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		yield return JumpLeftOrRight(instructions, me, body, stats, "GoLeft");
	}

	private IEnumerator JumpRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		yield return JumpLeftOrRight(instructions, me, body, stats, "GoRight");
	}

	private IEnumerator JumpAroundLeftOrRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats, string goStart, string goEnd, Func<float?, bool> condition) {
		Func<Vector2[], GameObject, Rigidbody2D, Capabilities, IEnumerator> GoStart = DetermineAction(goStart);
		Func<Vector2[], GameObject, Rigidbody2D, Capabilities, IEnumerator> GoEnd = DetermineAction(goEnd);
		yield return StartCoroutine(GoStart(instructions, me, body, stats));
		Jump(body, stats);
		yield return new WaitForFixedUpdate();
		yield return StartCoroutine(DoUntil(() => {
			me.transform.Translate(new Vector3(-1f * stats.speed * Time.deltaTime, 0, 0));
		}, condition, null));
		Methods.ObjectWithCorners objWCorn = null;
		do {
			yield return new WaitForFixedUpdate();
			objWCorn = new Methods.ObjectWithCorners(me);
		} while (objWCorn.corners.topRight.y < instructions[1].y && following);
		yield return new WaitForFixedUpdate();
		yield return StartCoroutine(GoEnd(Methods.slice(instructions, 1), me, body, stats));
		yield return new WaitForFixedUpdate();
	}

	private IEnumerator JumpAroundLeft(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		yield return JumpAroundLeftOrRight(instructions, me, body, stats, "GoLeft", "GoRight", _ => {
			Methods.ObjectWithCorners objWCorn = new Methods.ObjectWithCorners(me);
			return objWCorn.corners.topRight.x >= instructions[1].x;
		});
	}

	private IEnumerator JumpAroundRight(Vector2[] instructions, GameObject me, Rigidbody2D body, Capabilities stats) {
		yield return JumpAroundLeftOrRight(instructions, me, body, stats, "GoRight", "GoLeft", _ => {
			Methods.ObjectWithCorners objWCorn = new Methods.ObjectWithCorners(me);
			return objWCorn.corners.topLeft.x <= instructions[1].x;
		});
	}

	private void SetLocalScaleX(float x) {
		transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
	}
}
