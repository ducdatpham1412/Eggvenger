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
}
