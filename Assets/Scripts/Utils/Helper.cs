using System;
using System.Collections.Generic;
using UnityEngine;


public static class Helper {
    public static Transform FindChildRecursive(Transform parent, string childName) {
        foreach (Transform child in parent) {
            if (child.name == childName)
                return child;

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }


    public static void SetImageUrl(Transform gameObject, string url) {
        ImageLoader imageLoader = gameObject.GetComponent<ImageLoader>();
        imageLoader.SetImageUrl(url);
    }


    public static Color ColorFromHex(string hex) {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color)) {
            return color;
        }
        Debug.LogError("Invalid hex code: " + hex);
        return Color.white;
    }


    static void Swap<T>(List<T> list, int index1, int index2) {
        if (index1 < 0 || index1 >= list.Count || index2 < 0 || index2 >= list.Count) {
            Debug.LogError("Indices are out of range.");
            return;
        }

        T temp = list[index1];
        list[index1] = list[index2];
        list[index2] = temp;
    }


    public static object Get<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key) {
        if (dict.TryGetValue(key, out TValue value)) {
            return value;
        }
        return null;
    }
}
