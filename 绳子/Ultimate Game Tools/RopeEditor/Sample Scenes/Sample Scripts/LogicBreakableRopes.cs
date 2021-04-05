using UnityEngine;
using System.Collections;

public class LogicBreakableRopes : MonoBehaviour
{
    public UltimateRope Rope1;
    public UltimateRope Rope2;

    bool bBroken1;
    bool bBroken2;

    void Start()
    {
        bBroken1 = false;
        bBroken2 = false;
    }

    void OnGUI()
    {
        LogicGlobal.GlobalGUI();
        GUILayout.Label("Breakable rope test (procedural rope and linkedobjects rope with breakable properties and notifications set)");
        GUILayout.Label("Move the mouse while holding down the left button to move the camera");
        GUILayout.Label("Use the spacebar to shoot balls and aim for the ropes to break them");

        Color colGUIColor = GUI.color;
        GUI.color = new Color(255, 0, 0);
        if(bBroken1) GUILayout.Label("Rope 1 was broken");
        if(bBroken2) GUILayout.Label("Rope 2 was broken");
        GUI.color = colGUIColor;
    }

    void OnRopeBreak(UltimateRope.RopeBreakEventInfo breakInfo)
    {
        if(breakInfo.rope == Rope1) bBroken1 = true;
        if(breakInfo.rope == Rope2) bBroken2 = true;
    }
}
