using UnityEngine;
using System.Collections;

public class FbxExporter : MonoBehaviour {
    private string fbxname = "fbx";
    public GameObject[] meshObjs;
    // Use this for initialization
    void Start () {
        FBXExporter.ExportFBX("", fbxname, meshObjs, true);
    }
}
