using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(UltimateRope))]
public class UltimateRopeEditor : Editor
{
    SerializedProperty PropRopeType;

    SerializedProperty PropRopeStart;
    SerializedProperty PropRopeLayer;
    SerializedProperty PropRopePhysicsMaterial;

    SerializedProperty PropRopeDiameter;
    SerializedProperty PropRopeDiameterScaleX;
    SerializedProperty PropRopeDiameterScaleY;
    SerializedProperty PropRopeSegmentSides;
    SerializedProperty PropRopeMaterial;
    SerializedProperty PropRopeSectionMaterial;

    SerializedProperty PropIsExtensible;
    SerializedProperty PropExtensibleLength;
    SerializedProperty PropHasACoil;
    SerializedProperty PropCoilObject;
    SerializedProperty PropCoilAxisRight;
    SerializedProperty PropCoilAxisUp;
    SerializedProperty PropCoilWidth;
    SerializedProperty PropCoilDiameter;
    SerializedProperty PropCoilNumBones;

    SerializedProperty PropLinkObject;
    SerializedProperty PropLinkAxis;
    SerializedProperty PropLinkOffsetObject;
    SerializedProperty PropLinkTwistAngleStart;
    SerializedProperty PropLinkTwistAngleIncrement;

    SerializedProperty PropBoneFirst;
    SerializedProperty PropBoneLast;
    SerializedProperty PropBoneListNamesStatic;
    SerializedProperty PropBoneListNamesNoColliders;
    SerializedProperty PropBoneAxis;
    SerializedProperty PropBoneColliderType;
    SerializedProperty PropBoneColliderDiameter;
    SerializedProperty PropBoneColliderSkip;
    SerializedProperty PropBoneColliderLength;
    SerializedProperty PropBoneColliderOffset;

    SerializedProperty PropLinkMass;
    SerializedProperty PropLinkSolverIterationCount;
    SerializedProperty PropLinkJointAngularXLimit;
    SerializedProperty PropLinkJointAngularYLimit;
    SerializedProperty PropLinkJointAngularZLimit;
    SerializedProperty PropLinkJointSpringValue;
    SerializedProperty PropLinkJointDamperValue;
    SerializedProperty PropLinkJointMaxForceValue;
    SerializedProperty PropLinkJointBreakForce;
    SerializedProperty PropLinkJointBreakTorque;
    SerializedProperty PropLockStartEndInZAxis;

    SerializedProperty PropSendEvents;
    SerializedProperty PropEventsObjectReceiver;
    SerializedProperty PropOnBreakMethodName;

    SerializedProperty PropPersistAfterPlayMode;
    SerializedProperty PropEnablePrefabUsage;
//  SerializedProperty PropAutoRegenerate;

    [MenuItem("GameObject/Create Other/Ultimate Game Tools/Rope")]
    static void CreateRope() 
    {
        GameObject rope = new GameObject();
        rope.name = "Rope";
        rope.transform.position = Vector3.zero;
        rope.AddComponent<Rigidbody>();
        rope.GetComponent<Rigidbody>().isKinematic = true;
        rope.AddComponent<UltimateRope>();

        Selection.activeGameObject = rope;
    }

    void OnEnable()
    {
        PropRopeType                     = serializedObject.FindProperty("RopeType");

        PropRopeStart                    = serializedObject.FindProperty("RopeStart");
        PropRopeLayer                    = serializedObject.FindProperty("RopeLayer");
        PropRopePhysicsMaterial          = serializedObject.FindProperty("RopePhysicsMaterial");

        PropRopeDiameter                 = serializedObject.FindProperty("RopeDiameter");
        PropRopeDiameterScaleX           = serializedObject.FindProperty("RopeDiameterScaleX");
        PropRopeDiameterScaleY           = serializedObject.FindProperty("RopeDiameterScaleY");
        PropRopeSegmentSides             = serializedObject.FindProperty("RopeSegmentSides");
        PropRopeMaterial                 = serializedObject.FindProperty("RopeMaterial");
        PropRopeSectionMaterial          = serializedObject.FindProperty("RopeSectionMaterial");

        PropIsExtensible                 = serializedObject.FindProperty("IsExtensible");
        PropExtensibleLength             = serializedObject.FindProperty("ExtensibleLength");
        PropHasACoil                     = serializedObject.FindProperty("HasACoil");
        PropCoilObject                   = serializedObject.FindProperty("CoilObject");
        PropCoilAxisRight                = serializedObject.FindProperty("CoilAxisRight");
        PropCoilAxisUp                   = serializedObject.FindProperty("CoilAxisUp");
        PropCoilWidth                    = serializedObject.FindProperty("CoilWidth");
        PropCoilDiameter                 = serializedObject.FindProperty("CoilDiameter");
        PropCoilNumBones                 = serializedObject.FindProperty("CoilNumBones");

        PropLinkObject                   = serializedObject.FindProperty("LinkObject");
        PropLinkAxis                     = serializedObject.FindProperty("LinkAxis");
        PropLinkOffsetObject             = serializedObject.FindProperty("LinkOffsetObject");
        PropLinkTwistAngleStart          = serializedObject.FindProperty("LinkTwistAngleStart");
        PropLinkTwistAngleIncrement      = serializedObject.FindProperty("LinkTwistAngleIncrement");

        PropBoneFirst                    = serializedObject.FindProperty("BoneFirst");
        PropBoneLast                     = serializedObject.FindProperty("BoneLast");
        PropBoneListNamesStatic          = serializedObject.FindProperty("BoneListNamesStatic");
        PropBoneListNamesNoColliders     = serializedObject.FindProperty("BoneListNamesNoColliders");
        PropBoneAxis                     = serializedObject.FindProperty("BoneAxis");
        PropBoneColliderType             = serializedObject.FindProperty("BoneColliderType");
        PropBoneColliderDiameter         = serializedObject.FindProperty("BoneColliderDiameter");
        PropBoneColliderSkip             = serializedObject.FindProperty("BoneColliderSkip");
        PropBoneColliderLength           = serializedObject.FindProperty("BoneColliderLength");
        PropBoneColliderOffset           = serializedObject.FindProperty("BoneColliderOffset");

        PropLinkMass                     = serializedObject.FindProperty("LinkMass");
        PropLinkSolverIterationCount     = serializedObject.FindProperty("LinkSolverIterationCount");
        PropLinkJointAngularXLimit       = serializedObject.FindProperty("LinkJointAngularXLimit");
        PropLinkJointAngularYLimit       = serializedObject.FindProperty("LinkJointAngularYLimit");
        PropLinkJointAngularZLimit       = serializedObject.FindProperty("LinkJointAngularZLimit");
        PropLinkJointSpringValue         = serializedObject.FindProperty("LinkJointSpringValue");
        PropLinkJointDamperValue         = serializedObject.FindProperty("LinkJointDamperValue");
        PropLinkJointMaxForceValue       = serializedObject.FindProperty("LinkJointMaxForceValue");
        PropLinkJointBreakForce          = serializedObject.FindProperty("LinkJointBreakForce");
        PropLinkJointBreakTorque         = serializedObject.FindProperty("LinkJointBreakTorque");
        PropLockStartEndInZAxis          = serializedObject.FindProperty("LockStartEndInZAxis");

        PropSendEvents                   = serializedObject.FindProperty("SendEvents");
        PropEventsObjectReceiver         = serializedObject.FindProperty("EventsObjectReceiver");
        PropOnBreakMethodName            = serializedObject.FindProperty("OnBreakMethodName");

        PropPersistAfterPlayMode         = serializedObject.FindProperty("PersistAfterPlayMode");
        PropEnablePrefabUsage            = serializedObject.FindProperty("EnablePrefabUsage");
//      PropAutoRegenerate               = serializedObject.FindProperty("AutoRegenerate");
    }

    public override void OnInspectorGUI()
    {
        Vector4 v4GUIColor = GUI.contentColor;

        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.

        serializedObject.Update();

        // Show the custom GUI controls

        bool bResetNodePositions     = false;

        bool bDeleteRope             = false;
        bool bRegenerateRope         = false;
        bool bMakeStatic             = false;
        bool bChangeRopeDiameter     = false;
        bool bChangeRopeSegmentSides = false;
        bool bSetupRopeLinks         = false;
        bool bRecomputeCoil          = false;
        bool bSetupRopeJoints        = false;
        bool bSetupRopeMaterials     = false;

        UltimateRope rope = target as UltimateRope;

        bool bIsRopeBreakable = rope.LinkJointBreakForce != Mathf.Infinity || rope.LinkJointBreakTorque != Mathf.Infinity;

        EditorGUILayout.HelpBox(rope.Status == null ? "No status" : rope.Status, rope.IsLastStatusError() ? MessageType.Error : MessageType.Info);

        EditorGUI.BeginChangeCheck();
        PropRopeLayer.intValue = EditorGUILayout.LayerField(new GUIContent("Rope Layer", "Set to a layer for either graphics or collision filtering"), PropRopeLayer.intValue);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeLinks = true;
        }

        EditorGUI.BeginChangeCheck();
        PropRopePhysicsMaterial.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Rope Physic Material", "Physic material assigned to the rope"), PropRopePhysicsMaterial.objectReferenceValue, typeof(PhysicMaterial), false);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeLinks = true;
        }

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(PropRopeType, new GUIContent("Rope Type", "The type of rope to generate"));
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bRegenerateRope = true;
        }

        if(PropRopeType.enumNames[PropRopeType.enumValueIndex] == UltimateRope.ERopeType.Procedural.ToString())
        {
            EditorGUI.BeginChangeCheck();
            GUI.contentColor = PropRopeStart.objectReferenceValue == null ? Color.red : GUI.contentColor;
            PropRopeStart.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Rope Start", "GameObject where the rope starts"), PropRopeStart.objectReferenceValue, typeof(GameObject), true);
            GUI.contentColor = v4GUIColor;
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            PropRopeDiameter.floatValue = EditorGUILayout.FloatField(new GUIContent("Rope Diameter", "Rope's section diameter"), PropRopeDiameter.floatValue);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bChangeRopeDiameter = true;

                if(rope.FirstNodeIsCoil())
                {
                    bRecomputeCoil = true;
                }
            }

            EditorGUI.BeginChangeCheck();
            PropRopeDiameterScaleX.floatValue = EditorGUILayout.FloatField(new GUIContent("Diameter ScaleX", "Rope's section diameter x scale. Allows for non circular sections."), PropRopeDiameterScaleX.floatValue);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bChangeRopeDiameter = true;
            }

            EditorGUI.BeginChangeCheck();
            PropRopeDiameterScaleY.floatValue = EditorGUILayout.FloatField(new GUIContent("Diameter ScaleY", "Rope's section diameter y scale. Allows for non circular sections."), PropRopeDiameterScaleY.floatValue);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bChangeRopeDiameter = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.IntSlider(PropRopeSegmentSides, 3, 128, new GUIContent("Rope Sides", "Set to larger values for rounder rope"));
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bChangeRopeSegmentSides = true;
            }

            EditorGUI.BeginChangeCheck();
            PropRopeMaterial.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Rope Material", "Material assigned to the rope mesh"), PropRopeMaterial.objectReferenceValue, typeof(Material), false);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bSetupRopeMaterials = true;
            }

            EditorGUI.BeginChangeCheck();
            PropRopeSectionMaterial.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Rope Section Material", "Material assigned to the caps and also the inside section of a rope mesh when it breaks"), PropRopeSectionMaterial.objectReferenceValue, typeof(Material), false);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bSetupRopeMaterials = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropEnablePrefabUsage, new GUIContent("Enable Prefab Usage", "Will save the rope procedural mesh to an asset file on disk. Use this if you want to add this object to a prefab, otherwise the mesh won't be instanced properly."), GUILayout.ExpandWidth(true));
            if(EditorGUI.EndChangeCheck() && PropEnablePrefabUsage.boolValue)
            {
                rope.m_strAssetFile = UnityEditor.EditorUtility.SaveFilePanelInProject("Save mesh asset", "mesh_" + rope.name + this.GetInstanceID().ToString() + ".asset", "asset", "Please enter a file name to save the mesh asset to");

                if(rope.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    if(rope.GetComponent<SkinnedMeshRenderer>().sharedMesh != null)
                    {
                        UnityEditor.AssetDatabase.CreateAsset(rope.GetComponent<SkinnedMeshRenderer>().sharedMesh, rope.m_strAssetFile);
                        UnityEditor.AssetDatabase.ImportAsset(rope.m_strAssetFile);
                        UnityEditor.AssetDatabase.Refresh();
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropIsExtensible, new GUIContent("Is Extensible", "Adds the ability to extend the rope's end through scripting"), GUILayout.ExpandWidth(true));
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            if(PropIsExtensible.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                PropExtensibleLength.floatValue = EditorGUILayout.FloatField(new GUIContent("Extensible Length", "The additional rope length that can be extended through scripting."), PropExtensibleLength.floatValue);
                if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                {
                    bRegenerateRope = true;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(PropHasACoil, new GUIContent("Has a Coil", "Adds a coil to the extensible rope that can simulate visually from where it is being extended"), GUILayout.ExpandWidth(true));
                if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                {
                    bRegenerateRope = true;
                }

                if(PropHasACoil.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    GUI.contentColor = PropCoilObject.objectReferenceValue == null ? Color.red : GUI.contentColor;
                    PropCoilObject.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Coil Object", "Object around which the coil is placed"), PropCoilObject.objectReferenceValue, typeof(GameObject), true);
                    GUI.contentColor = v4GUIColor;
                    if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                    {
                        bRegenerateRope = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(PropCoilAxisRight, new GUIContent("Coil Axis Right", "The local axis along which the coil is going to twist"));
                    if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                    {
                        bRegenerateRope = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(PropCoilAxisUp, new GUIContent("Coil Axis Up", "The local coil object's axis that looks up"));
                    if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                    {
                        bRegenerateRope = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    PropCoilWidth.floatValue = EditorGUILayout.FloatField(new GUIContent("Coil Width", "The width of the coil"), PropCoilWidth.floatValue);
                    if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                    {
                        bRegenerateRope = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    PropCoilDiameter.floatValue = EditorGUILayout.FloatField(new GUIContent("Coil Diameter", "The diameter of the coil"), PropCoilDiameter.floatValue);
                    if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                    {
                        bRegenerateRope = true;
                    }

                    EditorGUI.BeginChangeCheck();
                    PropCoilNumBones.intValue = EditorGUILayout.IntField(new GUIContent("Coil Bone Count", "The number of bones to use to create the coil"), PropCoilNumBones.intValue, GUILayout.ExpandWidth(true));
                    if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                    {
                        bRegenerateRope = true;
                    }
                }
            }
        }
        else if(PropRopeType.enumNames[PropRopeType.enumValueIndex] == UltimateRope.ERopeType.LinkedObjects.ToString())
        {
            EditorGUI.BeginChangeCheck();
            GUI.contentColor = PropRopeStart.objectReferenceValue == null ? Color.red : GUI.contentColor;
            PropRopeStart.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Rope Start", "GameObject where the rope starts"), PropRopeStart.objectReferenceValue, typeof(GameObject), true);
            GUI.contentColor = v4GUIColor;
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            GUI.contentColor = PropLinkObject.objectReferenceValue == null ? Color.red : GUI.contentColor;
            PropLinkObject.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Link Object", "GameObject to be used as link"), PropLinkObject.objectReferenceValue, typeof(GameObject), true);
            GUI.contentColor = v4GUIColor;
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropLinkAxis, new GUIContent("Link Axis", "The local axis along which the link object is placed"));
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(PropLinkOffsetObject, -1.0f, 1.0f, new GUIContent("Object Offset", "Offset of each link along its placement axis"));
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            PropLinkTwistAngleStart.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Twist Start", "Start twist angle in degrees"), PropLinkTwistAngleStart.floatValue);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            PropLinkTwistAngleIncrement.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Twist Increment", "Increment applied to each link to the twist angle in degrees"), PropLinkTwistAngleIncrement.floatValue);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }
        }
        else if(PropRopeType.enumNames[PropRopeType.enumValueIndex] == UltimateRope.ERopeType.ImportBones.ToString())
        {
            EditorGUI.BeginChangeCheck();
            GUI.contentColor = PropBoneFirst.objectReferenceValue == null ? Color.red : GUI.contentColor;
            PropBoneFirst.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("First Bone", "Start rope bone"), PropBoneFirst.objectReferenceValue, typeof(GameObject), true);
            GUI.contentColor = v4GUIColor;
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            GUI.contentColor = PropBoneLast.objectReferenceValue == null ? Color.red : GUI.contentColor;
            PropBoneLast.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Last Bone", "End rope bone"), PropBoneLast.objectReferenceValue, typeof(GameObject), true);
            GUI.contentColor = v4GUIColor;
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            PropBoneListNamesStatic.stringValue = EditorGUILayout.TextField(new GUIContent("Static Bone List", "Comma-separated indices of the bones that will be position-locked and thus not having rope physics. Ranges are also valid like this example: 0, 1, 3-6, 10"), PropBoneListNamesStatic.stringValue);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            PropBoneListNamesNoColliders.stringValue = EditorGUILayout.TextField(new GUIContent("Ignore Bone Collider List", "Comma-separated indices of the bones that will NOT have a collider. Ranges are also valid like this example: 0, 1, 3-6, 10"), PropBoneListNamesNoColliders.stringValue);
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropBoneAxis, new GUIContent("Bone Axis", "The local axis along which the bones are placed"));
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropBoneColliderType, new GUIContent("Bone Collider Type", "The type of collider to generate for the bones"));
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bSetupRopeLinks = true;
            }

            if(PropBoneColliderType.enumNames[PropBoneColliderType.enumValueIndex] != UltimateRope.EColliderType.None.ToString())
            {
                EditorGUI.BeginChangeCheck();
                PropBoneColliderDiameter.floatValue = EditorGUILayout.FloatField(new GUIContent("Bone Collider Diameter", "Rope's section diameter"), PropBoneColliderDiameter.floatValue);
                if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                {
                    bSetupRopeLinks = true;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.IntSlider(PropBoneColliderSkip, 0, 10, new GUIContent("Bone Collider Skip", "Skips collider generation each n links to avoid undesired inter-link collisions and/or speed up collision physics"));
                if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                {
                    bSetupRopeLinks = true;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Slider(PropBoneColliderLength, 0.01f, 5.0f, new GUIContent("Bone Collider Length", "Adjusts the bone collider length"));
                if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                {
                    bSetupRopeLinks = true;
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Slider(PropBoneColliderOffset, -1.0f, 1.0f, new GUIContent("Bone Collider Offset", "Adjusts the bone collider position offset"));
                if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                {
                    bSetupRopeLinks = true;
                }
            }
        }

        EditorGUILayout.Space();

        int nUp   = -1;
        int nDown = -1;
        int nAdd  = -1;
        int nDel  = -1;

        bool bHasRopeNodes = false;

        if(rope.RopeNodes != null)
        {
            if(rope.RopeNodes.Count != 0)
            {
                bHasRopeNodes = true;
            }
        }

        if(bHasRopeNodes == false)
        {
            rope.RopeNodes = new List<UltimateRope.RopeNode>();
            rope.RopeNodes.Add(new UltimateRope.RopeNode());

            if(rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }
        }

        if(PropRopeType.enumNames[PropRopeType.enumValueIndex] == UltimateRope.ERopeType.Procedural.ToString() || PropRopeType.enumNames[PropRopeType.enumValueIndex] == UltimateRope.ERopeType.LinkedObjects.ToString())
        {
            for(int nNode = 0; nNode < rope.RopeNodes.Count; nNode++)
            {
                if(rope.RopeNodes[nNode].bIsCoil == false)
                {
                    rope.RopeNodes[nNode].bFold = EditorGUILayout.Foldout(rope.RopeNodes[nNode].bFold, "Rope segment " + nNode);

                    if(rope.RopeNodes[nNode].bFold)
                    {   
                        int nButtonWidth = 60;

                        bool bUpEnabled   = rope.FirstNodeIsCoil() ? nNode > 1 : nNode > 0;
                        bool bDownEnabled = nNode < rope.RopeNodes.Count - 1;
                        bool bDelEnabled  = rope.FirstNodeIsCoil() ? rope.RopeNodes.Count > 2 : rope.RopeNodes.Count > 1;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(40));
                        GUI.enabled = bUpEnabled;
                        if(GUILayout.Button(new GUIContent("Up"),   GUILayout.Width(nButtonWidth))) { nUp   = nNode; }
                        GUI.enabled = bDownEnabled;
                        if(GUILayout.Button(new GUIContent("Down"), GUILayout.Width(nButtonWidth))) { nDown = nNode; }
                        GUI.enabled = true;
                        if(GUILayout.Button(new GUIContent("Add"),  GUILayout.Width(nButtonWidth))) { nAdd  = nNode; }
                        GUI.enabled = bDelEnabled;
                        if(GUILayout.Button(new GUIContent("Del"),  GUILayout.Width(nButtonWidth))) { nDel  = nNode; }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        EditorGUI.BeginChangeCheck();
                        GUI.contentColor = rope.RopeNodes[nNode].goNode == null ? Color.red : GUI.contentColor;
                        rope.RopeNodes[nNode].goNode = EditorGUILayout.ObjectField(new GUIContent("    Segment End", "GameObject where the rope segment ends"), rope.RopeNodes[nNode].goNode, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                        GUI.contentColor = v4GUIColor;
                        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                        {
                            bRegenerateRope = true;
                        }
  
                        EditorGUI.BeginChangeCheck();
                        rope.RopeNodes[nNode].fLength = EditorGUILayout.FloatField(new GUIContent("    Length", "Length of the rope segment"), rope.RopeNodes[nNode].fLength, GUILayout.ExpandWidth(true));
                        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                        {
                            bRegenerateRope = true;
                        }

                        EditorGUI.BeginChangeCheck();
                        rope.RopeNodes[nNode].nNumLinks = EditorGUILayout.IntField(new GUIContent("    Num Links", "The number of links to use to simulate the rope segment."), rope.RopeNodes[nNode].nNumLinks, GUILayout.ExpandWidth(true));
                        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                        {
                            bRegenerateRope = true;
                        }

                        EditorGUI.BeginChangeCheck();
                        rope.RopeNodes[nNode].eColliderType = (UltimateRope.EColliderType)EditorGUILayout.EnumPopup(new GUIContent("    Collider Type", "The type of collider to generate for the links, or no colliders"), rope.RopeNodes[nNode].eColliderType, GUILayout.ExpandWidth(true));
                        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                        {
                            bSetupRopeLinks = true;
                        }

                        if(rope.RopeNodes[nNode].eColliderType != UltimateRope.EColliderType.None)
                        {
                            EditorGUI.BeginChangeCheck();
                            rope.RopeNodes[nNode].nColliderSkip = EditorGUILayout.IntSlider(new GUIContent("    Link Collider Skip", "Skips collider generation each n links to avoid undesired inter-link collisions and/or speed up collision physics"), rope.RopeNodes[nNode].nColliderSkip, 0, 10);
                            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
                            {
                                bSetupRopeLinks = true;
                            }
                        }
                    }
                }
            }

            if(nUp != -1)
            {
                rope.MoveNodeUp(nUp);
                if(rope.AutoRegenerate) bRegenerateRope = true;
            }
            else if(nDown != -1)
            {
                rope.MoveNodeDown(nDown);
                if(rope.AutoRegenerate) bRegenerateRope = true;
            }
            else if(nAdd != -1)
            {
                rope.CreateNewNode(nAdd);
                if(rope.AutoRegenerate) bRegenerateRope = true;
            }
            else if(nDel != -1)
            {
                rope.RemoveNode(nDel);
                if(rope.AutoRegenerate) bRegenerateRope = true;
            }
        
            EditorGUILayout.Space();
        }

        EditorGUI.BeginChangeCheck();
        PropLinkMass.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Mass", "Mass assigned to each link"), PropLinkMass.floatValue);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeLinks = true;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.IntSlider(PropLinkSolverIterationCount, 1, 255, new GUIContent("Link Solver Iterations", "Set to larger values for more physic steps at the cost of more computational power"));
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeLinks = true;
        }
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Slider(PropLinkJointAngularXLimit, 0.0f, 180.0f, new GUIContent("Link Joint Angular X Limit", "Angular limit for link joints in the x asis"));
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Slider(PropLinkJointAngularYLimit, 0.0f, 180.0f, new GUIContent("Link Joint Angular Y Limit", "Angular limit for link joints in the y asis"));
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Slider(PropLinkJointAngularZLimit, 0.0f, 180.0f, new GUIContent("Link Joint Angular Z Limit", "Angular limit for link joints in the z asis"));
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }

        EditorGUI.BeginChangeCheck();
        PropLinkJointSpringValue.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Joint Spring", "Spring value assigned to each link joint"), PropLinkJointSpringValue.floatValue);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }

        EditorGUI.BeginChangeCheck();
        PropLinkJointDamperValue.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Joint Damper", "Damper value assigned to each link joint"), PropLinkJointDamperValue.floatValue);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }

        EditorGUI.BeginChangeCheck();
        PropLinkJointMaxForceValue.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Joint Max Force", "Max Force value assigned to each link joint"), PropLinkJointMaxForceValue.floatValue);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }

        EditorGUI.BeginChangeCheck();
        PropLinkJointBreakForce.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Joint Break Force", "Force needed to break a link joint (Infinity for non breakable)"), PropLinkJointBreakForce.floatValue);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }

        EditorGUI.BeginChangeCheck();
        PropLinkJointBreakTorque.floatValue = EditorGUILayout.FloatField(new GUIContent("Link Joint Break Torque", "Torque needed to break a link joint (Infinity for non breakable)"), PropLinkJointBreakTorque.floatValue);
        if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
        {
            bSetupRopeJoints = true;
        }

        bool bIsRopeBreakableNow = PropLinkJointBreakTorque.floatValue != Mathf.Infinity || PropLinkJointBreakForce.floatValue != Mathf.Infinity;

        if(bIsRopeBreakable != bIsRopeBreakableNow)
        {
            bChangeRopeSegmentSides = true; // Changing rope segment sides forces a new mesh
        }

        if(PropRopeType.enumNames[PropRopeType.enumValueIndex] == UltimateRope.ERopeType.Procedural.ToString())
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(PropLockStartEndInZAxis, new GUIContent("Lock Start/End ZAxis", "For each segment, makes the first link and last link to mantain fixed position (they will be aligned in the Z-Axis of each start/end node)"), GUILayout.ExpandWidth(true));
            if(EditorGUI.EndChangeCheck() && rope.AutoRegenerate)
            {
                bRegenerateRope = true;
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(PropSendEvents, new GUIContent("Send Rope Events", "Send event messages?"), GUILayout.ExpandWidth(true));

        if(PropSendEvents.boolValue)
        {
            EditorGUILayout.PropertyField(PropEventsObjectReceiver, new GUIContent("Events Object Receiver", "GameObject whose methods will be called when events are triggered"), GUILayout.ExpandWidth(true));
            EditorGUILayout.PropertyField(PropOnBreakMethodName, new GUIContent("Break Event Method", "Name of the method that will be called when a rope link breaks"), GUILayout.ExpandWidth(true));
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(PropPersistAfterPlayMode, new GUIContent("Persist after playmode", "Should the rope component keep its state after exiting playmode?"), GUILayout.ExpandWidth(true));

/*
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(PropAutoRegenerate, new GUIContent("Auto regenerate", "Should the rope be regenerated automatically after editor changes?"), GUILayout.ExpandWidth(true));
        if(EditorGUI.EndChangeCheck() && PropAutoRegenerate.boolValue)
        {
            bRegenerateRope = true;
        }
*/
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button(new GUIContent("Reset rope")))
        {
            bRegenerateRope     = true;
            bResetNodePositions = rope.HasDynamicSegmentNodes();
        }

        if(GUILayout.Button(new GUIContent("Delete rope")))
        {
            bDeleteRope = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if(GUILayout.Button(new GUIContent("Convert to static mesh")))
        {
            bMakeStatic = true;
        }

        // Apply changes to the serializedProperty

        serializedObject.ApplyModifiedProperties();

        // Perform action at the end

        if(bDeleteRope)
        {
            rope.DeleteRope();
        }

        if(bRegenerateRope)
        {
            rope.Regenerate(bResetNodePositions);
        }
        else
        {
            if(bChangeRopeDiameter)
            {
              rope.ChangeRopeDiameter(PropRopeDiameter.floatValue, PropRopeDiameterScaleX.floatValue, PropRopeDiameterScaleY.floatValue);
            }

            if(bChangeRopeSegmentSides)
            {
                rope.ChangeRopeSegmentSides(PropRopeSegmentSides.intValue);
            }

            if(bSetupRopeLinks)
            {
                rope.SetupRopeLinks();
            }

            if(bSetupRopeMaterials)
            {
                rope.SetupRopeMaterials();
            }

            if(bSetupRopeJoints)
            {
                rope.SetupRopeJoints();
            }

            if(bRecomputeCoil)
            {
                rope.RecomputeCoil();
            }
        }

        if(bMakeStatic)
        {
            if(Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Rope can't be made static from within the play mode, please use edit mode instead. Current rope state can be saved using the Persist After Playmode checkbox if needed.", "OK");
            }
            else if(EditorUtility.DisplayDialog("Are you sure?", "A new static GameObject will be created and the current will be hidden but not deleted in case the result is not satisfactory. To unhide it use the checkbox left to the name in the inspector.\nFor some ropes it will take some time to create the static object, since lightmapping coordinates will be computed if the object doesn't have them.", "Make static", "Cancel") == true)
            {
                string strStatus;

                GameObject newObject = rope.BuildStaticMeshObject(out strStatus);

                if(newObject == null)
                {
                    EditorUtility.DisplayDialog("Error", strStatus, "OK");
                }
                else
                {
                    UnityEditor.Selection.activeGameObject = newObject;
                }
            }
        }
    }
}

