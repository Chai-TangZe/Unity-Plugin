using UnityEngine;
using System.Collections;

public class LogicLamp : MonoBehaviour
{
    void OnGUI()
    {
        LogicGlobal.GlobalGUI();
        GUILayout.Label("Lamp rope physics test (procedural rope linked to a dynamic object)");
        GUILayout.Label("Move the mouse while holding down the left button to move the camera");
        GUILayout.Label("Use the spacebar to shoot balls and aim for the lamp to test the physics");
    }
}
