using UnityEditor;
using UnityEngine;

public class BuildPreProcessor : MonoBehaviour
{
#if UNITY_CLOUD_BUILD
    public static void UCBPreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
    {
        Debug.Log("UCBPreExport Started");
        string buildNumber = manifest.GetValue("buildNumber", "0");
        Debug.Log("Setting build number to " + buildNumber);
        PlayerSettings.iOS.buildNumber = buildNumber;
        PlayerSettings.Android.bundleVersionCode = int.Parse(buildNumber);
        Debug.Log("UCBPreExport Finished");
    }
#endif
}