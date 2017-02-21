using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Methods : MonoBehaviour {
	public class Corners {
		public Vector2 topLeft;
		public Vector2 topRight;
		public Vector2 bottomLeft;
		public Vector2 bottomRight;
		public Vector2 landCenter;

		public Corners(GameObject obj) {
			Vector2 pos = new Vector2 (obj.transform.position.x, obj.transform.position.y);
			BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
			Vector2 ext = new Vector2(collider.bounds.extents.x, collider.bounds.extents.y);
			Vector2 off = collider.offset;
			topLeft = new Vector2(pos.x - ext.x + off.x, pos.y + ext.y + off.y);
			topRight = new Vector2(pos.x + ext.x + off.x, pos.y + ext.y + off.y);
			bottomLeft = new Vector2(pos.x - ext.x + off.x, pos.y - ext.y + off.y);
			bottomRight = new Vector2(pos.x + ext.x + off.x, pos.y - ext.y + off.y);
			landCenter = new Vector2(Methods.average(topLeft.x, topRight.x), topLeft.y);
		}
	}

	public class ObjectWithCorners {
		public GameObject gameObject;
		public Corners corners;

		public ObjectWithCorners(GameObject obj) {
			this.gameObject = obj;
			this.corners = new Corners(obj);
		}
	}

	public class Comparer<T> : IComparer<T> {
		private Func<T, T, double> comparer;

		public Comparer(Func<T, T, double> comparer) {
			this.comparer = comparer;
		}

		public int Compare(T item1, T item2) {
			double output = comparer(item1, item2);
			if (output > 0) {
				return 1;
			} else if (output < 0) {
				return -1;
			}
			return 0;
		}
	}

	public static GameObject onTaggedObject(GameObject obj, float tolerance, string tag) {
		Corners objectCorners = new Corners(obj);
		GameObject[] platforms = GameObject.FindGameObjectsWithTag(tag);
		for (int i = 0; i < platforms.Length; i++) {
			Corners corners = new Corners(platforms[i]);
			if (objectCorners.bottomRight.x > corners.topLeft.x &&
				objectCorners.bottomLeft.x < corners.topRight.x &&
				Mathf.Abs(objectCorners.bottomLeft.y - corners.topLeft.y) <= tolerance
			) {
				return platforms[i];
			}
		}
		return null;
	}

	public static GameObject onSomething(GameObject obj, float tolerance) {
		RaycastHit2D ray = RaycastClosest(obj.transform.position, new Vector2(0, -1), obj.transform);
		ObjectWithCorners corner = new ObjectWithCorners(obj);
		if (ray.distance <= (obj.transform.position.y - corner.corners.bottomLeft.y) + tolerance) {
			return ray.transform.gameObject;
		}
		return null;
	}

	public static float distance(Vector2 point1, Vector2 point2) {
		float power = 2f;
		return Mathf.Sqrt(Mathf.Pow(point1.x - point2.x, power) + Mathf.Pow(point1.y - point2.y, power));
	}

	public static float average(float val1, float val2) {
		return (val1 + val2) / 2f;
	}

	public static string leftPad(string str, string chr, int length) {
		if (str.Length < length) {
			return leftPad(chr + str, chr, length);
		}
		return str;
	}

	public static int indexOf<T>(T[] array, T element) {
		return copy(array).IndexOf(element);
	}

	public static T[] filter<T>(T[] array, Func<T, bool> func) {
		List<T> output = new List<T>();
		for (int i = 0; i < array.Length; i ++) {
			if (func(array[i])) {
				output.Add(array[i]);
			}
		}
		return copy(output);
	}

	public static T[] copy<T>(List<T> array) {
		T[] output = new T[array.Count];
		for (int i = 0; i < array.Count; i ++) {
			output[i] = array[i];
		}
		return output;
	}

	public static List<T> copy<T>(T[] array) {
		List<T> output = new List<T>();
		foreach (T item in array) {
			output.Add(item);
		}
		return output;
	}

	public static T[] slice<T>(T[] array, int start, int end) {
		T[] output = new T[end - start];
		for (int i = start; i < end; i ++) {
			output[i] = array[i];
		}
		return output;
	}

	public static T[] slice<T>(T[] array, int start) {
		return slice(array, start, array.Length);
	}

	public static void forEach<T>(T[] array, Func<T, Void> func) {
		foreach (T item in array) {
			func(item);
		}
	}

	public static void forEach<T>(T[] array, Func<T, int, Void> func) {
		for (int i = 0; i < array.Length; i ++) {
			func(array[i], i);
		}
	}

	public static U[] map<T, U>(T[] array, Func<T, U> func) {
		U[] output = new U[array.Length];
		for (int i = 0; i < array.Length; i ++) {
			output[i] = func(array[i]);
		}
		return output;
	}

	public static U[] map<T, U>(T[] Array, Func<T, int, U> func) {
		U[] output = new U[Array.Length];
		for (int i = 0; i < Array.Length; i ++) {
			output[i] = func(Array[i], i);
		}
		return output;
	}

	public static U reduce<T, U>(T[] array, Func<U, T, U> accumulator, U initial) {
		U value = initial;
		foreach (T item in array) {
			value = accumulator(initial, item);
		}
		return value;
	}

	public static T find<T>(T[] array, Func<T, bool> func) {
		for (int i = 0; i < array.Length; i ++) {
			if (func(array[i])) {
				return array[i];
			}
		}
		return default(T);
	}

	public static T[] insert<T>(T[] array, T element, int position) {
		List<T> output = new List<T>();
		for (int i = 0; i < array.Length; i ++) {
			if (i == position) {
				output.Add(element);
			}
			output.Add(array[i]);
		}
		return copy(output);
	}

	public static T[] eject<T>(T[] array, int position) {
		List<T> output = new List<T>();
		for (int i = 0; i < array.Length; i ++) {
			if (i != position) {
				output.Add(array[i]);
			}
		}
		return copy(output);
	}

	public static T[] eject<T>(T[] array) {
		return eject(array, array.Length - 1);
	}

	public static T[] swap<T>(T[] array, int pos1, int pos2) {
		T temp = array[pos1];
		array[pos1] = array[pos2];
		array[pos2] = temp;
		return array;
	}

	public static T[] sort<T>(T[] array, Func<T, T, double> func) {
		List<T> sorted = copy(array);
		sorted.Sort(new Comparer<T>(func));
		return copy(sorted);
	}

	public static T[] concat<T>(T[] array, T item) {
		T[] output = new T[array.Length + 1];
		for (int i = 0; i < array.Length; i++) {
			output[i] = array[i];
		}
		output[array.Length] = item;
		return output;
	}

	public static T[] concat<T>(T[] array, params T[] items) {
		T[] output = new T[array.Length + 1];
		for (int i = 0; i < array.Length; i++) {
			output[i] = array[i];
		}
		for (int i = array.Length; i < array.Length + items.Length; i++) {
			output[i] = items[i - array.Length];
		}
		return output;
	}

	public static void doTimes(int count, Func<Void> action) {
		for (int i = 0; i < count; i++) {
			action();
		}
	}

	public static T last<T>(T[] array) {
		return array.Length == 0 ? default(T) : array[array.Length - 1];
	}

	public static T[] collect<T>(params T[] items) {
		return items;
	}

	public static RaycastHit2D[] Raycast(Vector2 position, Vector2 direction, Transform trans) {
		RaycastHit2D[] rays = Physics2D.RaycastAll(position, direction);
		return filter(rays, ray => ray.transform != trans && !ray.collider.isTrigger);
	}

	public static RaycastHit2D RaycastClosest(Vector2 position, Vector2 direction, Transform trans) {
		RaycastHit2D[] array = Raycast(position, direction, trans);
		array = sort(array, (ray1, ray2) => distance(trans.position, ray2.point) - distance(trans.position, ray1.point));
		return array[0];
	}

	public static bool compareVectors(Vector3 ve1, Vector3 ve2, float tolerance) {
		return Mathf.Abs(ve1.x - ve2.x) <= tolerance &&
			Mathf.Abs(ve1.y - ve2.y) <= tolerance &&
			Mathf.Abs(ve1.z - ve2.z) <= tolerance;
	}

	public static void destroyChildren(GameObject obj) {
		foreach (Transform child in obj.transform) {
			Destroy(child.gameObject);
		}
	}
}
