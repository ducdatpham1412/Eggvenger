using System;
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
}
