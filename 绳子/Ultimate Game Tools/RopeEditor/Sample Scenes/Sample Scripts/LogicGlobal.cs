using UnityEngine;
using System.Collections;

public class LogicGlobal : MonoBehaviour
{
    void Start()
    {

    }

    public static void GlobalGUI()
    {
        GUILayout.Label("Press 1-4 to select different sample scenes");
        GUILayout.Space(20);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) Application.LoadLevel(0);
        if(Input.GetKeyDown(KeyCode.Alpha2)) Application.LoadLevel(1);
        if(Input.GetKeyDown(KeyCode.Alpha3)) Application.LoadLevel(2);
        if(Input.GetKeyDown(KeyCode.Alpha4)) Application.LoadLevel(3);
    }
}
