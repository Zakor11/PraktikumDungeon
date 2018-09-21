using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper {
    public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag) where T : Component {
        Transform t = parent.transform;
        foreach (Transform tr in t) {
            if (tr.tag == tag) {
                return tr.GetComponent<T>();
            }
        }
        return null;
    }

    public static float Azimuth(Vector3 vector) {
        return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
    }

    public static TItem GetRandom<TItem>(TItem[] array) {
        return array[Random.Range(0, array.Length)];
    }

    public static void SetAlpha(Material material, float value)
    {
        Color color = material.color;
        color.a = value;
        material.color = color;
        Debug.Log("Set Alpha to " + value);
    }
}