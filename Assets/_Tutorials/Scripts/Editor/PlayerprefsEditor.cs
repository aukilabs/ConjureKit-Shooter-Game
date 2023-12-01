using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PlayerprefsEditor
{
    [MenuItem("Romi/Playerprefs/Clear Save")]
    private static void ClearSave()
    {
        PlayerPrefs.DeleteAll();
    }
}
