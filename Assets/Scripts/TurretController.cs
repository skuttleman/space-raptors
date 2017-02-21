using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour {
	public float distance;
	public float width;
	public float startRotation;
	public float honingSpeed;
	public Vector2 coneOffset;
	public int damage;
	public GameObject laserPrefab;
	public Color startColor;
	public Color endColor;
	public int damageRate;

	private PolygonCollider2D area;
	private GameObject player;
	private bool seesPlayer;
	private float honingTimer;
	private GameObject targetLineNeg;
	private GameObject targetLinePos;
	private GameObject fireLine;
	private int damageRepeat;
	private bool targeting;

	void Start() {
		honingTimer = honingSpeed;
		player = GameObject.Find("Player");
		area = this.gameObject.AddComponent<PolygonCollider2D>();
		area.pathCount = 1;
		targetLineNeg = GetChild("TargetLaserNeg");
		targetLinePos = GetChild("TargetLaserPos");
		fireLine = GetChild("FiringLaser");
		targetLineNeg.SetActive(false);
		targetLinePos.SetActive(false);
		fireLine.SetActive(false);
		damageRepeat = damageRate;
		targeting = true;

		area.SetPath(0, InitVex());
		area.isTrigger = true;
	}

	void FixedUpdate() {
		if (!seesPlayer || player.GetComponent<PlayerController>().stealth) {
			honingTimer = honingSpeed;
			DestroyLines();
			targeting = false;
			StopAllCoroutines();
		} else if (seesPlayer && !targeting){
			targeting = true;
			StartCoroutine(Target());
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject == player) {
			seesPlayer = true;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject == player) {
			seesPlayer = false;
		}
	}

	private Vector2 RotatePoint(Vector2 point, Vector2 origin, float angle) {
		Vector2 ret = new Vector2(0f, 0f);
		while (angle < 0f) angle += 360f;
		angle = angle % 360f;
		float radians = angle * Mathf.Deg2Rad;
		ret.x = ((point.x - origin.x) * Mathf.Cos(radians) -
			(point.y - origin.y) * -Mathf.Sin(radians)) + origin.x;
		ret.y = ((point.x - origin.x) * Mathf.Sin(radians) -
			(point.y - origin.y) * -Mathf.Cos(radians)) + origin.y;
		return ret + origin;
	}

	private Vector3 RotatePoint(Vector3 point, Vector3 origin, float angle) {
		Vector2 ret = RotatePoint(
			new Vector2(point.x, point.y),
			new Vector2(origin.x, origin.y),
			angle);
		return new Vector3(ret.x, ret.y, 0f);
	}

	private Vector2[] InitVex() {
		Vector2[] vex = new Vector2[3];
		vex[0] = new Vector2(coneOffset.x, coneOffset.y);
		vex[1] = RotatePoint(new Vector2(0, distance) + vex[0], vex[0], startRotation + (width / 2));
		vex[2] = RotatePoint(new Vector2(0, distance) + vex[0], vex[0], startRotation - (width / 2));
		return vex;
	}

	private GameObject GetChild(string name) {
		Transform[] transforms = this.gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform t in transforms) {
			if (t.gameObject.name == name) {
				return t.gameObject;
			}
		}
		return null;
	}

	private void DestroyLines() {
		targetLineNeg.SetActive(false);
		targetLinePos.SetActive(false);
		fireLine.SetActive(false);
	}

	private IEnumerator Target() {
		UpdateLine(targetLineNeg, -honingTimer / honingSpeed, startColor, startColor);
		UpdateLine(targetLinePos, honingTimer / honingSpeed, startColor, startColor);
		targetLineNeg.SetActive(true);
		targetLinePos.SetActive(true);
		while (honingTimer > 0) {
			honingTimer -= Time.deltaTime;
			UpdateLine(targetLineNeg, -honingTimer / honingSpeed, startColor, endColor);
			UpdateLine(targetLinePos, honingTimer / honingSpeed, startColor, endColor);
			yield return new WaitForFixedUpdate();
		}
		player.SendMessage("TakeDamage", damage);
		targetLineNeg.SetActive(false);
		targetLinePos.SetActive(false);
		UpdateLine(fireLine, 0, endColor, endColor);
		fireLine.SetActive(true);
		while (true) {
			while (damageRepeat > 0) {
				damageRepeat --;
				yield return new WaitForFixedUpdate();
			}
			damageRepeat = damageRate;
			player.SendMessage("TakeDamage", damage);
			yield return new WaitForFixedUpdate();
		}
	}

	private void UpdateLine(GameObject obj, float percent, Color startColor, Color endColor) {
		LineRenderer line = obj.GetComponent<LineRenderer>();
		Vector3[] vertices = new Vector3[2];
		Vector3 cone = new Vector3(coneOffset.x, coneOffset.y, 1f);
		vertices[0] = cone + this.gameObject.transform.position;
		vertices[1] = RotatePoint(player.transform.position, vertices[0], percent * honingSpeed * 20f) - vertices[0];
		line.SetPositions(vertices);
		Color color = Color.Lerp(endColor, startColor, Mathf.Abs(percent));
		line.SetColors(color, color);
	}
}
