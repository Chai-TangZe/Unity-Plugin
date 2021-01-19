/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: FbxExporter.cs
  Author:周亚琪      Version :1.0       Date: 2018/8/15
  Description:
************************************************************/

using UnityEngine;
using System.Collections;

public class FbxExporter : MonoBehaviour {
    private string fbxname = "fbx";
    public GameObject[] meshObjs;
    // Use this for initialization
    void Start () {
        FBXExporter.ExportFBX("", fbxname, meshObjs, true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
