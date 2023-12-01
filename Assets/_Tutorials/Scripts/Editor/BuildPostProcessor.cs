#if UNITY_IOS
using UnityEngine;
using UnityEditor.iOS.Xcode;
using UnityEditor;

public class BuildPostProcessor
{
    public static void UCBPostExport(string buildPath)
    {
        Debug.Log("UCBPostExport Started");
        Debug.Log($"Build path: {buildPath}");

        string plistPath = buildPath + "/Info.plist";
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict plistRoot = plist.root;

        plistRoot.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);
        plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        plist.WriteToFile(plistPath);

        Debug.Log("UCBPostExport Finished");
    }
}
#endif