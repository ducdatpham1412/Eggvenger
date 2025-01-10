using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;


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


    public static object Get<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key) {
        if (dict.TryGetValue(key, out TValue value)) {
            return value;
        }
        return null;
    }


    public static long TsNow() {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static string GetID(int byteLength = 5) {
        byte[] randomBytes = new byte[byteLength];
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(randomBytes);
        }
        string base64String = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        return base64String;
    }
}
