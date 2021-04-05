using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class RopePersistAttribute : System.Attribute
{

}

public static class RopePersistManager
{
    static RopePersistManager()
    {
        s_hashInstanceID2RopeData = new Dictionary<int, RopeData>();
    }

    private class RopeData
    {
        public class TransformInfo
        {
            public GameObject goObject;
            public string     strObjectName;
            public Transform  tfParent;
            public Vector3    v3LocalPosition;
            public Quaternion qLocalOrientation;
            public Vector3    v3LocalScale;
            public bool       bLinkMarkedKinematic;
            public bool       bExtensibleKinematic;
        }

        public RopeData(UltimateRope rope)
        {
            m_rope                  = rope;
            m_hashFieldName2Value   = new Dictionary<string, object>();
            m_aLinkTransformInfo    = new TransformInfo[rope.TotalLinks];
            m_transformInfoSegments = new TransformInfo[rope.RopeNodes.Count];

            m_bSkin = rope.GetComponent<SkinnedMeshRenderer>() != null;

            if(m_bSkin)
            {
                SkinnedMeshRenderer skin = rope.GetComponent<SkinnedMeshRenderer>();

                Mesh skinMesh = skin.sharedMesh;

                int nVertices          = skin.sharedMesh.vertexCount;
                int nTrianglesRope     = skin.sharedMesh.GetTriangles(0).Length;
                int nTrianglesSections = skin.sharedMesh.GetTriangles(1).Length;

                m_av3SkinVertices         = new Vector3   [nVertices];
                m_av2SkinMapping          = new Vector2   [nVertices];
                m_av4SkinTangents         = skinMesh.tangents != null ? new Vector4[skinMesh.tangents.Length] : null;
                m_aSkinBoneWeights        = new BoneWeight[nVertices];
                m_anSkinTrianglesRope     = new int       [nTrianglesRope];
                m_anSkinTrianglesSections = new int       [nTrianglesSections];
                m_amtxSkinBindPoses       = new Matrix4x4 [skin.sharedMesh.bindposes.Length];

                MakeSkinDeepCopy(skinMesh.vertices, skinMesh.uv,      skinMesh.tangents, skinMesh.boneWeights, skinMesh.GetTriangles(0), skinMesh.GetTriangles(1),  skinMesh.bindposes,
                                 m_av3SkinVertices, m_av2SkinMapping, m_av4SkinTangents, m_aSkinBoneWeights,   m_anSkinTrianglesRope,    m_anSkinTrianglesSections, m_amtxSkinBindPoses);
            }
        }

        public static void MakeSkinDeepCopy(Vector3[] av3VerticesSource,  Vector2[] av2MappingSource,  Vector4[] av4TangentsSource,  BoneWeight[] aBoneWeightsSource,  int[] anTrianglesRopeSource,  int[] anTrianglesSectionsSource,  Matrix4x4[] aBindPosesSource, 
                                            Vector3[] av3VerticesDestiny, Vector2[] av2MappingDestiny, Vector4[] av4TangentsDestiny, BoneWeight[] aBoneWeightsDestiny, int[] anTrianglesRopeDestiny, int[] anTrianglesSectionsDestiny, Matrix4x4[] aBindPosesDestiny)
        {
            int nVertices = av3VerticesSource.Length;

            for(int nVertex = 0; nVertex < nVertices; nVertex++)
            {
                av3VerticesDestiny [nVertex] = av3VerticesSource [nVertex];
                av2MappingDestiny  [nVertex] = av2MappingSource  [nVertex];

                if(av4TangentsDestiny != null && av4TangentsSource != null)
                {
                    if(av4TangentsDestiny.Length == nVertices && av4TangentsSource.Length == nVertices)
                    {
                        av4TangentsDestiny[nVertex] = av4TangentsSource[nVertex];
                    }
                }

                aBoneWeightsDestiny[nVertex] = aBoneWeightsSource[nVertex];
            }

            for(int nIndex = 0; nIndex < anTrianglesRopeDestiny.Length; nIndex++)
            {
                anTrianglesRopeDestiny[nIndex] = anTrianglesRopeSource[nIndex];
            }

            for(int nIndex = 0; nIndex < anTrianglesSectionsDestiny.Length; nIndex++)
            {
                anTrianglesSectionsDestiny[nIndex] = anTrianglesSectionsSource[nIndex];
            }

            for(int nIndex = 0; nIndex < aBindPosesSource.Length; nIndex++)
            {
                aBindPosesDestiny[nIndex] = aBindPosesSource[nIndex];
            }
        }

        public UltimateRope                 m_rope;
        public bool                         m_bDeleted;
        public Dictionary <string, object>  m_hashFieldName2Value;
        public bool                         m_bSkin;
        public Vector3[]                    m_av3SkinVertices;
        public Vector2[]                    m_av2SkinMapping;
        public Vector4[]                    m_av4SkinTangents;
        public BoneWeight[]                 m_aSkinBoneWeights;
        public int[]                        m_anSkinTrianglesRope;
        public int[]                        m_anSkinTrianglesSections;
        public Matrix4x4[]                  m_amtxSkinBindPoses;
        public TransformInfo                m_transformInfoRope;
        public TransformInfo[]              m_aLinkTransformInfo;
        public TransformInfo                m_transformInfoStart;
        public TransformInfo[]              m_transformInfoSegments;
        public bool[][]                     m_aaJointsProcessed;
        public bool[][]                     m_aaJointsBroken;
    }

    public static void StorePersistentData(UltimateRope rope)
    {
        RopeData ropeData = new RopeData(rope);

        // Attributes

        foreach(FieldInfo fieldInfo in rope.GetType().GetFields())
        {
            if(Attribute.IsDefined(fieldInfo, typeof(RopePersistAttribute)))
            {
                ropeData.m_hashFieldName2Value.Add(fieldInfo.Name, fieldInfo.GetValue(rope));
            }
        }

        // Physics

        if(rope.Deleted)
        {
            ropeData.m_bDeleted = true;
        }
        else
        {
            ropeData.m_aaJointsBroken    = new bool[rope.RopeNodes.Count][];
            ropeData.m_aaJointsProcessed = new bool[rope.RopeNodes.Count][];

            ropeData.m_transformInfoRope = ComputeTransformInfo(rope, rope.gameObject, rope.transform.parent != null ? rope.transform.parent.gameObject : null);

            if(rope.RopeStart != null)
            {
                ropeData.m_transformInfoStart = ComputeTransformInfo(rope, rope.RopeStart, rope.RopeStart.transform.parent != null ? rope.RopeStart.transform.parent.gameObject : null);
            }

            int nLinearLinkIndex = 0;

            for(int nNode = 0; nNode < rope.RopeNodes.Count; nNode++)
            {
                if(rope.RopeNodes[nNode].goNode != null)
                {
                    ropeData.m_transformInfoSegments[nNode] = ComputeTransformInfo(rope, rope.RopeNodes[nNode].goNode, rope.RopeNodes[nNode].goNode.transform.parent != null ? rope.RopeNodes[nNode].goNode.transform.parent.gameObject : null);
                }

                foreach(GameObject link in rope.RopeNodes[nNode].segmentLinks)
                {
                    ropeData.m_aLinkTransformInfo[nLinearLinkIndex] = ComputeTransformInfo(rope, link, rope.RopeType == UltimateRope.ERopeType.ImportBones ? rope.ImportedBones[nLinearLinkIndex].tfNonBoneParent.gameObject : rope.RopeNodes[nNode].goNode.transform.gameObject);
                    nLinearLinkIndex++;
                }

                ropeData.m_aaJointsBroken[nNode]    = new bool[rope.RopeNodes[nNode].linkJoints.Length];
                ropeData.m_aaJointsProcessed[nNode] = new bool[rope.RopeNodes[nNode].linkJointBreaksProcessed.Length];

                for(int nJoint = 0; nJoint < rope.RopeNodes[nNode].linkJoints.Length; nJoint++)
                {
                    ropeData.m_aaJointsBroken[nNode][nJoint] = rope.RopeNodes[nNode].linkJoints[nJoint] == null;
                }

                for(int nJoint = 0; nJoint < rope.RopeNodes[nNode].linkJoints.Length; nJoint++)
                {
                    ropeData.m_aaJointsProcessed[nNode][nJoint] = rope.RopeNodes[nNode].linkJointBreaksProcessed[nJoint];
                }
            }

            ropeData.m_bDeleted = false;
        }

        s_hashInstanceID2RopeData.Add(rope.GetInstanceID(), ropeData);
    }

    public static void RetrievePersistentData(UltimateRope rope)
    {
        RopeData ropeData = s_hashInstanceID2RopeData[rope.GetInstanceID()];

        // Attributes

        foreach(FieldInfo fieldInfo in rope.GetType().GetFields())
        {
            fieldInfo.SetValue(rope, ropeData.m_hashFieldName2Value[fieldInfo.Name]);
        }

        // Physics

        if(ropeData.m_bDeleted)
        {
            rope.DeleteRope();
        }
        else
        {
            // Set rope tranform info

            SetTransformInfo(ropeData.m_transformInfoRope, rope.gameObject);

            // Set rope start transform info

            if(rope.RopeStart != null)
            {
                if(ropeData.m_transformInfoStart.goObject == null)
                {
                    rope.RopeStart = new GameObject(ropeData.m_transformInfoStart.strObjectName);
                }

                SetTransformInfo(ropeData.m_transformInfoStart, rope.RopeStart);
            }

            // Delete all dynamic links to reset them, so that even after creating/deleting/breaking links during playmode they get recreated correctly after playmode

            if(rope.RopeType != UltimateRope.ERopeType.ImportBones)
            {
                rope.DeleteRopeLinks();
            }

            int nLinearLinkIndex = 0;

            for(int nNode = 0; nNode < rope.RopeNodes.Count; nNode++)
            {
                if(rope.RopeType != UltimateRope.ERopeType.ImportBones)
                {
                    for(int nJoint = 0; nJoint < rope.RopeNodes[nNode].linkJoints.Length; nJoint++)
                    {
                        rope.RopeNodes[nNode].linkJointBreaksProcessed[nJoint] = ropeData.m_aaJointsProcessed[nNode][nJoint];
                    }

                    if(rope.RopeNodes[nNode].goNode != null)
                    {
                        if(ropeData.m_transformInfoSegments[nNode].goObject == null)
                        {
                            rope.RopeNodes[nNode].goNode = new GameObject(ropeData.m_transformInfoSegments[nNode].strObjectName);
                        }

                        SetTransformInfo(ropeData.m_transformInfoSegments[nNode], rope.RopeNodes[nNode].goNode);
                    }
                }

                if(rope.RopeType != UltimateRope.ERopeType.ImportBones)
                {
                    rope.RopeNodes[nNode].segmentLinks = new GameObject[rope.RopeNodes[nNode].nTotalLinks];
                }

                for(int nLink = 0; nLink < rope.RopeNodes[nNode].segmentLinks.Length; nLink++)
                {
                    if(rope.RopeType != UltimateRope.ERopeType.ImportBones)
                    {
                        if(rope.RopeType == UltimateRope.ERopeType.Procedural)
                        {
                            rope.RopeNodes[nNode].segmentLinks[nLink] = new GameObject(ropeData.m_aLinkTransformInfo[nLinearLinkIndex].strObjectName);
                        }
                        else if(rope.RopeType == UltimateRope.ERopeType.LinkedObjects)
                        {
                            rope.RopeNodes[nNode].segmentLinks[nLink] = GameObject.Instantiate(rope.LinkObject) as GameObject;
                            rope.RopeNodes[nNode].segmentLinks[nLink].name = ropeData.m_aLinkTransformInfo[nLinearLinkIndex].strObjectName;
                        }

                        rope.RopeNodes[nNode].segmentLinks[nLink].AddComponent<UltimateRopeLink>();
                        rope.RopeNodes[nNode].segmentLinks[nLink].transform.parent = (rope.FirstNodeIsCoil() && nNode == 0) ? rope.CoilObject.transform : rope.gameObject.transform;

                        if(rope.RopeNodes[nNode].bIsCoil == false)
                        {
                            rope.RopeNodes[nNode].segmentLinks[nLink].AddComponent<Rigidbody>();
                            rope.RopeNodes[nNode].segmentLinks[nLink].GetComponent<Rigidbody>().isKinematic = ropeData.m_aLinkTransformInfo[nLinearLinkIndex].bExtensibleKinematic || ropeData.m_aLinkTransformInfo[nLinearLinkIndex].bLinkMarkedKinematic;
                        }
                    }

                    SetTransformInfo(ropeData.m_aLinkTransformInfo[nLinearLinkIndex], rope.RopeNodes[nNode].segmentLinks[nLink]);

                    if(rope.RopeType == UltimateRope.ERopeType.ImportBones)
                    {
                        rope.RopeNodes[nNode].segmentLinks[nLink].transform.parent = rope.ImportedBones[nLinearLinkIndex].bIsStatic ? rope.ImportedBones[nLinearLinkIndex].tfNonBoneParent : rope.transform;
                    }

                    if(ropeData.m_aLinkTransformInfo[nLinearLinkIndex].bExtensibleKinematic)
                    {
                        UltimateRopeLink ropeLink = rope.RopeNodes[nNode].segmentLinks[nLink].GetComponent<UltimateRopeLink>();

                        if(ropeLink != null) ropeLink.ExtensibleKinematic = true;
                        
                        rope.RopeNodes[nNode].segmentLinks[nLink].transform.parent   = nNode > rope.m_nFirstNonCoilNode ? rope.RopeNodes[nNode - 1].goNode.transform : rope.RopeStart.transform;
                        rope.RopeNodes[nNode].segmentLinks[nLink].transform.position = rope.RopeNodes[nNode].segmentLinks[nLink].transform.parent.position;

                        Vector3 v3WorldForward = rope.RopeNodes[nNode].segmentLinks[nLink].transform.parent.TransformDirection(rope.RopeNodes[nNode].m_v3LocalDirectionForward);
                        Vector3 v3WorldUp      = rope.RopeNodes[nNode].segmentLinks[nLink].transform.parent.TransformDirection(rope.RopeNodes[nNode].m_v3LocalDirectionUp);
                        rope.RopeNodes[nNode].segmentLinks[nLink].transform.rotation = Quaternion.LookRotation(v3WorldForward, v3WorldUp);
                    }

                    nLinearLinkIndex++;
                }
            }

            rope.SetupRopeLinks();

            // Mesh

            SkinnedMeshRenderer skin = rope.GetComponent<SkinnedMeshRenderer>();

            if(ropeData.m_bSkin == false)
            {
                if(skin != null)
                {
                    UnityEngine.Object.DestroyImmediate(skin);
                }
            }
            else
            {
                if(skin == null)
                {
                    skin = rope.gameObject.AddComponent<SkinnedMeshRenderer>();
                }

                int nVertices          = ropeData.m_av3SkinVertices.Length;
                int nTrianglesRope     = ropeData.m_anSkinTrianglesRope.Length;
                int nTrianglesSections = ropeData.m_anSkinTrianglesSections.Length;

                Vector3[]    av3SkinVertices         = new Vector3   [nVertices];
                Vector2[]    av2SkinMapping          = new Vector2   [nVertices];
                Vector4[]    av4SkinTangents         = ropeData.m_av4SkinTangents != null ? new Vector4[ropeData.m_av4SkinTangents.Length] : null;
                BoneWeight[] aSkinBoneWeights        = new BoneWeight[nVertices];
                int[]        anSkinTrianglesRope     = new int       [nTrianglesRope];
                int[]        anSkinTrianglesSections = new int       [nTrianglesSections];
                Matrix4x4[]  amtxSkinBindPoses       = new Matrix4x4 [ropeData.m_amtxSkinBindPoses.Length];

                Mesh mesh = new Mesh();

                RopeData.MakeSkinDeepCopy(ropeData.m_av3SkinVertices, ropeData.m_av2SkinMapping, ropeData.m_av4SkinTangents, ropeData.m_aSkinBoneWeights, ropeData.m_anSkinTrianglesRope, ropeData.m_anSkinTrianglesSections, ropeData.m_amtxSkinBindPoses,
                                          av3SkinVertices,            av2SkinMapping,            av4SkinTangents,            aSkinBoneWeights,            anSkinTrianglesRope,            anSkinTrianglesSections,            amtxSkinBindPoses);

                Transform[] aBones = new Transform[rope.TotalLinks];

                nLinearLinkIndex = 0;

                for(int nNode = 0; nNode < rope.RopeNodes.Count; nNode++)
                {
                    for(int nLink = 0; nLink < rope.RopeNodes[nNode].segmentLinks.Length; nLink++)
                    {
                        aBones[nLinearLinkIndex++] = rope.RopeNodes[nNode].segmentLinks[nLink].transform;
                    }
                }

                mesh.vertices    = av3SkinVertices;
                mesh.uv          = av2SkinMapping;
                mesh.boneWeights = aSkinBoneWeights;
                mesh.bindposes   = amtxSkinBindPoses;

                mesh.subMeshCount = 2;
                mesh.SetTriangles(anSkinTrianglesRope,     0);
                mesh.SetTriangles(anSkinTrianglesSections, 1);
                mesh.RecalculateNormals();

                if(av4SkinTangents != null)
                {
                    if(av4SkinTangents.Length == nVertices)
                    {
                        mesh.tangents = av4SkinTangents;
                    }
                }

                skin.bones      = aBones;
                skin.sharedMesh = mesh;

                Material[] ropeMaterials = new Material[2];
                ropeMaterials[0] = rope.RopeMaterial;
                ropeMaterials[1] = rope.RopeSectionMaterial;

                skin.materials  = ropeMaterials;
            }
        }
    }

    public static bool PersistentDataExists(UltimateRope rope)
    {
        return s_hashInstanceID2RopeData.ContainsKey(rope.GetInstanceID());
    }

    public static void RemovePersistentData(UltimateRope rope)
    {
        s_hashInstanceID2RopeData.Remove(rope.GetInstanceID());
    }

    private static RopeData.TransformInfo ComputeTransformInfo(UltimateRope rope, GameObject node, GameObject parent)
    {
        RopeData.TransformInfo transformInfo = new RopeData.TransformInfo();

        transformInfo.goObject      = node;
        transformInfo.strObjectName = node.name;

        transformInfo.tfParent = parent == null ? null : parent.transform;

        if(transformInfo.tfParent != null)
        {
            transformInfo.v3LocalPosition   = transformInfo.tfParent.InverseTransformPoint(node.transform.position);
            transformInfo.qLocalOrientation = Quaternion.Inverse(transformInfo.tfParent.rotation) * node.transform.rotation;
        }
        else
        {
            transformInfo.v3LocalPosition   = node.transform.position;
            transformInfo.qLocalOrientation = node.transform.rotation;
        }

        transformInfo.v3LocalScale = node.transform.localScale;

        UltimateRopeLink ropeLink = node.GetComponent<UltimateRopeLink>();

        if(ropeLink != null)
        {
            transformInfo.bExtensibleKinematic = ropeLink.ExtensibleKinematic;
            transformInfo.bLinkMarkedKinematic = node.GetComponent<Rigidbody>() != null ? node.GetComponent<Rigidbody>().isKinematic : false;
        }
        else
        {
            transformInfo.bExtensibleKinematic = false;
            transformInfo.bLinkMarkedKinematic = false;
        }

        return transformInfo;
    }

    private static void SetTransformInfo(RopeData.TransformInfo transformInfo, GameObject node)
    {
        if(transformInfo.tfParent != null)
        {
            node.transform.position = transformInfo.tfParent.TransformPoint(transformInfo.v3LocalPosition);
            node.transform.rotation = transformInfo.tfParent.rotation * transformInfo.qLocalOrientation;
        }
        else
        {
            node.transform.position = transformInfo.v3LocalPosition;
            node.transform.rotation = transformInfo.qLocalOrientation;
        }

        node.transform.localScale = transformInfo.v3LocalScale;
    }

    static private Dictionary<int, RopeData> s_hashInstanceID2RopeData;
}

[ExecuteInEditMode]
public class UltimateRope : MonoBehaviour
{
                            // Global parameters
    [RopePersistAttribute]  public ERopeType            RopeType                        = ERopeType.Procedural;
    
    [RopePersistAttribute]  public GameObject           RopeStart;
    [RopePersistAttribute]  public List<RopeNode>       RopeNodes;
    [RopePersistAttribute]  public int                  RopeLayer;
    [RopePersistAttribute]  public PhysicMaterial       RopePhysicsMaterial;

                            // Procedural specific parameters
    [RopePersistAttribute]  public float                RopeDiameter                    = 0.1f;
    [RopePersistAttribute]  public float                RopeDiameterScaleX              = 1.0f;
    [RopePersistAttribute]  public float                RopeDiameterScaleY              = 1.0f;
    [RopePersistAttribute]  public int                  RopeSegmentSides                = 8;
    [RopePersistAttribute]  public Material             RopeMaterial;
    [RopePersistAttribute]  public float                RopeTextureTileMeters           = 1.0f;
    [RopePersistAttribute]  public Material             RopeSectionMaterial;
    [RopePersistAttribute]  public float                RopeTextureSectionTileMeters    = 1.0f;

                            // Extensible rope parameters
    [RopePersistAttribute]  public bool                 IsExtensible                    = false;
    [RopePersistAttribute]  public float                ExtensibleLength                = 10.0f;
    [RopePersistAttribute]  public bool                 HasACoil                        = false;
    [RopePersistAttribute]  public GameObject           CoilObject;
    [RopePersistAttribute]  public EAxis                CoilAxisRight                   = EAxis.X;
    [RopePersistAttribute]  public EAxis                CoilAxisUp                      = EAxis.Y;
    [RopePersistAttribute]  public float                CoilWidth                       = 0.5f;
    [RopePersistAttribute]  public float                CoilDiameter                    = 0.5f;
    [RopePersistAttribute]  public int                  CoilNumBones                    = 50;

                            // LinkedObjects specific parameters
    [RopePersistAttribute]  public GameObject           LinkObject;
    [RopePersistAttribute]  public EAxis                LinkAxis                        = EAxis.Z;
    [RopePersistAttribute]  public float                LinkOffsetObject                = 0.0f;
    [RopePersistAttribute]  public float                LinkTwistAngleStart             = 0.0f;
    [RopePersistAttribute]  public float                LinkTwistAngleIncrement         = 0.0f;

                            // ImportBones specific parameters
    [RopePersistAttribute]  public GameObject           BoneFirst;
    [RopePersistAttribute]  public GameObject           BoneLast;
    [RopePersistAttribute]  public string               BoneListNamesStatic;
    [RopePersistAttribute]  public string               BoneListNamesNoColliders;
    [RopePersistAttribute]  public EAxis                BoneAxis                        = EAxis.Z;
    [RopePersistAttribute]  public EColliderType        BoneColliderType                = EColliderType.Capsule;
    [RopePersistAttribute]  public float                BoneColliderDiameter            = 0.1f;
    [RopePersistAttribute]  public int                  BoneColliderSkip                = 0;
    [RopePersistAttribute]  public float                BoneColliderLength              = 1.0f;
    [RopePersistAttribute]  public float                BoneColliderOffset              = 0.0f;

                            // Global link parameters
    [RopePersistAttribute]  public float                LinkMass                        = 1.0f;
    [RopePersistAttribute]  public int                  LinkSolverIterationCount        = 100;
    [RopePersistAttribute]  public float                LinkJointAngularXLimit          = 30.0f;
    [RopePersistAttribute]  public float                LinkJointAngularYLimit          = 30.0f;
    [RopePersistAttribute]  public float                LinkJointAngularZLimit          = 30.0f;
    [RopePersistAttribute]  public float                LinkJointSpringValue            = 1.0f;
    [RopePersistAttribute]  public float                LinkJointDamperValue            = 0.0f;
    [RopePersistAttribute]  public float                LinkJointMaxForceValue          = 1.0f;
    [RopePersistAttribute]  public float                LinkJointBreakForce             = Mathf.Infinity;
    [RopePersistAttribute]  public float                LinkJointBreakTorque            = Mathf.Infinity;
    [RopePersistAttribute]  public bool                 LockStartEndInZAxis             = false;

    [RopePersistAttribute]  public bool                 SendEvents                      = false;
    [RopePersistAttribute]  public GameObject           EventsObjectReceiver;
    [RopePersistAttribute]  public string               OnBreakMethodName;

    [RopePersistAttribute]  public bool                 PersistAfterPlayMode            = false;
    [RopePersistAttribute]  public bool                 EnablePrefabUsage               = false;
    [RopePersistAttribute]  public bool                 AutoRegenerate                  = true;

    [HideInInspector]       public string               Status { get { return m_strStatus; } set { /* Debug.Log(value); */ m_strStatus = value; } }

    [HideInInspector, RopePersistAttribute] public bool                Deleted                         = true;
    [HideInInspector, RopePersistAttribute] public float[]             LinkLengths;
    [HideInInspector, RopePersistAttribute] public int                 TotalLinks                      = 0;
    [HideInInspector, RopePersistAttribute] public float               TotalRopeLength                 = 0.0f;
    [HideInInspector, RopePersistAttribute] public bool                m_bRopeStartInitialOrientationInitialized = false;
    [HideInInspector, RopePersistAttribute] public Vector3             m_v3InitialRopeStartLocalPos;
    [HideInInspector, RopePersistAttribute] public Quaternion          m_qInitialRopeStartLocalRot;
    [HideInInspector, RopePersistAttribute] public Vector3             m_v3InitialRopeStartLocalScale;
    [HideInInspector, RopePersistAttribute] public int                 m_nFirstNonCoilNode             = 0;
    [HideInInspector, RopePersistAttribute] public float[]             m_afCoilBoneRadiuses            = null;
    [HideInInspector, RopePersistAttribute] public float[]             m_afCoilBoneAngles              = null;
    [HideInInspector, RopePersistAttribute] public float[]             m_afCoilBoneX                   = null;
    [HideInInspector, RopePersistAttribute] public float               m_fCurrentCoilRopeRadius        = 0.0f;
    [HideInInspector, RopePersistAttribute] public float               m_fCurrentCoilTurnsLeft         = 0.0f;
    [HideInInspector, RopePersistAttribute] public float               m_fCurrentCoilLength            = 0.0f;
    [HideInInspector, RopePersistAttribute] public float               m_fCurrentExtension             = 0.0f;
    [HideInInspector, RopePersistAttribute] public float               m_fCurrentExtensionInput        = 0.0f;
    [HideInInspector, RopePersistAttribute] public RopeBone[]          ImportedBones;
    [HideInInspector, RopePersistAttribute] public bool                m_bBonesAreImported             = false;
    [HideInInspector, RopePersistAttribute] public string              m_strStatus;
    [HideInInspector, RopePersistAttribute] public bool                m_bLastStatusIsError            = true;
    [HideInInspector, RopePersistAttribute] public string              m_strAssetFile                  = "";

    public enum ERopeType
    {
        Procedural,
        LinkedObjects,
        ImportBones
    }

    public enum EAxis
    {
        MinusX,
        MinusY,
        MinusZ,
        X,
        Y,
        Z
    };

    public enum EColliderType
    {
        None,
        Capsule,
        Box
    };

    public enum ERopeExtensionMode
    {
        CoilRotationIncrement,
        LinearExtensionIncrement
    };

    [Serializable]
    public class RopeNode
    {
        public RopeNode()
        {
            goNode        = null;
            fLength       = 5.0f;
            fTotalLength  = fLength;
            nNumLinks     = 20;
            nTotalLinks   = nNumLinks;
            eColliderType = EColliderType.Capsule;
            nColliderSkip = 1;
            bFold         = true;
            bIsCoil       = false;

            bInitialOrientationInitialized = false;

            linkJoints               = new ConfigurableJoint[0];
            linkJointBreaksProcessed = new bool[0];
            bSegmentBroken           = false;
        }

        public GameObject       goNode;
        public float            fLength;
        public float            fTotalLength; // Including extension length
        public int              nNumLinks;
        public int              nTotalLinks;  // Includinig extension links
        public EColliderType    eColliderType;
        public int              nColliderSkip;
        public bool             bFold;
        public bool             bIsCoil;

        public bool             bInitialOrientationInitialized;
        public Vector3          v3InitialLocalPos;
        public Quaternion       qInitialLocalRot;
        public Vector3          v3InitialLocalScale;

        public bool             m_bExtensionInitialized;
        public int              m_nExtensionLinkIn;
        public int              m_nExtensionLinkOut;
        public float            m_fExtensionRemainingLength;
        public float            m_fExtensionRemainderIn;
        public float            m_fExtensionRemainderOut;
        public Vector3          m_v3LocalDirectionForward;
        public Vector3          m_v3LocalDirectionUp;

        public GameObject[]        segmentLinks;
        public ConfigurableJoint[] linkJoints;
        public bool[]              linkJointBreaksProcessed;
        public bool                bSegmentBroken;
    }

    [Serializable]
    public class RopeBone
    {
        public RopeBone()
        {
            goBone            = null;
            tfParent          = null;
            tfNonBoneParent   = null;
            bCreatedCollider  = false;
            bIsStatic         = false;
            fLength           = 0;
            bCreatedRigidbody = false;
            nOriginalLayer    = 0;
        }

        public GameObject   goBone;
        public Transform    tfParent;
        public Transform    tfNonBoneParent;
        public bool         bCreatedCollider;
        public bool         bIsStatic;
        public float        fLength;
        public bool         bCreatedRigidbody;
        public int          nOriginalLayer;
        public Vector3      v3OriginalLocalScale;
        public Vector3      v3OriginalLocalPos;
        public Quaternion   qOriginalLocalRot;
    }

    public class RopeBreakEventInfo
    {
        public UltimateRope rope;
        public GameObject   link1;
        public GameObject   link2;
        public Vector3      worldPos;
        public Vector3      localLink1Pos;
        public Vector3      localLink2Pos;
    };

    void Awake()
    {
        if(Application.isPlaying == true)
        {
            CreateRopeJoints(true);
            SetupRopeLinks();

            if(FirstNodeIsCoil())
            {
                RecomputeCoil();
            }
        }
        else
        {
            // Called right after playmode stop
            CheckLoadPersistentData();
        }
   }

    void OnApplicationQuit()
    {
        CheckSavePersistentData();
    }

    void Start()
    {
        m_fCurrentExtensionInput = m_fCurrentExtension;
    }

    void OnGUI()
    {/*
        if(FirstNodeIsCoil())
        {
            m_fCurrentExtensionInput = GUILayout.HorizontalSlider(m_fCurrentExtensionInput, 0.0f, ExtensibleLength, GUILayout.Width(400));
            ExtendRope(ERopeExtensionMode.LinearExtensionIncrement, m_fCurrentExtensionInput - m_fCurrentExtension);
        }*/
    }

    void Update()
    {

    }
    
    void FixedUpdate()
    {
        if(RopeNodes == null) return;
        if(RopeNodes.Count == 0) return;

        int nNodeSnapped = -1;

        if(RopeType == ERopeType.Procedural && (LinkJointBreakForce != Mathf.Infinity || LinkJointBreakTorque != Mathf.Infinity))
        {
            SkinnedMeshRenderer skin = gameObject.GetComponent<SkinnedMeshRenderer>();
            
            if(skin == null) return;
            
            Mesh newMesh = skin.sharedMesh;
            int[] trianglesRope     = newMesh.GetTriangles(0);
            int[] trianglesSections = newMesh.GetTriangles(1);
            
            int nLinearLinkIndex = 0;
    
            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                if(node.bIsCoil)
                {
                    nLinearLinkIndex += node.segmentLinks.Length;
                    continue;
                }
    
                for(int nJoint = 0; nJoint < node.linkJoints.Length; nJoint++)
                {
                    if(node.linkJoints[nJoint] == null && node.linkJointBreaksProcessed[nJoint] == false)
                    {
                        node.linkJointBreaksProcessed[nJoint] = true;

                        bool bIsFirst = nNode == 0 && nJoint == 0 && FirstNodeIsCoil() == false;
                        bool bIsLast  = nNode == RopeNodes.Count - 1 && nJoint == node.linkJoints.Length - 1;
                        
                        if(bIsFirst == false && bIsLast == false)
                        {
                            FillLinkMeshIndicesRope    (nLinearLinkIndex - 1, TotalLinks, ref trianglesRope,     true, true);
                            FillLinkMeshIndicesSections(nLinearLinkIndex - 1, TotalLinks, ref trianglesSections, true, true);
                            nNodeSnapped = nNode;
                        }

                        if(SendEvents)
                        {
                            if(EventsObjectReceiver != null && OnBreakMethodName.Length > 0)
                            {
                                RopeBreakEventInfo breakInfo = new RopeBreakEventInfo();
                                breakInfo.rope     = this;
                                breakInfo.worldPos = nJoint == node.linkJoints.Length - 1 ? node.goNode.transform.position : node.segmentLinks[nJoint].transform.position;
                                breakInfo.link2    = nJoint == node.linkJoints.Length - 1 ? node.goNode : node.segmentLinks[nJoint];
                                breakInfo.localLink2Pos = Vector3.zero;

                                if(bIsFirst)
                                {
                                    breakInfo.link1 = RopeStart.gameObject;
                                    breakInfo.localLink1Pos = Vector3.zero;
                                }
                                else
                                {
                                    if(nJoint > 0)
                                    {
                                        breakInfo.link1 = node.segmentLinks[nJoint - 1];
                                    }
                                    else
                                    {
                                        breakInfo.link1 = RopeNodes[nNode - 1].goNode;
                                    }

                                    breakInfo.localLink1Pos = GetLinkAxisOffset(LinkLengths[nLinearLinkIndex - 1]);
                                }

                                EventsObjectReceiver.SendMessage(OnBreakMethodName, breakInfo);
                            }
                        }
                    }
                    
                    if(nJoint < node.segmentLinks.Length)
                    {
                        nLinearLinkIndex++;
                    }
                }
            }
            
            if(nNodeSnapped != -1)
            {
                newMesh.SetTriangles(trianglesRope,     0);
                newMesh.SetTriangles(trianglesSections, 1);

                Vector4[] av4Tangents = null;

                if(newMesh.tangents != null)
                {
                    if(newMesh.tangents.Length == newMesh.vertexCount)
                    {
                        av4Tangents = newMesh.tangents;
                    }
                }

                newMesh.RecalculateNormals();

                if(av4Tangents != null)
                {
                    newMesh.tangents = av4Tangents;
                }
            }
        }
        else if(RopeType == ERopeType.LinkedObjects && (LinkJointBreakForce != Mathf.Infinity || LinkJointBreakTorque != Mathf.Infinity) && SendEvents)
        {
            int nLinearLinkIndex = 0;
    
            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                if(node.bIsCoil)
                {
                    nLinearLinkIndex += node.segmentLinks.Length;
                    continue;
                }
    
                for(int nJoint = 0; nJoint < node.linkJoints.Length; nJoint++)
                {
                    if(node.linkJoints[nJoint] == null && node.linkJointBreaksProcessed[nJoint] == false)
                    {
                        node.linkJointBreaksProcessed[nJoint] = true;

                        bool bIsFirst = nNode == 0 && nJoint == 0 && FirstNodeIsCoil() == false;

                        nNodeSnapped = nNode;

                        if(SendEvents)
                        {
                            if(EventsObjectReceiver != null && OnBreakMethodName.Length > 0)
                            {
                                RopeBreakEventInfo breakInfo = new RopeBreakEventInfo();
                                breakInfo.rope     = this;
                                breakInfo.worldPos = nJoint == node.linkJoints.Length - 1 ? node.goNode.transform.position : node.segmentLinks[nJoint].transform.position;
                                breakInfo.link2    = nJoint == node.linkJoints.Length - 1 ? node.goNode : node.segmentLinks[nJoint];
                                breakInfo.localLink2Pos = Vector3.zero;

                                if(bIsFirst)
                                {
                                    breakInfo.link1 = RopeStart.gameObject;
                                    breakInfo.localLink1Pos = Vector3.zero;
                                }
                                else
                                {
                                    if(nJoint > 0)
                                    {
                                        breakInfo.link1 = node.segmentLinks[nJoint - 1];
                                    }
                                    else
                                    {
                                        breakInfo.link1 = RopeNodes[nNode - 1].goNode;
                                    }

                                    breakInfo.localLink1Pos = GetLinkAxisOffset(LinkLengths[nLinearLinkIndex - 1]);
                                }

                                EventsObjectReceiver.SendMessage(OnBreakMethodName, breakInfo);
                            }
                        }
                    }
                    
                    if(nJoint < node.segmentLinks.Length)
                    {
                        nLinearLinkIndex++;
                    }
                }
            }
        }

        if(nNodeSnapped != -1)
        {
            RopeNode node = RopeNodes[nNodeSnapped];
            node.bSegmentBroken = true;

            for(int nJoint = 0; nJoint < node.linkJoints.Length; nJoint++)
            {
                if(node.linkJoints[nJoint] != null)
                {
                    node.linkJoints[nJoint].breakForce  = Mathf.Infinity;
                    node.linkJoints[nJoint].breakTorque = Mathf.Infinity;
                }
            }
        }
    }

    public void DeleteRope(bool bResetNodePositions = false, bool bDestroySkin = true)
    {
        // Delete rope links

        DeleteRopeLinks();

        // Restore node positions and delete joints

        foreach(RopeNode node in RopeNodes)
        {
            node.bSegmentBroken = false;

            if(node.bInitialOrientationInitialized && bResetNodePositions)
            {
                node.goNode.transform.localPosition = node.v3InitialLocalPos;
                node.goNode.transform.localRotation = node.qInitialLocalRot;
                node.goNode.transform.localScale    = node.v3InitialLocalScale;
            }

            node.bInitialOrientationInitialized = false;

            for(int i = 0; i < node.linkJoints.Length; i++)
            {
                if(node.linkJoints[i] != null)
                {
                    DestroyImmediate(node.linkJoints[i]);
                }
            }
        }

        if(RopeStart != null && m_bRopeStartInitialOrientationInitialized && bResetNodePositions)
        {
            RopeStart.transform.localPosition = m_v3InitialRopeStartLocalPos;
            RopeStart.transform.localRotation = m_qInitialRopeStartLocalRot;
            RopeStart.transform.localScale    = m_v3InitialRopeStartLocalScale;
        }

        m_bRopeStartInitialOrientationInitialized = false;

        // Delete rope bone list

        if(ImportedBones != null)
        {
            foreach(RopeBone bone in ImportedBones)
            {
                if(bone.goBone != null)
                {
                    bone.goBone.layer = bone.nOriginalLayer;

                    if(bone.bCreatedCollider && bone.goBone.GetComponent<Collider>() != null)
                    {
                        DestroyImmediate(bone.goBone.GetComponent<Collider>());
                    }

                    if(bone.bCreatedRigidbody && bone.goBone.GetComponent<Rigidbody>() != null)
                    {
                        DestroyImmediate(bone.goBone.GetComponent<Rigidbody>());
                    }
                }
            }

            foreach(RopeBone bone in ImportedBones)
            {
                if(bone.goBone != null)
                {
                    if(bone.tfNonBoneParent != null)
                    {
                        // Reposition
                        bone.goBone.transform.parent = bone.tfNonBoneParent;
                        bone.goBone.transform.localPosition = bone.v3OriginalLocalPos;
                        bone.goBone.transform.localRotation = bone.qOriginalLocalRot;
                    }

                    bone.goBone.transform.parent     = bone.tfParent;
                    bone.goBone.transform.localScale = bone.v3OriginalLocalScale;
                }
            }
        }

        if(Application.isEditor && Application.isPlaying)
        {
            // We keep ImportedBones so that it can mantain persistent data if a rope is deleted in playmode
        }
        else
        {
            ImportedBones = null;
        }

        // Delete skin

        SkinnedMeshRenderer skin = GetComponent<SkinnedMeshRenderer>();

        if(skin)
        {
            DestroyImmediate(skin.sharedMesh);

            if (bDestroySkin)
            {
              DestroyImmediate(skin);
            }
        }

        // Delete coil if necessary

        CheckDelCoilNode();

        Deleted = true;
    }

    public void DeleteRopeLinks()
    {
        if(m_bBonesAreImported == false)
        {
            if(CoilObject != null)
            {
                for(int nChild = 0; nChild < CoilObject.transform.childCount; nChild++)
                {
                    Transform tfChild = CoilObject.transform.GetChild(nChild);

                    if(tfChild.gameObject.GetComponent<UltimateRopeLink>() != null)
                    {
                        DestroyImmediate(tfChild.gameObject);
                        nChild--;
                    }
                }
            }

            if(RopeStart != null)
            {
                for(int nChild = 0; nChild < RopeStart.transform.childCount; nChild++)
                {
                    Transform tfChild = RopeStart.transform.GetChild(nChild);

                    if(tfChild.gameObject.GetComponent<UltimateRopeLink>() != null)
                    {
                        DestroyImmediate(tfChild.gameObject);
                        nChild--;
                    }
                }
            }

            for(int nChild = 0; nChild < transform.childCount; nChild++)
            {
                Transform tfChild = transform.GetChild(nChild);

                if(tfChild.gameObject.GetComponent<UltimateRopeLink>() != null)
                {
                    DestroyImmediate(tfChild.gameObject);
                    nChild--;
                }
            }

            foreach(RopeNode node in RopeNodes)
            {
                if(node.goNode)
                {
                    for(int nChild = 0; nChild < node.goNode.transform.childCount; nChild++)
                    {
                        Transform tfChild = node.goNode.transform.GetChild(nChild);

                        if(tfChild.gameObject.GetComponent<UltimateRopeLink>() != null)
                        {
                            DestroyImmediate(tfChild.gameObject);
                            nChild--;
                        }
                    }
                }

                node.segmentLinks = null;
            }
        }
    }

    public bool Regenerate(bool bResetNodePositions = false)
    {
        m_bLastStatusIsError = true;

        DeleteRope(bResetNodePositions, false);

        if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
        {
            if(RopeStart == null)    { Status = "A rope start GameObject needs to be specified"; return false; }
            if(RopeNodes == null)    { Status = "At least a rope node needs to be added";        return false; }
            if(RopeNodes.Count == 0) { Status = "At least a rope node needs to be added";        return false; }

            if(RopeType == ERopeType.Procedural)
            {
                if(IsExtensible && HasACoil && CoilObject == null)
                {
                    Status = "A coil object needs to be specified";
                    return false;
                }
            }

            if(RopeType == ERopeType.LinkedObjects)
            {
                if(LinkObject == null)
                {
                    Status = "A link object needs to be specified";
                    return false;
                }
            }

            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                if(RopeNodes[nNode].goNode == null)
                {
                    Status = string.Format("Rope segment {0} has unassigned Segment End property", nNode);
                    return false;
                }
            }
        }

        float fStartTime = Time.realtimeSinceStartup;

        List<RopeBone> NewListImportedBones = null;

        if(RopeType == ERopeType.ImportBones)
        {
            Status = "";

            List<int> ListImportBonesStatic;
            List<int> ListImportBonesNoCollider;

            if(BoneFirst == null)
            {
                Status = "The first bone needs to be specified";
                return false;
            }

            if(BoneLast == null)
            {
                Status = "The last bone needs to be specified";
                return false;
            }

            string strErrorStatus;

            if(ParseBoneIndices(BoneListNamesStatic, out ListImportBonesStatic, out strErrorStatus) == false)
            {
                Status = "Error parsing static bone list:\n" + strErrorStatus;
                return false;
            }

            if(ParseBoneIndices(BoneListNamesNoColliders, out ListImportBonesNoCollider, out strErrorStatus) == false)
            {
                Status = "Error parsing collider bone list:\n" + strErrorStatus;
                return false;
            }

            if(BuildImportedBoneList(BoneFirst, BoneLast, ListImportBonesStatic, ListImportBonesNoCollider, out NewListImportedBones, out strErrorStatus) == false)
            {
                Status = "Error building bone list:\n" + strErrorStatus;
                return false;
            }
        }

        this.gameObject.layer = RopeLayer;

        // Create links and joints

        CheckAddCoilNode();

        if(m_bRopeStartInitialOrientationInitialized == false && RopeStart != null)
        {
            m_v3InitialRopeStartLocalPos   = RopeStart.transform.localPosition;
            m_qInitialRopeStartLocalRot    = RopeStart.transform.localRotation;
            m_v3InitialRopeStartLocalScale = RopeStart.transform.localScale;
            m_bRopeStartInitialOrientationInitialized = true;
        }

        if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
        {
            TotalLinks = 0;
            TotalRopeLength = 0.0f;

            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                if(node.bInitialOrientationInitialized == false)
                {
                    node.v3InitialLocalPos   = node.goNode.transform.localPosition;
                    node.qInitialLocalRot    = node.goNode.transform.localRotation;
                    node.v3InitialLocalScale = node.goNode.transform.localScale;
                    node.bInitialOrientationInitialized = true;
                }

                if(node.nNumLinks < 1)
                {
                    node.nNumLinks = 1;
                }

                if(node.fLength < 0.0f)
                {
                    node.fLength = 0.001f;
                }

                node.nTotalLinks  = node.nNumLinks;
                node.fTotalLength = node.fLength;

                GameObject goSegmentStart = null;
                GameObject goSegmentEnd   = null;

                if(FirstNodeIsCoil() && nNode == 0)
                {
                    goSegmentStart = CoilObject;
                    goSegmentEnd   = RopeStart;
                }
                else
                {
                    goSegmentStart = nNode == m_nFirstNonCoilNode ? RopeStart : RopeNodes[nNode - 1].goNode;
                    goSegmentEnd   = RopeNodes[nNode].goNode;
                }

                node.m_v3LocalDirectionForward = goSegmentStart.transform.InverseTransformDirection((goSegmentEnd.transform.position - goSegmentStart.transform.position).normalized);

                if(nNode == RopeNodes.Count - 1 && IsExtensible)
                {
                    if(ExtensibleLength > 0.0f)
                    {
                        node.nTotalLinks  += (int)(ExtensibleLength / (node.fLength / node.nNumLinks)) + 1;
                        node.fTotalLength += ExtensibleLength;

                        node.m_bExtensionInitialized     = false;
                        node.m_nExtensionLinkIn          = node.nTotalLinks - node.nNumLinks;
                        node.m_nExtensionLinkOut         = node.m_nExtensionLinkIn - 1;
                        node.m_fExtensionRemainingLength = ExtensibleLength;
                        node.m_fExtensionRemainderIn     = 0.0f;
                        node.m_fExtensionRemainderOut    = 0.0f;

                        m_fCurrentExtension = 0.0f;
                    }
                }

                node.linkJoints = new ConfigurableJoint[node.nTotalLinks + 1];
                node.linkJointBreaksProcessed = new bool[node.nTotalLinks + 1];

                node.segmentLinks = new GameObject[node.nTotalLinks];

                if(FirstNodeIsCoil() && nNode == 0)
                {
                    for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                    {
                        string strLinkName = "Coil Link " + nLink;

                        node.segmentLinks[nLink] = new GameObject(strLinkName);
                        node.segmentLinks[nLink].AddComponent<UltimateRopeLink>();
                        node.segmentLinks[nLink].transform.parent = CoilObject.transform;
                        node.segmentLinks[nLink].layer = RopeLayer;
                    }

                    if(CoilDiameter < 0.0f) CoilDiameter = 0.0f;
                    if(CoilWidth    < 0.0f) CoilWidth    = 0.0f;

                    SetupCoilBones(ExtensibleLength);
                }
                else
                {
                    float fLinkLength = node.fLength / node.nNumLinks;
                    float fRemainingLength = ((goSegmentEnd.transform.position - goSegmentStart.transform.position).magnitude - fLinkLength) / (goSegmentEnd.transform.position - goSegmentStart.transform.position).magnitude;

                    float fLinkScale = RopeType == ERopeType.LinkedObjects ? GetLinkedObjectScale(node.fLength, node.nNumLinks) : 1.0f;

                    for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                    {
                        float fLinkT = (float)nLink / (node.segmentLinks.Length == 1 ? 1.0f : (node.segmentLinks.Length - 1.0f));

                        string strLinkName = "Node " + nNode + " Link " + nLink;

                        if(node.nTotalLinks > node.nNumLinks && nLink < (node.nTotalLinks - node.nNumLinks))
                        {
                            strLinkName += " (extension)";
                        }

                        if(RopeType == ERopeType.Procedural)
                        {
                            node.segmentLinks[nLink] = new GameObject(strLinkName);
                        }
                        else if(RopeType == ERopeType.LinkedObjects)
                        {
                            node.segmentLinks[nLink] = Instantiate(LinkObject) as GameObject;
                            node.segmentLinks[nLink].name = strLinkName;
                        }

                        node.segmentLinks[nLink].AddComponent<UltimateRopeLink>();

                        if(Vector3.Distance(goSegmentStart.transform.position, goSegmentEnd.transform.position) < 0.001f)
                        {
                            node.segmentLinks[nLink].transform.position = goSegmentStart.transform.position;
                            node.segmentLinks[nLink].transform.rotation = goSegmentStart.transform.rotation;
                        }
                        else
                        {
                            node.segmentLinks[nLink].transform.position = Vector3.Lerp(goSegmentStart.transform.position, goSegmentEnd.transform.position, fLinkT * fRemainingLength);
                            node.segmentLinks[nLink].transform.rotation = Quaternion.LookRotation((goSegmentEnd.transform.position - goSegmentStart.transform.position).normalized);
                        }

                        if(RopeType == ERopeType.LinkedObjects)
                        {
                            node.segmentLinks[nLink].transform.rotation   *= GetLinkedObjectLocalRotation(LinkTwistAngleStart + LinkTwistAngleIncrement * nLink);
                            node.segmentLinks[nLink].transform.localScale  = new Vector3(fLinkScale, fLinkScale, fLinkScale);
                        }

                        if(node.segmentLinks[nLink].GetComponent<Rigidbody>() == null)
                        {
                            node.segmentLinks[nLink].AddComponent<Rigidbody>();
                        }

                        node.segmentLinks[nLink].transform.parent = this.transform;

                        node.segmentLinks[nLink].layer = RopeLayer;
                    }
                }
            
                TotalLinks      += node.segmentLinks.Length;
                TotalRopeLength += node.fTotalLength;
            }

            m_bBonesAreImported = false;
        }
        else if(RopeType == ERopeType.ImportBones)
        {
            TotalLinks = 0;
            TotalRopeLength = 0.0f;

            ImportedBones = NewListImportedBones.ToArray();

            bool bHasRopeNodes = false;

            if(RopeNodes != null)
            {
                if(RopeNodes.Count != 0)
                {
                    bHasRopeNodes = true;
                }
            }

            if(bHasRopeNodes == false)
            {
                RopeNodes = new List<UltimateRope.RopeNode>();
                RopeNodes.Add(new UltimateRope.RopeNode());
            }

            RopeNode node = RopeNodes[0];
            node.nNumLinks = ImportedBones.Length;
            node.nTotalLinks = node.nNumLinks;
            node.linkJoints = new ConfigurableJoint [ImportedBones.Length];
            node.linkJointBreaksProcessed = new bool[ImportedBones.Length];

            node.segmentLinks = new GameObject[node.nTotalLinks];

            int nLinearIndex = 0;

            for(int nBone = 0; nBone < ImportedBones.Length; nBone++)
            {
                node.segmentLinks[nLinearIndex] = ImportedBones[nBone].goBone;

                if(ImportedBones[nBone].goBone.GetComponent<Rigidbody>() == null)
                {
                    ImportedBones[nBone].goBone.AddComponent<Rigidbody>();
                    ImportedBones[nBone].bCreatedRigidbody = true;
                }
                else ImportedBones[nBone].bCreatedRigidbody = false;

                ImportedBones[nBone].goBone.layer = RopeLayer;

                float fLength = 0.0f;

                if(nLinearIndex < ImportedBones.Length - 1)
                {
                    fLength = Vector3.Distance(ImportedBones[nBone].goBone.transform.position, ImportedBones[nBone + 1].goBone.transform.position);
                }
                else
                {
                    fLength = 0.0f;
                }

                TotalLinks      += node.segmentLinks.Length;
                TotalRopeLength += fLength;

                ImportedBones[nBone].fLength = fLength;

                nLinearIndex++;
            }

            node.fLength = TotalRopeLength;
            node.fTotalLength = node.fLength;
            node.eColliderType = BoneColliderType;
            node.nColliderSkip = BoneColliderSkip;

            m_bBonesAreImported = true;
        }

        // Create mesh

        if(RopeType == ERopeType.Procedural)
        {
            Transform[] boneTransforms = new Transform[TotalLinks];
            Matrix4x4[] bindPoses      = new Matrix4x4[TotalLinks];

            LinkLengths = new float[TotalLinks];

            int nLinearLinkIndex = 0;
            
            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];
                
                for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                {
                    boneTransforms[nLinearLinkIndex] = node.segmentLinks[nLink].transform;
                    bindPoses[nLinearLinkIndex]      = node.segmentLinks[nLink].transform.worldToLocalMatrix;
    
                    if(node.segmentLinks[nLink].transform.parent != null)
                    {
                        bindPoses[nLinearLinkIndex] *= this.transform.localToWorldMatrix;//node.segmentLinks[nLink].transform.parent.localToWorldMatrix;
                    }
                    
                    LinkLengths[nLinearLinkIndex] = node.fLength / node.nNumLinks;
                    
                    nLinearLinkIndex++;
                }
            }
    
            // Build mesh(es)

            if(RopeDiameter < 0.01f)
            {
                RopeDiameter = 0.01f;
            }

            if (RopeDiameterScaleX < 0.01f)
            {
              RopeDiameterScaleX = 0.01f;
            }

            if (RopeDiameterScaleY < 0.01f)
            {
              RopeDiameterScaleY = 0.01f;
            }

            bool bBreakable = LinkJointBreakForce != Mathf.Infinity || LinkJointBreakTorque != Mathf.Infinity;

            Mesh newMesh = new Mesh();

            int nVertices          = bBreakable ? (TotalLinks * (RopeSegmentSides + 1) * 4) : (((TotalLinks + 1) * (RopeSegmentSides + 1)) + ((RopeSegmentSides + 1) * 2));
            int nTrianglesRope     = TotalLinks * RopeSegmentSides * 2;
            int nTrianglesSections = bBreakable ? (TotalLinks * 2 * (RopeSegmentSides - 2)) : (2 * (RopeSegmentSides - 2));

            Vector3[]    vertices          = new Vector3   [nVertices];
            Vector2[]    mapping           = new Vector2   [nVertices];
            Vector4[]    tangents          = new Vector4   [nVertices];
            BoneWeight[] weights           = new BoneWeight[nVertices];
            int[]        trianglesRope     = new int       [nTrianglesRope     * 3];
            int[]        trianglesSections = new int       [nTrianglesSections * 3];
        
            if(bBreakable)
            {
                int nVertexIndex = 0;
            
                for(int nLink = 0; nLink < TotalLinks; nLink++)
                {
                    int   nBoneIndex0 = nLink;
                    int   nBoneIndex1 = nBoneIndex0;
                    float fWeight0    = 1.0f;
                    float fWeight1    = 1.0f - fWeight0;
                
                    FillLinkMeshIndicesRope    (nLink, TotalLinks, ref trianglesRope,     bBreakable);
                    FillLinkMeshIndicesSections(nLink, TotalLinks, ref trianglesSections, bBreakable);

                    for(int nPart = 0; nPart < 4; nPart++)
                    {
                        for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
                        {
                            int nExtreme = nPart < 2 ? 0 : 1;

                            float fRopeT = (float)(nLink + nExtreme) / (float)TotalLinks;

                            float fCos = Mathf.Cos(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);
                            float fSin = Mathf.Sin(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);

                            vertices[nVertexIndex] = new Vector3(fCos * RopeDiameter * RopeDiameterScaleX * 0.5f, fSin * RopeDiameter * RopeDiameterScaleY * 0.5f, LinkLengths[nLink] * nExtreme);
                            vertices[nVertexIndex] = (boneTransforms[nBoneIndex0].TransformPoint(vertices[nVertexIndex]) * fWeight0) + (boneTransforms[nBoneIndex1].TransformPoint(vertices[nVertexIndex]) * fWeight1);
                            vertices[nVertexIndex] = this.transform.InverseTransformPoint(vertices[nVertexIndex]);

                            if(nPart == 0 || nPart == 3)
                            {
                                // tops
                                mapping[nVertexIndex]  = new Vector2(Mathf.Clamp01((fCos + 1.0f) * 0.5f), Mathf.Clamp01((fSin + 1.0f) * 0.5f));
                                tangents[nVertexIndex] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                            }
                            else
                            {
                                // rope
                                mapping[nVertexIndex]  = new Vector2(fRopeT * TotalRopeLength * RopeTextureTileMeters, (float)nSide / (float)RopeSegmentSides);
                                tangents[nVertexIndex] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                            }

                            weights [nVertexIndex].boneIndex0 = nBoneIndex0;
                            weights [nVertexIndex].boneIndex1 = nBoneIndex1;
                            weights [nVertexIndex].weight0    = fWeight0;
                            weights [nVertexIndex].weight1    = fWeight1;

                            nVertexIndex++;
                        }
                    }
                }
            }   
            else
            {
                int nVertexIndex = 0;

                FillLinkMeshIndicesSections(0, TotalLinks, ref trianglesSections, bBreakable);
            
                for(int nLink = 0; nLink < TotalLinks + 1; nLink++)
                {
                    int   nBoneIndex0 = nLink < TotalLinks ? nLink : TotalLinks - 1;
                    int   nBoneIndex1 = nBoneIndex0;
                    float fWeight0    = 1.0f;
                    float fWeight1    = 1.0f - fWeight0;

                    if(nLink < TotalLinks)
                    {
                        FillLinkMeshIndicesRope(nLink, TotalLinks, ref trianglesRope, bBreakable);
                    }
                
                    bool bFirst = false;
                    bool bLast  = false;
                    int  nRepeats = 1;

                    if (nLink == 0)          { nRepeats++; bFirst = true; }
                    if (nLink == TotalLinks) { nRepeats++; bLast  = true; }

                    for(int nRepeat = 0; nRepeat < nRepeats; nRepeat++)
                    {
                        for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
                        {
                            float fRopeT = (float)nLink / (float)TotalLinks;
                            float fCos = Mathf.Cos(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);
                            float fSin = Mathf.Sin(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);

                            vertices[nVertexIndex] = new Vector3(fCos * RopeDiameter * RopeDiameterScaleX * 0.5f, fSin * RopeDiameter * RopeDiameterScaleY * 0.5f, bLast == false ? 0.0f : LinkLengths[TotalLinks - 1]);
                            vertices[nVertexIndex] = (boneTransforms[nBoneIndex0].TransformPoint(vertices[nVertexIndex]) * fWeight0) + (boneTransforms[nBoneIndex1].TransformPoint(vertices[nVertexIndex]) * fWeight1);
                            vertices[nVertexIndex] = this.transform.InverseTransformPoint(vertices[nVertexIndex]);

                            if((bFirst && nRepeat == 0) || (bLast && nRepeat == (nRepeats - 1)))
                            {
                                // Tops
                                mapping[nVertexIndex] = new Vector2(Mathf.Clamp01((fCos + 1.0f) * 0.5f), Mathf.Clamp01((fSin + 1.0f) * 0.5f));
                                tangents[nVertexIndex] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                            }
                            else
                            {
                                // Rope
                                mapping[nVertexIndex] = new Vector2(fRopeT * TotalRopeLength * RopeTextureTileMeters, (float)nSide / (float)RopeSegmentSides);
                                tangents[nVertexIndex] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                            }
                    
                            weights [nVertexIndex].boneIndex0 = nBoneIndex0;
                            weights [nVertexIndex].boneIndex1 = nBoneIndex1;
                            weights [nVertexIndex].weight0    = fWeight0;
                            weights [nVertexIndex].weight1    = fWeight1;

                            nVertexIndex++;
                        }
                    }
                }
            }   

            newMesh.vertices    = vertices;
            newMesh.uv          = mapping;
            newMesh.boneWeights = weights;
            newMesh.bindposes   = bindPoses;

            newMesh.subMeshCount = 2;
            newMesh.SetTriangles(trianglesRope,     0);
            newMesh.SetTriangles(trianglesSections, 1);
            newMesh.RecalculateNormals();
            newMesh.tangents = tangents;

            // Build skinned mesh renderer

            SkinnedMeshRenderer skin = gameObject.GetComponent<SkinnedMeshRenderer>() != null ? gameObject.GetComponent<SkinnedMeshRenderer>() : gameObject.AddComponent<SkinnedMeshRenderer>();

            Material[] ropeMaterials = new Material[2];
            ropeMaterials[0] = RopeMaterial;
            ropeMaterials[1] = RopeSectionMaterial;

            skin.materials  = ropeMaterials;
            skin.bones      = boneTransforms;
            skin.sharedMesh = newMesh;

            skin.updateWhenOffscreen = true;
        }

        Deleted = false;

        if(Application.isPlaying)
        {
            CreateRopeJoints();
        }

        SetupRopeLinks();

        float fEndTime = Time.realtimeSinceStartup;

        Status = string.Format("Rope generated in {0} seconds", fEndTime - fStartTime);
        m_bLastStatusIsError = false;

        return true;
    }

    public bool IsLastStatusError()
    {
        return m_bLastStatusIsError;
    }

    public bool ChangeRopeDiameter(float fNewDiameter, float fNewScaleX, float fNewScaleY)
    {
        if(RopeType != ERopeType.Procedural)
        {
            return false;
        }

        SkinnedMeshRenderer skin = gameObject.GetComponent<SkinnedMeshRenderer>();

        if(skin == null)
        {
            return false;
        }

        // Build mesh(es)

        RopeDiameter = fNewDiameter;
        RopeDiameterScaleX = fNewScaleX;
        RopeDiameterScaleY = fNewScaleY;
        
        if(RopeDiameter < 0.01f)
        {
            RopeDiameter = 0.01f;
        }

        if (RopeDiameterScaleX < 0.01f)
        {
          RopeDiameterScaleX = 0.01f;
        }

        if (RopeDiameterScaleY < 0.01f)
        {
          RopeDiameterScaleY = 0.01f;
        }

        bool bBreakable = LinkJointBreakForce != Mathf.Infinity || LinkJointBreakTorque != Mathf.Infinity;

        Vector3[]   vertices        = skin.sharedMesh.vertices;
        Matrix4x4[] bindPoses       = skin.sharedMesh.bindposes;
        Vector2[]   verticesSection = new Vector2[RopeSegmentSides + 1];

        // Precalc section vertices

        for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
        {
            float fCos = Mathf.Cos(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);
            float fSin = Mathf.Sin(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);

            verticesSection[nSide] = new Vector2(fCos * RopeDiameter * RopeDiameterScaleX * 0.5f, fSin * RopeDiameter * RopeDiameterScaleY * 0.5f);
        }
        
        if(bBreakable)
        {
            int nVertexIndex = 0;

            for(int nLink = 0; nLink < TotalLinks; nLink++)
            {
                int   nBoneIndex0 = nLink;
                int   nBoneIndex1 = nBoneIndex0;
                float fWeight0    = 1.0f;
                float fWeight1    = 1.0f - fWeight0;

                bindPoses[nLink] = skin.bones[nLink].transform.worldToLocalMatrix;
    
                if(skin.bones[nLink].transform.parent != null)
                {
                    bindPoses[nLink] *= this.transform.localToWorldMatrix; //skin.bones[nLink].transform.parent.localToWorldMatrix;
                }

                for(int nPart = 0; nPart < 4; nPart++)
                {
                    for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
                    {
                        int nExtreme = nPart < 2 ? 0 : 1;

                        vertices[nVertexIndex] = new Vector3(verticesSection[nSide].x, verticesSection[nSide].y, LinkLengths[nLink] * nExtreme);
                        vertices[nVertexIndex] = (skin.bones[nBoneIndex0].TransformPoint(vertices[nVertexIndex]) * fWeight0) + (skin.bones[nBoneIndex1].TransformPoint(vertices[nVertexIndex]) * fWeight1);
                        vertices[nVertexIndex] = this.transform.InverseTransformPoint(vertices[nVertexIndex]);
                        nVertexIndex++;
                    }
                }
            }
        }   
        else
        {
            int nVertexIndex = 0;
            
            for(int nLink = 0; nLink < TotalLinks + 1; nLink++)
            {
                int   nBoneIndex0 = nLink < TotalLinks ? nLink : TotalLinks - 1;
                int   nBoneIndex1 = nBoneIndex0;
                float fWeight0    = 1.0f;
                float fWeight1    = 1.0f - fWeight0;

                bool bLast    = false;
                int  nRepeats = 1;

                if (nLink == 0)          { nRepeats++; }
                if (nLink == TotalLinks) { nRepeats++; bLast  = true; }

                if(nLink < TotalLinks)
                {
                    bindPoses[nLink] = skin.bones[nLink].transform.worldToLocalMatrix;
    
                    if(skin.bones[nLink].transform.parent != null)
                    {
                        bindPoses[nLink] *= this.transform.localToWorldMatrix; //skin.bones[nLink].transform.parent.localToWorldMatrix;
                    }
                }

                for(int nRepeat = 0; nRepeat < nRepeats; nRepeat++)
                {
                    for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
                    {
                        vertices[nVertexIndex] = new Vector3(verticesSection[nSide].x, verticesSection[nSide].y, bLast == false ? 0.0f : LinkLengths[TotalLinks - 1]);
                        vertices[nVertexIndex] = (skin.bones[nBoneIndex0].TransformPoint(vertices[nVertexIndex]) * fWeight0) + (skin.bones[nBoneIndex1].TransformPoint(vertices[nVertexIndex]) * fWeight1);
                        vertices[nVertexIndex] = this.transform.InverseTransformPoint(vertices[nVertexIndex]);

                        nVertexIndex++;
                    }
                }
            }
        }   

        skin.sharedMesh.vertices  = vertices;
        skin.sharedMesh.bindposes = bindPoses;
        SetupRopeLinks();

        return true;
    }

    public bool ChangeRopeSegmentSides(int nNewSegmentSides)
    {
        if(RopeType != ERopeType.Procedural)
        {
            return false;
        }

        SkinnedMeshRenderer skin = gameObject.GetComponent<SkinnedMeshRenderer>();

        if(skin == null)
        {
            return false;
        }

        // Build mesh(es)

        RopeSegmentSides = nNewSegmentSides;
        
        if(RopeSegmentSides < 3)
        {
            RopeSegmentSides = 3;
        }

        bool bBreakable = LinkJointBreakForce != Mathf.Infinity || LinkJointBreakTorque != Mathf.Infinity;

        Mesh newMesh = new Mesh();

        int nVertices          = bBreakable ? (TotalLinks * (RopeSegmentSides + 1) * 4) : (((TotalLinks + 1) * (RopeSegmentSides + 1)) + ((RopeSegmentSides + 1) * 2));
        int nTrianglesRope     = TotalLinks * RopeSegmentSides * 2;
        int nTrianglesSections = bBreakable ? (TotalLinks * 2 * (RopeSegmentSides - 2)) : (2 * (RopeSegmentSides - 2));

        Vector3[]    vertices          = new Vector3   [nVertices];
        Vector2[]    mapping           = new Vector2   [nVertices];
        Vector4[]    tangents          = new Vector4   [nVertices];
        BoneWeight[] weights           = new BoneWeight[nVertices];
        int[]        trianglesRope     = new int       [nTrianglesRope     * 3];
        int[]        trianglesSections = new int       [nTrianglesSections * 3];
        Matrix4x4[]  bindPoses         = skin.sharedMesh.bindposes;
        
        if(bBreakable)
        {
            int nVertexIndex = 0;
            
            for(int nLink = 0; nLink < TotalLinks; nLink++)
            {
                int   nBoneIndex0 = nLink;
                int   nBoneIndex1 = nBoneIndex0;
                float fWeight0    = 1.0f;
                float fWeight1    = 1.0f - fWeight0;
                
                bindPoses[nLink] = skin.bones[nLink].transform.worldToLocalMatrix;
    
                if(skin.bones[nLink].transform.parent != null)
                {
                    bindPoses[nLink] *= this.transform.localToWorldMatrix; //skin.bones[nLink].transform.parent.localToWorldMatrix;
                }

                FillLinkMeshIndicesRope    (nLink, TotalLinks, ref trianglesRope,     bBreakable);
                FillLinkMeshIndicesSections(nLink, TotalLinks, ref trianglesSections, bBreakable);

                for(int nPart = 0; nPart < 4; nPart++)
                {
                    for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
                    {
                        int nExtreme = nPart < 2 ? 0 : 1;

                        float fRopeT = (float)(nLink + nExtreme) / (float)TotalLinks;

                        float fCos = Mathf.Cos(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);
                        float fSin = Mathf.Sin(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);

                        vertices[nVertexIndex] = new Vector3(fCos * RopeDiameter * RopeDiameterScaleX * 0.5f, fSin * RopeDiameter * RopeDiameterScaleY * 0.5f, LinkLengths[nLink] * nExtreme);
                        vertices[nVertexIndex] = (skin.bones[nBoneIndex0].TransformPoint(vertices[nVertexIndex]) * fWeight0) + (skin.bones[nBoneIndex1].TransformPoint(vertices[nVertexIndex]) * fWeight1);
                        vertices[nVertexIndex] = this.transform.InverseTransformPoint(vertices[nVertexIndex]);

                        if(nPart == 0 || nPart == 3)
                        {
                            // Tops
                            mapping [nVertexIndex] = new Vector2(Mathf.Clamp01((fCos + 1.0f) * 0.5f), Mathf.Clamp01((fSin + 1.0f) * 0.5f));
                            tangents[nVertexIndex] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                        }
                        else
                        {
                            // Rope
                            mapping [nVertexIndex] = new Vector2(fRopeT * TotalRopeLength * RopeTextureTileMeters, (float)nSide / (float)RopeSegmentSides);
                            tangents[nVertexIndex] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                        }

                        weights [nVertexIndex].boneIndex0 = nBoneIndex0;
                        weights [nVertexIndex].boneIndex1 = nBoneIndex1;
                        weights [nVertexIndex].weight0    = fWeight0;
                        weights [nVertexIndex].weight1    = fWeight1;

                        nVertexIndex++;
                    }
                }
            }
        }   
        else
        {
            int nVertexIndex = 0;

            FillLinkMeshIndicesSections(0, TotalLinks, ref trianglesSections, bBreakable);
            
            for(int nLink = 0; nLink < TotalLinks + 1; nLink++)
            {
                int   nBoneIndex0 = nLink < TotalLinks ? nLink : TotalLinks - 1;
                int   nBoneIndex1 = nBoneIndex0;
                float fWeight0    = 1.0f;
                float fWeight1    = 1.0f - fWeight0;

                if(nLink < TotalLinks)
                {
                    FillLinkMeshIndicesRope(nLink, TotalLinks, ref trianglesRope, bBreakable);
                }
                
                bool bFirst = false;
                bool bLast  = false;
                int  nRepeats = 1;

                if (nLink == 0)          { nRepeats++; bFirst = true; }
                if (nLink == TotalLinks) { nRepeats++; bLast  = true; }

                if(nLink < TotalLinks)
                {
                    bindPoses[nLink] = skin.bones[nLink].transform.worldToLocalMatrix;
    
                    if(skin.bones[nLink].transform.parent != null)
                    {
                        bindPoses[nLink] *= this.transform.localToWorldMatrix; //skin.bones[nLink].transform.parent.localToWorldMatrix;
                    }
                }

                for(int nRepeat = 0; nRepeat < nRepeats; nRepeat++)
                {
                    for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
                    {
                        float fRopeT = (float)nLink / (float)TotalLinks;
                        float fCos = Mathf.Cos(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);
                        float fSin = Mathf.Sin(((float)nSide / (float)RopeSegmentSides) * Mathf.PI * 2.0f);

                        vertices[nVertexIndex] = new Vector3(fCos * RopeDiameter * RopeDiameterScaleX * 0.5f, fSin * RopeDiameter * RopeDiameterScaleY * 0.5f, bLast == false ? 0.0f : LinkLengths[TotalLinks - 1]);
                        vertices[nVertexIndex] = (skin.bones[nBoneIndex0].TransformPoint(vertices[nVertexIndex]) * fWeight0) + (skin.bones[nBoneIndex1].TransformPoint(vertices[nVertexIndex]) * fWeight1);
                        vertices[nVertexIndex] = this.transform.InverseTransformPoint(vertices[nVertexIndex]);

                        if((bFirst && nRepeat == 0) || (bLast && nRepeat == (nRepeats - 1)))
                        {
                            // Tops
                            mapping [nVertexIndex] = new Vector2(Mathf.Clamp01((fCos + 1.0f) * 0.5f), Mathf.Clamp01((fSin + 1.0f) * 0.5f));
                            tangents[nVertexIndex] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                        }
                        else
                        {
                            // Rope
                            mapping [nVertexIndex] = new Vector2(fRopeT * TotalRopeLength * RopeTextureTileMeters, (float)nSide / (float)RopeSegmentSides);
                            tangents[nVertexIndex] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                        }
                    
                        weights [nVertexIndex].boneIndex0 = nBoneIndex0;
                        weights [nVertexIndex].boneIndex1 = nBoneIndex1;
                        weights [nVertexIndex].weight0    = fWeight0;
                        weights [nVertexIndex].weight1    = fWeight1;

                        nVertexIndex++;
                    }
                }
            }
        }   

        newMesh.vertices    = vertices;
        newMesh.uv          = mapping;
        newMesh.boneWeights = weights;
        newMesh.bindposes   = bindPoses;

        newMesh.subMeshCount = 2;
        newMesh.SetTriangles(trianglesRope,     0);
        newMesh.SetTriangles(trianglesSections, 1);
        newMesh.RecalculateNormals();
        newMesh.tangents = tangents;

        // Update skinned mesh renderer

        if(Application.isEditor && Application.isPlaying == false)
        {
            DestroyImmediate(skin.sharedMesh);
        }
        else
        {
            Destroy(skin.sharedMesh);
        }

        skin.sharedMesh = newMesh;

        SetupRopeLinks();

        return true;
    }

    public void SetupRopeMaterials()
    {
        if(RopeType != ERopeType.Procedural)
        {
            return;
        }

        SkinnedMeshRenderer skin = gameObject.GetComponent<SkinnedMeshRenderer>();
        
        if(skin != null)
        {
            Material[] ropeMaterials = new Material[2];
            ropeMaterials[0] = RopeMaterial;
            ropeMaterials[1] = RopeSectionMaterial;

            skin.materials = ropeMaterials;
        }
    }

    public void SetupRopeLinks()
    {
        if(RopeNodes == null)     return;
        if(RopeNodes.Count  == 0) return;
        if(Deleted == true) return;

        if(RopeType == ERopeType.ImportBones)
        {
            if(ImportedBones == null)
            {
                return;
            }
        }

        gameObject.layer = RopeLayer;

        if(RopeDiameter < 0.01f)
        {
            RopeDiameter = 0.01f;
        }

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            if(node.bIsCoil) continue;

            if(RopeType == ERopeType.ImportBones)
            {
                node.eColliderType = BoneColliderType;
                node.nColliderSkip = BoneColliderSkip;
            }

            float fLinkLength     = node.fLength / node.nNumLinks;
            float fRopeDiameter   = GetLinkDiameter();
            int   nColliderSkip   = node.nColliderSkip;
            float fColliderCenter = RopeType == ERopeType.Procedural ? fLinkLength * 0.5f : 0.0f;

            int nLink = 0;

            foreach(GameObject link in node.segmentLinks)
            {
                if(link)
                {
                    if(link.GetComponent<Collider>())
                    {
                        DestroyImmediate(link.GetComponent<Collider>());
                    }

                    bool bColliderEnabled = nLink % (nColliderSkip + 1) == 0 ? true : false;
                    bool bKinematic = link.GetComponent<Rigidbody>() != null ? link.GetComponent<Rigidbody>().isKinematic : false;

                    if(RopeType == ERopeType.ImportBones)
                    {
                        if(Mathf.Approximately(ImportedBones[nLink].fLength, 0.0f) == true)
                        {
                            bColliderEnabled = false;
                        }
                        else if(bColliderEnabled)
                        {
                            bColliderEnabled = ImportedBones[nLink].bCreatedCollider;
                        }

                        fLinkLength     = ImportedBones[nLink].fLength * BoneColliderLength;
                        fColliderCenter = fLinkLength * BoneColliderOffset;
                        bKinematic      = ImportedBones[nLink].bIsStatic;
                    }

                    if(bColliderEnabled)
                    {
                        switch(node.eColliderType)
                        {
                            case EColliderType.Capsule:

                                CapsuleCollider capsule = link.AddComponent<CapsuleCollider>();

                                capsule.material  = RopePhysicsMaterial;
                                capsule.center    = GetLinkAxisOffset(fColliderCenter);
                                capsule.radius    = fRopeDiameter * 0.5f;
                                capsule.height    = fLinkLength;
                                capsule.direction = GetLinkAxisIndex();
                                capsule.material  = RopePhysicsMaterial;
                                capsule.enabled   = bColliderEnabled;

                                break;

                            case EColliderType.Box:

                                BoxCollider box = link.AddComponent<BoxCollider>();

                                Vector3 v3Center = GetLinkAxisOffset(fColliderCenter);
                                Vector3 v3Size   = Vector3.zero;

                                box.material = RopePhysicsMaterial;

                                if(GetLinkBoxColliderCenterAndSize(fLinkLength, fRopeDiameter, ref v3Center, ref v3Size))
                                {
                                    box.center  = v3Center;
                                    box.size    = v3Size;
                                    box.enabled = bColliderEnabled;
                                }
                                else
                                {
                                    box.enabled = false;
                                }

                                break;
                        }
                    }

                    if(link.GetComponent<Collider>() != null)
                    {
                        link.GetComponent<Collider>().enabled = bKinematic == false;
                    }

                    Rigidbody linkRigidbody = link.GetComponent<Rigidbody>() != null ? link.GetComponent<Rigidbody>() : link.AddComponent<Rigidbody>();

                    linkRigidbody.mass = LinkMass;
                    linkRigidbody.solverIterations = LinkSolverIterationCount;
                    linkRigidbody.isKinematic = bKinematic;

                    link.layer = RopeLayer;        
                    nLink++;
                }
            }
        }
    }

    public void SetupRopeJoints()
    {
        if(RopeNodes == null)     return;
        if(RopeNodes.Count  == 0) return;
        if(Deleted == true)       return;

        if(RopeType == ERopeType.ImportBones)
        {
            if(ImportedBones == null)
            {
                return;
            }
        }

        foreach(RopeNode node in RopeNodes)
        {
            if(node.segmentLinks == null)
            {
                return;
            }
        }

        // Store transforms for later

        int nLinearLinkIndex = 0;

        Vector3[]    av3LinkPositions = new Vector3[TotalLinks];
        Quaternion[] aqLinkRotations  = new Quaternion[TotalLinks];

        Vector3      v3LocalStartRope = RopeStart != null ? RopeStart.transform.localPosition : Vector3.zero;
        Quaternion   qLocalStartRope  = RopeStart != null ? RopeStart.transform.localRotation : Quaternion.identity;
        Vector3[]    av3NodePositions = new Vector3[RopeNodes.Count];
        Quaternion[] aqNodeRotations  = new Quaternion[RopeNodes.Count];

        if(m_bRopeStartInitialOrientationInitialized && RopeStart != null)
        {
            RopeStart.transform.localPosition = m_v3InitialRopeStartLocalPos;
            RopeStart.transform.localRotation = m_qInitialRopeStartLocalRot;
        }

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            if(node.bInitialOrientationInitialized && node.goNode != null)
            {
                av3NodePositions[nNode] = node.goNode.transform.localPosition;
                aqNodeRotations[nNode]  = node.goNode.transform.localRotation;
                node.goNode.transform.localPosition = node.v3InitialLocalPos;
                node.goNode.transform.localRotation = node.qInitialLocalRot;
            }
        }

        if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
        {
            // Put them in line so that new motion limits will be referenced from here, not relative to the current position

            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                float fLinkLength    = node.fLength / node.nNumLinks;
                float fSegmentLength = fLinkLength * (node.segmentLinks.Length - 1);

                for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                {
                    float fLinkT = (float)nLink / (node.segmentLinks.Length == 1 ? 1.0f : (node.segmentLinks.Length - 1.0f));

                    av3LinkPositions[nLinearLinkIndex] = node.segmentLinks[nLink].transform.position;
                    aqLinkRotations[nLinearLinkIndex]  = node.segmentLinks[nLink].transform.rotation;

                    if(node.bIsCoil == false)
                    {
                        node.segmentLinks[nLink].transform.position = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, fSegmentLength), fLinkT);
                        node.segmentLinks[nLink].transform.rotation = Quaternion.identity;

                        if(RopeType == ERopeType.LinkedObjects)
                        {
                            node.segmentLinks[nLink].transform.rotation *= GetLinkedObjectLocalRotation(LinkTwistAngleStart + LinkTwistAngleIncrement * nLink);
                        }
                    }

                    nLinearLinkIndex++;
                }
            }
        }
        else if(RopeType == ERopeType.ImportBones)
        {
            // Reposition to original

            for(int nLink = 0; nLink < ImportedBones.Length; nLink++)
            {
                av3LinkPositions[nLink] = ImportedBones[nLink].goBone.transform.position;
                aqLinkRotations[nLink]  = ImportedBones[nLink].goBone.transform.rotation;

                if(ImportedBones[nLink].tfNonBoneParent != null)
                {
                    Transform tfParent = ImportedBones[nLink].goBone.transform.parent;
                    ImportedBones[nLink].goBone.transform.parent = ImportedBones[nLink].tfNonBoneParent;
                    ImportedBones[nLink].goBone.transform.localPosition = ImportedBones[nLink].v3OriginalLocalPos;
                    ImportedBones[nLink].goBone.transform.localRotation = ImportedBones[nLink].qOriginalLocalRot;
                    ImportedBones[nLink].goBone.transform.parent = tfParent;
                    ImportedBones[nLink].goBone.transform.localScale = ImportedBones[nLink].v3OriginalLocalScale;
                }
            }
        }

        // Set properties

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            if(node.bIsCoil == false)
            {
                foreach(ConfigurableJoint joint in node.linkJoints)
                {
                    if(joint)
                    {
                        SetupJoint(joint);
                    }
                }

                if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
                {
                    if(node.bInitialOrientationInitialized)
                    {
                        GameObject goSegmentStart = nNode == m_nFirstNonCoilNode ? RopeStart : RopeNodes[nNode - 1].goNode;
                        GameObject goSegmentEnd   = RopeNodes[nNode].goNode;

                        Vector3 v3WorldForward = goSegmentStart.transform.TransformDirection(node.m_v3LocalDirectionForward);
                        Vector3 v3WorldUp      = goSegmentStart.transform.TransformDirection(node.m_v3LocalDirectionUp);

                        node.segmentLinks[0].transform.position = goSegmentStart.transform.position;
                        node.segmentLinks[0].transform.rotation = Quaternion.LookRotation(v3WorldForward, v3WorldUp);

                        node.segmentLinks[node.segmentLinks.Length - 1].transform.position = goSegmentEnd.transform.position - (v3WorldForward * (node.fLength / node.nNumLinks));
                        node.segmentLinks[node.segmentLinks.Length - 1].transform.rotation = Quaternion.LookRotation(v3WorldForward, v3WorldUp);

                        if(RopeType == ERopeType.LinkedObjects)
                        {
                            node.segmentLinks[0].transform.rotation *= GetLinkedObjectLocalRotation(LinkTwistAngleStart);
                            node.segmentLinks[node.segmentLinks.Length - 1].transform.rotation *= GetLinkedObjectLocalRotation(LinkTwistAngleStart + LinkTwistAngleIncrement * (node.segmentLinks.Length - 1));
                        }

                        if(node.linkJoints[0] != null)
                        {
                            SetupJoint(node.linkJoints[0]);
                        }

                        if(node.linkJoints[node.linkJoints.Length - 1] != null)
                        {
                            SetupJoint(node.linkJoints[node.linkJoints.Length - 1]);
                        }
                    }
                }
            }
        }

        // Restore transforms

        nLinearLinkIndex = 0;

        if(m_bRopeStartInitialOrientationInitialized && RopeStart != null)
        {
            RopeStart.transform.localPosition = v3LocalStartRope;
            RopeStart.transform.localRotation = qLocalStartRope;
        }

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            if(node.bInitialOrientationInitialized && node.goNode != null)
            {
                node.goNode.transform.localPosition = av3NodePositions[nNode];
                node.goNode.transform.localRotation = aqNodeRotations[nNode];
            }
        }

        if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
        {
            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                {
                    node.segmentLinks[nLink].transform.position = av3LinkPositions[nLinearLinkIndex];
                    node.segmentLinks[nLink].transform.rotation = aqLinkRotations[nLinearLinkIndex];

                    nLinearLinkIndex++;
                }
            }
        }
        else if(RopeType == ERopeType.ImportBones)
        {
            for(int nLink = 0; nLink < ImportedBones.Length; nLink++)
            {
                ImportedBones[nLink].goBone.transform.position = av3LinkPositions[nLink];
                ImportedBones[nLink].goBone.transform.rotation = aqLinkRotations[nLink];
            }
        }
    }

    public void CheckNeedsStartExitLockZ()
    {
        if(RopeType == ERopeType.Procedural)
        {
            int nLinearLinkIndex = 0;

            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                {
                    Transform tfSegmentStart = null;
                    Transform tfSegmentEnd   = null;

                    if(FirstNodeIsCoil())
                    {

                    }
                    else
                    {
                        tfSegmentStart = nNode == m_nFirstNonCoilNode ? RopeStart.transform : RopeNodes[nNode - 1].goNode.transform;
                        tfSegmentEnd   = RopeNodes[nNode].goNode.transform;
                    }

                    if(tfSegmentStart != null && tfSegmentEnd != null)
                    {
                        if(nLink == 0)
                        {
                            node.segmentLinks[nLink].transform.rotation    = LockStartEndInZAxis ? tfSegmentStart.rotation : Quaternion.LookRotation((tfSegmentEnd.position - tfSegmentStart.position).normalized);
                            node.segmentLinks[nLink].transform.parent      = LockStartEndInZAxis ? tfSegmentStart : node.segmentLinks[nLink].transform.parent;
                            node.segmentLinks[nLink].GetComponent<Rigidbody>().isKinematic = LockStartEndInZAxis || node.segmentLinks[nLink].GetComponent<Rigidbody>().isKinematic;
                        }
                        else if(nLink == node.segmentLinks.Length - 1)
                        {
                            node.segmentLinks[nLink].transform.position    = LockStartEndInZAxis ? (tfSegmentEnd.position - (tfSegmentEnd.forward * LinkLengths[nLinearLinkIndex])) : (tfSegmentEnd.position - ((tfSegmentEnd.position - tfSegmentStart.position).normalized * LinkLengths[nLinearLinkIndex]));
                            node.segmentLinks[nLink].transform.rotation    = LockStartEndInZAxis ? tfSegmentEnd.rotation : Quaternion.LookRotation((tfSegmentEnd.position - tfSegmentStart.position).normalized);
                            node.segmentLinks[nLink].transform.parent      = LockStartEndInZAxis ? tfSegmentEnd : node.segmentLinks[nLink].transform.parent;
                            node.segmentLinks[nLink].GetComponent<Rigidbody>().isKinematic = LockStartEndInZAxis || node.segmentLinks[nLink].GetComponent<Rigidbody>().isKinematic;
                        }
                    }

                    nLinearLinkIndex++;
                }
            }
        }
    }

    public void FillLinkMeshIndicesRope(int nLinearLinkIndex, int nTotalLinks, ref int[] indices, bool bBreakable, bool bBrokenLink = false)
    {
        if(bBreakable)
        {
            int nTriangleIndex        = nLinearLinkIndex * RopeSegmentSides * 2;
            int nLinkVertexIndexStart = (nLinearLinkIndex * (RopeSegmentSides + 1) * 4) + (RopeSegmentSides + 1);
            int nJumpVertices         = (RopeSegmentSides + 1) * 3;

            int nAdd = (bBrokenLink == false && (nLinearLinkIndex < (nTotalLinks - 1))) ? nJumpVertices : 0;
            
            for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
            {
                if(nSide < RopeSegmentSides)
                {
                    int nVertexIndex = nLinkVertexIndexStart + nSide;
                    
                    indices[nTriangleIndex * 3 + 2] = nVertexIndex;
                    indices[nTriangleIndex * 3 + 1] = nVertexIndex + nAdd + (RopeSegmentSides + 1);
                    indices[nTriangleIndex * 3 + 0] = nVertexIndex + 1;
                    indices[nTriangleIndex * 3 + 5] = nVertexIndex + 1;
                    indices[nTriangleIndex * 3 + 4] = nVertexIndex + nAdd + (RopeSegmentSides + 1);
                    indices[nTriangleIndex * 3 + 3] = nVertexIndex + nAdd + (RopeSegmentSides + 1) + 1;
                    nTriangleIndex += 2;
                }
            }
        }
        else
        {
            int nTriangleIndex        = nLinearLinkIndex * RopeSegmentSides * 2;
            int nLinkVertexIndexStart = (nLinearLinkIndex * (RopeSegmentSides + 1)) + (RopeSegmentSides + 1);

            for(int nSide = 0; nSide < RopeSegmentSides + 1; nSide++)
            {
                if(nSide < RopeSegmentSides)
                {
                    int nVertexIndex = nLinkVertexIndexStart + nSide;
                    
                    indices[nTriangleIndex * 3 + 2] = nVertexIndex;
                    indices[nTriangleIndex * 3 + 1] = nVertexIndex + RopeSegmentSides + 1;
                    indices[nTriangleIndex * 3 + 0] = nVertexIndex + 1;
                    indices[nTriangleIndex * 3 + 5] = nVertexIndex + 1;
                    indices[nTriangleIndex * 3 + 4] = nVertexIndex + RopeSegmentSides + 1;
                    indices[nTriangleIndex * 3 + 3] = nVertexIndex + 1 + RopeSegmentSides + 1;
                    nTriangleIndex += 2;
                }
            }
        }
    }

    public void FillLinkMeshIndicesSections(int nLinearLinkIndex, int nTotalLinks, ref int[] indices, bool bBreakable, bool bBrokenLink = false)
    {
        if(bBreakable)
        {
            int nTriangleIndex        = nLinearLinkIndex * 2 * (RopeSegmentSides - 2);
            int nLinkVertexIndexStart = (nLinearLinkIndex * (RopeSegmentSides + 1) * 4);
            int nJumpVertices         = (RopeSegmentSides + 1) * 2;

//          Debug.Log(nTriangleIndex + " " + nLinkVertexIndexStart + " " + nJumpVertices);

            for(int nBaseTri = 0; nBaseTri < RopeSegmentSides - 2; nBaseTri++)
            {
                indices[nTriangleIndex * 3 + 0] = nLinkVertexIndexStart;
                indices[nTriangleIndex * 3 + 1] = nLinkVertexIndexStart + (nBaseTri + 2);
                indices[nTriangleIndex * 3 + 2] = nLinkVertexIndexStart + (nBaseTri + 1);
                nTriangleIndex++;
            }

            int nAdd = (bBrokenLink == false && (nLinearLinkIndex < (nTotalLinks - 1))) ? nJumpVertices : 0;

            for(int nTopTri = 0; nTopTri < RopeSegmentSides - 2; nTopTri++)
            {
                indices[nTriangleIndex * 3 + 2] = nLinkVertexIndexStart + ((RopeSegmentSides + 1) * 3) + nAdd;
                indices[nTriangleIndex * 3 + 1] = nLinkVertexIndexStart + ((RopeSegmentSides + 1) * 3) + nAdd + (nTopTri + 2);
                indices[nTriangleIndex * 3 + 0] = nLinkVertexIndexStart + ((RopeSegmentSides + 1) * 3) + nAdd + (nTopTri + 1);
                nTriangleIndex++;
            }
        }
        else
        {
            int nTriangleIndex        = 0;
            int nLinkVertexIndexStart = 0;

            for(int nBaseTri = 0; nBaseTri < RopeSegmentSides - 2; nBaseTri++)
            {
                indices[nTriangleIndex * 3 + 0] = nLinkVertexIndexStart;
                indices[nTriangleIndex * 3 + 1] = nLinkVertexIndexStart + (nBaseTri + 2);
                indices[nTriangleIndex * 3 + 2] = nLinkVertexIndexStart + (nBaseTri + 1);
                nTriangleIndex++;
            }

            nLinkVertexIndexStart = ((TotalLinks + 1) * (RopeSegmentSides + 1)) + (RopeSegmentSides + 1);

            for(int nTopTri = 0; nTopTri < RopeSegmentSides - 2; nTopTri++)
            {
                indices[nTriangleIndex * 3 + 2] = nLinkVertexIndexStart;
                indices[nTriangleIndex * 3 + 1] = nLinkVertexIndexStart + (nTopTri + 2);
                indices[nTriangleIndex * 3 + 0] = nLinkVertexIndexStart + (nTopTri + 1);
                nTriangleIndex++;
            }
        }
    }

    public bool HasDynamicSegmentNodes()
    {
        if(RopeNodes == null)    return false;
        if(RopeNodes.Count == 0) return false;

        foreach(RopeNode node in RopeNodes)
        {
            if(node.goNode)
            {
                if(node.goNode.GetComponent<Rigidbody>())
                {
                    if(node.goNode.GetComponent<Rigidbody>().isKinematic == false)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void BeforeImportedBonesObjectRespawn()
    {
        if(ImportedBones != null)
        {
            foreach(RopeBone bone in ImportedBones)
            {
                if(bone.goBone != null)
                {
                    bone.goBone.transform.parent = bone.tfParent;
                }
            }
        }
    }

    public void AfterImportedBonesObjectRespawn()
    {
        if(ImportedBones != null)
        {
            foreach(RopeBone bone in ImportedBones)
            {
                if(bone.goBone != null)
                {
                    bone.goBone.transform.parent = bone.bIsStatic ? bone.tfNonBoneParent : this.transform;
                }
            }
        }
    }

    public void ExtendRope(ERopeExtensionMode eRopeExtensionMode, float fIncrement)
    {
        if(IsExtensible == false)
        {
            Debug.LogError("Rope can not be extended since the IsExtensible property has been marked as false");
            return;
        }

        if(eRopeExtensionMode == ERopeExtensionMode.CoilRotationIncrement && FirstNodeIsCoil() == false)
        {
            Debug.LogError("Rope can not be extended through coil rotation since no coil is present");
            return;
        }

        float fLinearIncrement = eRopeExtensionMode == ERopeExtensionMode.LinearExtensionIncrement ? fIncrement : 0.0f;
        float fRadius = m_fCurrentCoilRopeRadius;

        if(eRopeExtensionMode == ERopeExtensionMode.CoilRotationIncrement)
        {
            fLinearIncrement = m_fCurrentCoilRopeRadius * (fIncrement / 360.0f) * 2.0f * Mathf.PI;
        }

        float fActualRopeIncrement      = ExtendRopeLinear(fLinearIncrement);
        float fActualRopeIncrementAngle = (fActualRopeIncrement * 360.0f) / (2.0f * Mathf.PI * fRadius);

        if(Mathf.Approximately(fActualRopeIncrement, 0.0f) == false)
        {
            if(FirstNodeIsCoil())
            {
                CoilObject.transform.Rotate(GetAxisVector(CoilAxisRight, 1.0f) * fActualRopeIncrementAngle);
                SetupCoilBones(m_fCurrentCoilLength - fActualRopeIncrement);
            }
        }
    }

    public void RecomputeCoil()
    {
        SetupCoilBones(m_fCurrentCoilLength);
    }

    public GameObject BuildStaticMeshObject(out string strStatusMessage)
    {
        if(Application.isEditor && Application.isPlaying)
        {
            strStatusMessage = "Error: Rope can't be made static from the editor in play mode";
            return null;
        }

        if(RopeType == ERopeType.Procedural)
        {
            SkinnedMeshRenderer skin = GetComponent<SkinnedMeshRenderer>();

            if(skin == null)
            {
                strStatusMessage = "Error: Procedural rope has no skinned mesh renderer";
                return null;
            }

#if UNITY_EDITOR

            // Get mesh asset file name first

            string strMeshAssetPath = UnityEditor.EditorUtility.SaveFilePanelInProject("Save mesh asset", "mesh_" + gameObject.name + "_" + this.GetInstanceID().ToString() + ".asset", "asset", "Please enter a file name to save the mesh asset to");

#endif

            // Create static mesh from skinned mesh

            Mesh skinMesh = skin.sharedMesh;
            Mesh mesh     = new Mesh();

            int nVertices          = skin.sharedMesh.vertexCount;
            int nTrianglesRope     = skin.sharedMesh.GetTriangles(0).Length;
            int nTrianglesSections = skin.sharedMesh.GetTriangles(1).Length;

            Vector3[] av3VerticesSrc         = skinMesh.vertices;
            Vector2[] av2MappingSrc          = skinMesh.uv;
            Vector4[] av4TangentsSrc         = skinMesh.tangents;
            int[]     anTrianglesRopeSrc     = skinMesh.GetTriangles(0);
            int[]     anTrianglesSectionsSrc = skinMesh.GetTriangles(1);

            Vector3[] av3VerticesDst         = new Vector3[nVertices];
            Vector2[] av2MappingDst          = new Vector2[nVertices];
            Vector4[] av4TangentsDst         = skinMesh.tangents != null ? new Vector4[skinMesh.tangents.Length] : null;
            int[]     anTrianglesRopeDst     = new int    [nTrianglesRope];
            int[]     anTrianglesSectionsDst = new int    [nTrianglesSections];

            BoneWeight[] aBoneWeights = skinMesh.boneWeights;
            Matrix4x4[]  aBindPoses   = skinMesh.bindposes;
            Transform[]  aBones       = skin.bones;

            Vector3 v3Center = new Vector3(0.0f, 0.0f, 0.0f);

            for(int nVertex = 0; nVertex < nVertices; nVertex++)
            {
                BoneWeight bw = aBoneWeights[nVertex];

                av3VerticesDst[nVertex] = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 v3LocalVertex;

                if(Math.Abs(bw.weight0) > 0.00001f)
                {
                    v3LocalVertex = aBindPoses[bw.boneIndex0].MultiplyPoint3x4(av3VerticesSrc[nVertex]);
                    av3VerticesDst[nVertex] += aBones[bw.boneIndex0].transform.localToWorldMatrix.MultiplyPoint3x4(v3LocalVertex) * bw.weight0;
                }
                if(Math.Abs(bw.weight1) > 0.00001f)
                {
                    v3LocalVertex = aBindPoses[bw.boneIndex1].MultiplyPoint3x4(av3VerticesSrc[nVertex]);
                    av3VerticesDst[nVertex] += aBones[bw.boneIndex1].transform.localToWorldMatrix.MultiplyPoint3x4(v3LocalVertex) * bw.weight1;
                }
                if(Math.Abs(bw.weight2) > 0.00001f)
                {
                    v3LocalVertex = aBindPoses[bw.boneIndex2].MultiplyPoint3x4(av3VerticesSrc[nVertex]);
                    av3VerticesDst[nVertex] += aBones[bw.boneIndex2].transform.localToWorldMatrix.MultiplyPoint3x4(v3LocalVertex) * bw.weight2;
                }
                if(Math.Abs(bw.weight3) > 0.00001f)
                {
                    v3LocalVertex = aBindPoses[bw.boneIndex3].MultiplyPoint3x4(av3VerticesSrc[nVertex]);
                    av3VerticesDst[nVertex] += aBones[bw.boneIndex3].transform.localToWorldMatrix.MultiplyPoint3x4(v3LocalVertex) * bw.weight3;
                }

                v3Center += av3VerticesDst[nVertex];

                av2MappingDst[nVertex] = av2MappingSrc[nVertex];

                if(av4TangentsDst != null)
                {
                    if(av4TangentsDst.Length == nVertices)
                    {
                        av4TangentsDst[nVertex] = av4TangentsSrc[nVertex];
                    }
                }
            }

            if(nVertices > 0)
            {
                v3Center = v3Center / nVertices;
            }

            Vector3 v3Position = transform.position;
            transform.position = v3Center;

            for(int nVertex = 0; nVertex < nVertices; nVertex++)
            {
                av3VerticesDst[nVertex] = transform.InverseTransformPoint(av3VerticesDst[nVertex]);
            }

            transform.position = v3Position;

            for(int nIndex = 0; nIndex < nTrianglesRope; nIndex++)
            {
                anTrianglesRopeDst[nIndex] = anTrianglesRopeSrc[nIndex];
            }

            for(int nIndex = 0; nIndex < nTrianglesSections; nIndex++)
            {
                anTrianglesSectionsDst[nIndex] = anTrianglesSectionsSrc[nIndex];
            }

            mesh.vertices = av3VerticesDst;
            mesh.uv       = av2MappingDst;

            mesh.subMeshCount = 2;
            mesh.SetTriangles(anTrianglesRopeDst,     0);
            mesh.SetTriangles(anTrianglesSectionsDst, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            if(av4TangentsDst != null)
            {
                if(av4TangentsDst.Length == nVertices)
                {
                    mesh.tangents = av4TangentsDst;
                }
            }

#if UNITY_EDITOR
            UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
#endif

            // Add filter, renderer and collider

            GameObject newObject = new GameObject(gameObject.name + " (static)");

            newObject.transform.position = v3Center;
            newObject.transform.rotation = transform.rotation;

            MeshFilter   meshFilter   = newObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = mesh;

            Material[] ropeMaterials = new Material[2];
            ropeMaterials[0] = RopeMaterial;
            ropeMaterials[1] = RopeSectionMaterial;
            meshRenderer.sharedMaterials = ropeMaterials;

            newObject.isStatic = true;

            MeshCollider meshCollider = newObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh   = mesh;
            meshCollider.convex       = false;
            meshCollider.material     = RopePhysicsMaterial;

#if UNITY_EDITOR

            // Save mesh as asset

            if(strMeshAssetPath.Length != 0)
            {
                UnityEditor.AssetDatabase.CreateAsset(mesh, strMeshAssetPath);
                UnityEditor.AssetDatabase.Refresh();
            }

#endif

#if UNITY_3_5
            gameObject.SetActiveRecursively(false);
#else
            gameObject.SetActive(false);
#endif
            strStatusMessage = "Rope converted succesfully";

            return newObject;
        }
        else if(RopeType == ERopeType.LinkedObjects)
        {
            if(LinkObject == null)
            {
                strStatusMessage = "Error: LinkObject not specified. Can't continue.";
                return null;
            }

#if UNITY_EDITOR

            // Get mesh asset file name first

            string strMeshAssetPath = UnityEditor.EditorUtility.SaveFilePanelInProject("Save mesh asset", "mesh_" + gameObject.name + "_" + this.GetInstanceID().ToString() + ".asset", "asset", "Please enter a file name to save the mesh asset to");

#endif

            // Get materials

            Renderer   linkRenderer   = LinkObject.GetComponent<Renderer>();
            MeshFilter linkMeshFilter = LinkObject.GetComponent<MeshFilter>();

            if(linkRenderer == null)
            {
                strStatusMessage = "Error: LinkObject has no Renderer. Can't continue.";
                return null;
            }

            if(linkMeshFilter == null)
            {
                strStatusMessage = "Error: LinkObject has no Mesh Filter. Can't continue.";
                return null;
            }

            if(linkMeshFilter.sharedMesh == null)
            {
                strStatusMessage = "Error: LinkObject has no mesh. Can't continue.";
                return null;
            }

            Material[] ropeMaterials = new Material[linkRenderer.sharedMaterials.Length];

            for(int i = 0; i < linkRenderer.sharedMaterials.Length; i++)
            {
                ropeMaterials[i] = linkRenderer.sharedMaterials[i];
            }

            // Build combine list

            List<CombineInstance> listCombineInstances = new List<CombineInstance>();

            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                {
                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.mesh      = linkMeshFilter.sharedMesh;
                    combineInstance.transform = node.segmentLinks[nLink].transform.localToWorldMatrix;

                    listCombineInstances.Add(combineInstance);
                }
            }

            // Create new object

            GameObject newObject = new GameObject(gameObject.name + " (static)");

            MeshFilter   meshFilter   = newObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.CombineMeshes(listCombineInstances.ToArray());

            meshRenderer.sharedMaterials = ropeMaterials;

            newObject.isStatic = true;

            // Create new transform centered on the object and recompute local vertices

            Vector3[] aVertices = meshFilter.sharedMesh.vertices;
            Vector3 v3Center = Vector3.zero;

            for(int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
            {
                aVertices[i] = transform.TransformPoint(aVertices[i]);
                v3Center += aVertices[i];
            }

            if(meshFilter.sharedMesh.vertexCount > 1)
            {
                v3Center = v3Center / meshFilter.sharedMesh.vertexCount;
            }

            newObject.transform.position = v3Center;
            newObject.transform.rotation = transform.rotation;

            for(int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
            {
                aVertices[i] = newObject.transform.InverseTransformPoint(aVertices[i]);
            }

            meshFilter.sharedMesh.vertices = aVertices;
            meshFilter.sharedMesh.RecalculateBounds();

#if UNITY_EDITOR

            if(meshFilter.sharedMesh.uv2 == null)
            {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
            }
            else if(meshFilter.sharedMesh.uv2.Length != meshFilter.sharedMesh.vertexCount)
            {
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
            }
#endif

            // Add collider

            MeshCollider meshCollider = newObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh   = meshFilter.sharedMesh;
            meshCollider.convex       = false;
            meshCollider.material     = RopePhysicsMaterial;

#if UNITY_EDITOR

            // Save mesh as asset

            if(strMeshAssetPath.Length != 0)
            {
                UnityEditor.AssetDatabase.CreateAsset(meshFilter.sharedMesh, strMeshAssetPath);
                UnityEditor.AssetDatabase.Refresh();
            }

#endif

#if UNITY_3_5
            gameObject.SetActiveRecursively(false);
#else
            gameObject.SetActive(false);
#endif

            strStatusMessage = "Rope converted succesfully";

            return newObject;
        }
        else if(RopeType == ERopeType.ImportBones)
        {
            strStatusMessage = "Error: ImportBones rope type not supported";
            return null;
        }

        strStatusMessage = "Error: Unknown rope type not supported";
        return null;
    }

    public void MoveNodeUp(int nNode)
    {
        if(RopeNodes != null)
        {
            if(nNode > 0 && nNode < RopeNodes.Count)
            {
                RopeNode old = RopeNodes[nNode];
                RopeNodes[nNode] = RopeNodes[nNode - 1];
                RopeNodes[nNode - 1] = old;
            }
        }
    }

    public void MoveNodeDown(int nNode)
    {
        if(RopeNodes != null)
        {
            if(nNode >= 0 && nNode < RopeNodes.Count - 1)
            {
                RopeNode old = RopeNodes[nNode];
                RopeNodes[nNode] = RopeNodes[nNode + 1];
                RopeNodes[nNode + 1] = old;
            }
        }
    }

    public void CreateNewNode(int nNode)
    {
        if(RopeNodes == null)
        {
            RopeNodes = new List<RopeNode>();
        }

        RopeNodes.Insert(nNode + 1, new RopeNode());
    }

    public void RemoveNode(int nNode)
    {
        if(RopeNodes == null)
        {
            return;
        }

        RopeNodes.RemoveAt(nNode);
    }

    public bool FirstNodeIsCoil()
    {
        if(RopeNodes != null)
        {
            if(RopeNodes.Count > 0)
            {
                if(RopeNodes[0].bIsCoil == true)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void CheckAddCoilNode()
    {
        if(RopeType == ERopeType.Procedural && IsExtensible && HasACoil)
        {
            if(CoilObject != null && RopeStart)
            {
                if(RopeNodes[0].bIsCoil == false)
                {
                    RopeNodes.Insert(0, new RopeNode());

                    if(CoilNumBones < 1)
                    {
                        CoilNumBones = 1;
                    }

                    RopeNodes[0].goNode        = CoilObject;
                    RopeNodes[0].fLength       = ExtensibleLength;
                    RopeNodes[0].fTotalLength  = RopeNodes[0].fLength;
                    RopeNodes[0].nNumLinks     = CoilNumBones;
                    RopeNodes[0].nTotalLinks   = RopeNodes[0].nNumLinks;
                    RopeNodes[0].eColliderType = EColliderType.None;
                    RopeNodes[0].nColliderSkip = 0;
                    RopeNodes[0].bFold         = true;
                    RopeNodes[0].bIsCoil       = true;

                    m_afCoilBoneRadiuses       = new float[RopeNodes[0].nTotalLinks];
                    m_afCoilBoneAngles         = new float[RopeNodes[0].nTotalLinks];
                    m_afCoilBoneX              = new float[RopeNodes[0].nTotalLinks];
                }

                m_nFirstNonCoilNode = 1;
            }
        }
    }

    void CheckDelCoilNode()
    {
        if(RopeNodes[0].bIsCoil == true)
        {
            RopeNodes.RemoveAt(0);

            m_afCoilBoneRadiuses = null;
            m_afCoilBoneAngles   = null;
            m_afCoilBoneX        = null;
        }

        m_nFirstNonCoilNode = 0;
    }

    void CreateRopeJoints(bool bCheckIfBroken = false)
    {
        if(RopeNodes == null)     return;
        if(RopeNodes.Count  == 0) return;
        if(Deleted == true) return;

        if(RopeType == ERopeType.ImportBones)
        {
            if(ImportedBones == null)
            {
                return;
            }

            if(ImportedBones.Length == 0)
            {
                return;
            }
        }

        foreach(RopeNode node in RopeNodes)
        {
            if(node.segmentLinks == null)
            {
                return;
            }
        }

        // Create rigidbodies if necessary

        if(RopeStart != null)
        {
            if(RopeStart.GetComponent<Rigidbody>() == null)
            {
                RopeStart.AddComponent<Rigidbody>();
                RopeStart.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            if(node.goNode != null)
            {
                if(node.goNode.GetComponent<Rigidbody>() == null)
                {
                    node.goNode.AddComponent<Rigidbody>();
                    node.goNode.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }

        // Store transforms for later

        int nLinearLinkIndex = 0;
        int nLinearLinkIndexAux = 0;

        Vector3[]    av3LinkPositions = new Vector3[TotalLinks];
        Quaternion[] aqLinkRotations  = new Quaternion[TotalLinks];

        Vector3      v3LocalStartRope = RopeStart != null ? RopeStart.transform.localPosition : Vector3.zero;
        Quaternion   qLocalStartRope  = RopeStart != null ? RopeStart.transform.localRotation : Quaternion.identity;
        Vector3[]    av3NodePositions = new Vector3[RopeNodes.Count];
        Quaternion[] aqNodeRotations  = new Quaternion[RopeNodes.Count];

        if(m_bRopeStartInitialOrientationInitialized && RopeStart != null)
        {
            RopeStart.transform.localPosition = m_v3InitialRopeStartLocalPos;
            RopeStart.transform.localRotation = m_qInitialRopeStartLocalRot;
        }

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            if(node.bInitialOrientationInitialized && node.goNode != null)
            {
                av3NodePositions[nNode] = node.goNode.transform.localPosition;
                aqNodeRotations[nNode]  = node.goNode.transform.localRotation;
                node.goNode.transform.localPosition = node.v3InitialLocalPos;
                node.goNode.transform.localRotation = node.qInitialLocalRot;
            }
        }

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            GameObject goSegmentStart = null;
            GameObject goSegmentEnd   = null;

            if(FirstNodeIsCoil() && nNode == 0)
            {
                goSegmentStart = CoilObject;
                goSegmentEnd   = RopeStart;
            }
            else
            {
                goSegmentStart = nNode == m_nFirstNonCoilNode ? RopeStart : RopeNodes[nNode - 1].goNode;
                goSegmentEnd   = RopeNodes[nNode].goNode;
            }

            float fLinkLength      = node.fLength / node.nNumLinks;
            float fSegmentLength   = fLinkLength * (node.segmentLinks.Length - 1);

            for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
            {
                if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
                {
                    float fLinkT = (float)nLink / (node.segmentLinks.Length == 1 ? 1.0f : (node.segmentLinks.Length - 1.0f));

                    if(nLink == 0)
                    {
                        node.m_v3LocalDirectionUp = goSegmentStart.transform.InverseTransformDirection(node.segmentLinks[nLink].transform.up);
                    }

                    Vector3 v3SegmentDir = (goSegmentEnd.transform.position - goSegmentStart.transform.position).normalized;

                    if(node.nTotalLinks > node.nNumLinks && node.m_bExtensionInitialized == false)
                    {
                        node.segmentLinks[nLink].transform.rotation = Quaternion.LookRotation((goSegmentEnd.transform.position - goSegmentStart.transform.position).normalized);

                        if(nLink < node.m_nExtensionLinkIn)
                        {
                            // Fixed kinematic extensible link
                            node.segmentLinks[nLink].transform.position = goSegmentStart.transform.position;
                            node.segmentLinks[nLink].transform.parent = nNode > m_nFirstNonCoilNode ? RopeNodes[nNode - 1].goNode.transform : RopeStart.transform;
                            node.segmentLinks[nLink].GetComponent<Rigidbody>().isKinematic = true;

                            UltimateRopeLink ropeLink = node.segmentLinks[nLink].GetComponent<UltimateRopeLink>();
                            if(ropeLink != null) ropeLink.ExtensibleKinematic = true;
                        }
                        else
                        {
                            // Free link
                            float fFreeT = (float)(nLink - node.m_nExtensionLinkIn) / (float)(node.nNumLinks > 1 ? node.nNumLinks - 1 : 1.0f);
                            node.segmentLinks[nLink].transform.position = Vector3.Lerp(goSegmentStart.transform.position + (v3SegmentDir * fLinkLength), goSegmentEnd.transform.position - (v3SegmentDir * fLinkLength), fFreeT);
                            node.segmentLinks[nLink].GetComponent<Rigidbody>().isKinematic = false;

                            UltimateRopeLink ropeLink = node.segmentLinks[nLink].GetComponent<UltimateRopeLink>();
                            if(ropeLink != null) ropeLink.ExtensibleKinematic = false;
                        }
                    }

                    av3LinkPositions[nLinearLinkIndexAux] = node.segmentLinks[nLink].transform.position;
                    aqLinkRotations[nLinearLinkIndexAux]  = node.segmentLinks[nLink].transform.rotation;

                    node.segmentLinks[nLink].transform.position = Vector3.Lerp(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, fSegmentLength), fLinkT);
                    node.segmentLinks[nLink].transform.rotation = Quaternion.identity;

                    if(RopeType == ERopeType.LinkedObjects)
                    {
                        node.segmentLinks[nLink].transform.rotation *= GetLinkedObjectLocalRotation(LinkTwistAngleStart + LinkTwistAngleIncrement * nLink);
                    }

                    nLinearLinkIndexAux++;
                }
                else if(RopeType == ERopeType.ImportBones)
                {
                    av3LinkPositions[nLink] = ImportedBones[nLink].goBone.transform.position;
                    aqLinkRotations[nLink]  = ImportedBones[nLink].goBone.transform.rotation;

                    if(ImportedBones[nLink].tfNonBoneParent != null)
                    {
                        Transform tfParent = ImportedBones[nLink].goBone.transform.parent;
                        ImportedBones[nLink].goBone.transform.parent = ImportedBones[nLink].tfNonBoneParent;
                        ImportedBones[nLink].goBone.transform.localPosition = ImportedBones[nLink].v3OriginalLocalPos;
                        ImportedBones[nLink].goBone.transform.localRotation = ImportedBones[nLink].qOriginalLocalRot;
                        ImportedBones[nLink].goBone.transform.parent = tfParent;
                        ImportedBones[nLink].goBone.transform.localScale = ImportedBones[nLink].v3OriginalLocalScale;
                    }
                }

                bool bCreateJoint = (bCheckIfBroken && node.linkJointBreaksProcessed[nLink] == true) == false;

                if(RopeType == ERopeType.ImportBones)
                {
                    // Don't generate a joint if this bone and the previous are static

                    bool bPreviousDynamic = true;

                    if(nLink > 0)
                    {
                        bPreviousDynamic = ImportedBones[nLink - 1].goBone.GetComponent<Rigidbody>().isKinematic == false;
                    }

                    if(bPreviousDynamic == false && ImportedBones[nLink].goBone.GetComponent<Rigidbody>().isKinematic == true)
                    {
                        bCreateJoint = false;
                    }
                }

                if(bCreateJoint && nLink > 0 && node.bIsCoil == false)
                {
                    node.linkJoints[nLink] = CreateJoint(node.segmentLinks[nLink], node.segmentLinks[nLink - 1], node.segmentLinks[nLink].transform.position);
                    node.linkJointBreaksProcessed[nLink] = false;
                }
                else
                {
                    node.linkJoints[nLink] = null;
                }
            }

            // Reposition

            float fRemainingLength = RopeType == ERopeType.ImportBones ? 0.0f : ((goSegmentEnd.transform.position - goSegmentStart.transform.position).magnitude - fLinkLength) / (goSegmentEnd.transform.position - goSegmentStart.transform.position).magnitude;

            if(fRemainingLength < 0.0f) fRemainingLength = 0.0f;

            for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
            {
                if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
                {
                    float fLinkT = (float)nLink / (node.segmentLinks.Length == 1 ? 1.0f : (node.segmentLinks.Length - 1.0f));

                    if(Vector3.Distance(goSegmentStart.transform.position, goSegmentEnd.transform.position) < 0.001f)
                    {
                        node.segmentLinks[nLink].transform.position = goSegmentStart.transform.position;
                        node.segmentLinks[nLink].transform.rotation = goSegmentStart.transform.rotation;
                    }
                    else
                    {
                        node.segmentLinks[nLink].transform.position = Vector3.Lerp(goSegmentStart.transform.position, goSegmentEnd.transform.position, fLinkT * fRemainingLength);
                        node.segmentLinks[nLink].transform.rotation = Quaternion.LookRotation((goSegmentEnd.transform.position - goSegmentStart.transform.position).normalized);
                    }

                    if(RopeType == ERopeType.LinkedObjects)
                    {
                        node.segmentLinks[nLink].transform.rotation *= GetLinkedObjectLocalRotation(LinkTwistAngleStart + LinkTwistAngleIncrement * nLink);
                    }
                }
                
                nLinearLinkIndex++;
            }

            if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
            {
                if(node.bIsCoil == false)
                {
                    if((bCheckIfBroken && node.linkJointBreaksProcessed[0] == true) == false)
                    {
                        if(node.nTotalLinks == node.nNumLinks)
                        {
                            node.linkJoints[0] = CreateJoint(node.segmentLinks[0], goSegmentStart, goSegmentStart.transform.position);
                            node.linkJointBreaksProcessed[0] = false;
                        }
                        else
                        {
                            // First node doesnt need a link since this is an extensible rope segment and if the goNode is rigidbody nonkinematic it won't move freely
                            node.linkJoints[0] = null;
                            node.linkJointBreaksProcessed[0] = true;
                        }
                    }
                    else
                    {
                        node.linkJoints[0] = null;
                    }

                    if((bCheckIfBroken && node.linkJointBreaksProcessed[node.segmentLinks.Length] == true) == false)
                    {
                        node.linkJoints[node.segmentLinks.Length] = CreateJoint(node.segmentLinks[node.segmentLinks.Length - 1], goSegmentEnd, goSegmentEnd.transform.position);
                        node.linkJointBreaksProcessed[node.segmentLinks.Length] = false;
                    }
                    else
                    {
                        node.linkJoints[node.segmentLinks.Length] = null;
                    }
                }
            }
            else if(RopeType == ERopeType.ImportBones)
            {
                node.linkJointBreaksProcessed[0] = true;
            }

            if(node.nTotalLinks > node.nNumLinks && node.m_bExtensionInitialized == false)
            {
                node.m_bExtensionInitialized = true;
            }
        }

        // Restore transforms

        if(m_bRopeStartInitialOrientationInitialized && RopeStart != null)
        {
            RopeStart.transform.localPosition = v3LocalStartRope;
            RopeStart.transform.localRotation = qLocalStartRope;
        }

        for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
        {
            RopeNode node = RopeNodes[nNode];

            if(node.bInitialOrientationInitialized && node.goNode != null)
            {
                node.goNode.transform.localPosition = av3NodePositions[nNode];
                node.goNode.transform.localRotation = aqNodeRotations[nNode];
            }
        }

        nLinearLinkIndexAux = 0;

        if(RopeType == ERopeType.Procedural || RopeType == ERopeType.LinkedObjects)
        {
            for(int nNode = 0; nNode < RopeNodes.Count; nNode++)
            {
                RopeNode node = RopeNodes[nNode];

                for(int nLink = 0; nLink < node.segmentLinks.Length; nLink++)
                {
                    node.segmentLinks[nLink].transform.position = av3LinkPositions[nLinearLinkIndexAux];
                    node.segmentLinks[nLink].transform.rotation = aqLinkRotations[nLinearLinkIndexAux];

                    nLinearLinkIndexAux++;
                }
            }
        }
        else if(RopeType == ERopeType.ImportBones)
        {
            for(int nLink = 0; nLink < ImportedBones.Length; nLink++)
            {
                ImportedBones[nLink].goBone.transform.position = av3LinkPositions[nLink];
                ImportedBones[nLink].goBone.transform.rotation = aqLinkRotations[nLink];
            }
        }

        CheckNeedsStartExitLockZ();
    }

    ConfigurableJoint CreateJoint(GameObject goObject, GameObject goConnectedTo, Vector3 v3Pivot)
    {
        ConfigurableJoint joint = goObject.AddComponent<ConfigurableJoint>();

        SetupJoint(joint);
        joint.connectedBody = goConnectedTo.GetComponent<Rigidbody>();
        joint.anchor = goObject.transform.InverseTransformPoint(v3Pivot);

        return joint;
    }

    void SetupJoint(ConfigurableJoint joint)
    {
      SoftJointLimit jointLimit = new SoftJointLimit();
      jointLimit.contactDistance = 0.0f;
      jointLimit.bounciness = 0.0f;

      JointDrive jointDrive = new JointDrive();
      jointDrive.mode = JointDriveMode.Position;
      jointDrive.positionSpring = LinkJointSpringValue;
      jointDrive.positionDamper = LinkJointDamperValue;
      jointDrive.maximumForce = LinkJointMaxForceValue;

      joint.axis = Vector3.right;
      joint.secondaryAxis = Vector3.up;
      joint.breakForce = LinkJointBreakForce;
      joint.breakTorque = LinkJointBreakTorque;

      joint.xMotion = ConfigurableJointMotion.Locked;
      joint.yMotion = ConfigurableJointMotion.Locked;
      joint.zMotion = ConfigurableJointMotion.Locked;
      joint.angularXMotion = Mathf.Approximately(LinkJointAngularXLimit, 0.0f) == false ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
      joint.angularYMotion = Mathf.Approximately(LinkJointAngularYLimit, 0.0f) == false ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
      joint.angularZMotion = Mathf.Approximately(LinkJointAngularZLimit, 0.0f) == false ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;

      jointLimit.limit = -LinkJointAngularXLimit;
      joint.lowAngularXLimit = jointLimit;

      jointLimit.limit = LinkJointAngularXLimit;
      joint.highAngularXLimit = jointLimit;

      jointLimit.limit = LinkJointAngularYLimit;
      joint.angularYLimit = jointLimit;

      jointLimit.limit = LinkJointAngularZLimit;
      joint.angularZLimit = jointLimit;

      joint.angularXDrive = jointDrive;
      joint.angularYZDrive = jointDrive;
      /*
              joint.projectionMode     = JointProjectionMode.PositionAndRotation;
              joint.projectionDistance = 0.1f;
              joint.projectionAngle    = 0.1f;*/
    }

    Vector3 GetAxisVector(EAxis eAxis, float fLength)
    {
        if(eAxis == EAxis.X)      return new Vector3(fLength, 0.0f, 0.0f);
        if(eAxis == EAxis.Y)      return new Vector3(0.0f, fLength, 0.0f);
        if(eAxis == EAxis.Z)      return new Vector3(0.0f, 0.0f, fLength);
        if(eAxis == EAxis.MinusX) return new Vector3(-fLength, 0.0f, 0.0f);
        if(eAxis == EAxis.MinusY) return new Vector3(0.0f, -fLength, 0.0f);
        if(eAxis == EAxis.MinusZ) return new Vector3(0.0f, 0.0f, -fLength);

        return Vector3.zero;
    }

    float ExtendRopeLinear(float fLinearIncrement)
    {
        if(fLinearIncrement > 0.0f && Mathf.Approximately(m_fCurrentExtension, ExtensibleLength)) return 0.0f;
        if(fLinearIncrement < 0.0f && Mathf.Approximately(m_fCurrentExtension, 0.0f))             return 0.0f;

        RopeNode nodeExtension = RopeNodes[RopeNodes.Count - 1];

        bool  bLastStep               = false;
        float fCurrentExtensionBefore = m_fCurrentExtension;
        float fLinkSeparation         = nodeExtension.fLength / nodeExtension.nNumLinks;

        Transform tfParent = (RopeNodes.Count - 1) > m_nFirstNonCoilNode ? RopeNodes[RopeNodes.Count - 2].goNode.transform : RopeStart.transform;

        Vector3 v3ExtensionDirection = tfParent.TransformDirection(nodeExtension.m_v3LocalDirectionForward);

        if(fLinearIncrement < 0.0f)
        {
            // In

            while(fLinearIncrement < 0.0f && nodeExtension.m_nExtensionLinkIn > 0 && nodeExtension.m_nExtensionLinkIn < nodeExtension.segmentLinks.Length - 1 && bLastStep == false)
            {
                float fChange = Mathf.Max(-fLinkSeparation * 0.5f, fLinearIncrement);

                if(Mathf.Abs(fChange) > m_fCurrentExtension)
                {
                    fChange   = -m_fCurrentExtension;
                    bLastStep = true;
                }

                nodeExtension.m_fExtensionRemainderIn += fChange;

                if(nodeExtension.m_fExtensionRemainderIn < -fLinkSeparation)
                {
                    fChange += Mathf.Abs(nodeExtension.m_fExtensionRemainderIn - (-fLinkSeparation));
                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn].transform.position = nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn - 1].transform.position;
                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn].transform.rotation = nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn - 1].transform.rotation;

                    SetExtensibleLinkToKinematic(nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn], true);

                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn].transform.parent = tfParent;

                    nodeExtension.m_nExtensionLinkIn++;
                    nodeExtension.m_nExtensionLinkOut = nodeExtension.m_nExtensionLinkIn - 1;
                    nodeExtension.m_fExtensionRemainderIn  = 0.0f;
                    nodeExtension.m_fExtensionRemainderOut = 0.0f;
                }
                else
                {
                    float fInT = -nodeExtension.m_fExtensionRemainderIn / fLinkSeparation;

                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn].transform.position = tfParent.position + (v3ExtensionDirection * (fLinkSeparation + nodeExtension.m_fExtensionRemainderIn));
                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn].transform.rotation = Quaternion.Slerp(nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn].transform.rotation, nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn - 1].transform.rotation, fInT);

                    SetExtensibleLinkToKinematic(nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn], true);

                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkIn].transform.parent = tfParent;
                    nodeExtension.m_nExtensionLinkOut      = nodeExtension.m_nExtensionLinkIn;
                    nodeExtension.m_fExtensionRemainderOut = fLinkSeparation + nodeExtension.m_fExtensionRemainderIn;
                }

                fLinearIncrement    -= fChange;
                m_fCurrentExtension += fChange;
            }
        }
        else if(fLinearIncrement > 0.0f)
        {
            // Out

            while(fLinearIncrement > 0.0f && nodeExtension.m_nExtensionLinkOut > 0 && nodeExtension.m_nExtensionLinkOut < nodeExtension.segmentLinks.Length - 1 && bLastStep == false)
            {
                float fChange = Mathf.Min(fLinkSeparation * 0.5f, fLinearIncrement);

                if(m_fCurrentExtension + fChange > ExtensibleLength)
                {
                    fChange   = (ExtensibleLength - m_fCurrentExtension);
                    bLastStep = true;
                }

                nodeExtension.m_fExtensionRemainderOut += fChange;

                if(nodeExtension.m_fExtensionRemainderOut > fLinkSeparation)
                {
                    fChange -= nodeExtension.m_fExtensionRemainderOut - fLinkSeparation;

                    SetExtensibleLinkToKinematic(nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkOut], false);

                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkOut].transform.parent = this.transform;

                    nodeExtension.m_nExtensionLinkOut--;
                    nodeExtension.m_nExtensionLinkIn = nodeExtension.m_nExtensionLinkOut + 1;
                    nodeExtension.m_fExtensionRemainderIn  = 0.0f;
                    nodeExtension.m_fExtensionRemainderOut = 0.0f;
                }
                else
                {
                    nodeExtension.segmentLinks[nodeExtension.m_nExtensionLinkOut].transform.position = tfParent.position + (v3ExtensionDirection * nodeExtension.m_fExtensionRemainderOut);
                    nodeExtension.m_nExtensionLinkIn      = nodeExtension.m_nExtensionLinkOut;
                    nodeExtension.m_fExtensionRemainderIn = -fLinkSeparation + nodeExtension.m_fExtensionRemainderOut;
                }

                fLinearIncrement    -= fChange;
                m_fCurrentExtension += fChange;
            }
        }

        return m_fCurrentExtension - fCurrentExtensionBefore;
    }

    void SetExtensibleLinkToKinematic(GameObject link, bool bKinematic)
    {
        if(link.GetComponent<Rigidbody>().isKinematic != bKinematic)
        {
            link.GetComponent<Rigidbody>().isKinematic = bKinematic;

            if(link.GetComponent<Collider>() != null)
            {
                link.GetComponent<Collider>().enabled = !bKinematic;
            }

            UltimateRopeLink ropeLink = link.GetComponent<UltimateRopeLink>();
            if(ropeLink != null) ropeLink.ExtensibleKinematic = bKinematic;

            ConfigurableJoint joint = link.GetComponent<ConfigurableJoint>();

            if(joint)
            {
                if(bKinematic == true)
                {
                    joint.breakForce  = Mathf.Infinity;
                    joint.breakTorque = Mathf.Infinity;
                }
                else
                {
                    joint.breakForce  = LinkJointBreakForce;
                    joint.breakTorque = LinkJointBreakTorque;
                }
            }
        }
    }

    void SetupCoilBones(float fCoilLength)
    {
        float fLengthCounter      = 0.0f;

        float fCurrentBoneX       = (CoilWidth    * -0.5f) + (RopeDiameter * 0.5f); // Current X position counting from the center when computing bones around the coil.
        float fCurrentRadius      = (CoilDiameter *  0.5f) + (RopeDiameter * 0.5f);
        float fCurrentAngle       =  0.0f;
        float fDirectionX         =  1.0f;  // Current X direction when computing bones around the coil (-1 left, 1 right)
        float fDegreesOnSide      = -1.0f;  // When a side has been reached, we need to do a whole turn around with increasing radius and then go the other way again. Set this to 360.0f to perform an increasing radius turn.

        float fLengthCoilToStart  = Vector3.Distance(CoilObject.transform.position, RopeStart.transform.position) + CoilDiameter;
        float fActualCoilLength   = fCoilLength + fLengthCoilToStart;

        float fEndToCoilLength    = 0.0f;
        float fRemainderAngle     = 0.0f;
        float fRemainderLength    = 0.0f;
        int   nBonesOnCoil        = 0;

        RopeNode   coilNode    = RopeNodes[0];
        Vector3    v3CoilPos   = coilNode.goNode.transform.localPosition;
        Quaternion qCoilRot    = coilNode.goNode.transform.localRotation;
        Vector3    v3CoilScale = coilNode.goNode.transform.localScale;

        if(coilNode.bInitialOrientationInitialized)
        {
            coilNode.goNode.transform.localPosition = coilNode.v3InitialLocalPos;
            coilNode.goNode.transform.localRotation = coilNode.qInitialLocalRot;
            coilNode.goNode.transform.localScale    = coilNode.v3InitialLocalScale;
        }

        Vector3 v3CoilRight   = -CoilObject.transform.TransformDirection(GetAxisVector(CoilAxisRight, 1.0f));
        Vector3 v3CoilUp      = CoilObject.transform.TransformDirection(GetAxisVector(CoilAxisUp,    1.0f));
        Vector3 v3CoilForward = Vector3.Cross(v3CoilUp, v3CoilRight);

        Quaternion quatOrientation = Quaternion.LookRotation(v3CoilForward, v3CoilUp);

        coilNode.goNode.transform.localPosition = v3CoilPos;
        coilNode.goNode.transform.localRotation = qCoilRot;
        coilNode.goNode.transform.localScale    = v3CoilScale;

        float fLinkLength = (RopeNodes[0].fLength + fLengthCoilToStart) / RopeNodes[0].nNumLinks;

        for(int nBone = 0; nBone < RopeNodes[0].segmentLinks.Length; nBone++)
        {
            m_afCoilBoneRadiuses[nBone] = fCurrentRadius;
            m_afCoilBoneAngles[nBone]   = fCurrentAngle;
            m_afCoilBoneX[nBone]        = fCurrentBoneX;

            Vector3 v3CurrentEnd = CoilObject.transform.position + (v3CoilUp * fCurrentRadius) + (v3CoilRight * fCurrentBoneX);
            fEndToCoilLength = (v3CurrentEnd - RopeStart.transform.position).magnitude;

            fLengthCounter += fLinkLength;
            nBonesOnCoil++;

            float fStopCoilLength = fActualCoilLength - fEndToCoilLength;

            if(fLengthCounter > fStopCoilLength)
            {
                fRemainderLength = fLengthCounter - fStopCoilLength;
                fRemainderAngle  = fCurrentAngle - (fRemainderLength / (fCurrentRadius * Mathf.PI * 2.0f)) * 360.0f;
                m_fCurrentCoilRopeRadius = fCurrentRadius;
                m_fCurrentCoilTurnsLeft  = fCurrentAngle / 360.0f;
                break;
            }

            // Compute the angle increment and the bones per turn
            
            float fAngleIncrement = (fLinkLength / (fCurrentRadius * Mathf.PI * 2.0f)) * 360.0f;
            float fBonesPerTurn   = ((fCurrentRadius * Mathf.PI * 2.0f) / fLinkLength);

            fCurrentAngle += fAngleIncrement;
            
            if(fDegreesOnSide > 0.0f)
            {
                // We are performing a turn with increasing radius at the side of the Coil

                fCurrentRadius += RopeDiameter / fBonesPerTurn; 
                fDegreesOnSide -= fAngleIncrement;
            }
            else
            {
                // Non side of the Coil
                
                fCurrentBoneX  += (RopeDiameter * fDirectionX) / fBonesPerTurn;
            }

            if(fDirectionX > 0.0f && (fCurrentBoneX > ((CoilWidth * 0.5f) - (RopeDiameter * 0.5f))))
            {
                // Reached right side?
                
                fCurrentBoneX  = (CoilWidth * 0.5f) - (RopeDiameter * 0.5f);
                fDegreesOnSide = 360.0f;
                fDirectionX    = -1.0f;
            }

            if(fDirectionX < 0.0f && (fCurrentBoneX < ((CoilWidth * -0.5f) + (RopeDiameter * 0.5f))))
            {
                // Reached left side?
                
                fCurrentBoneX  = (CoilWidth * -0.5f) + (RopeDiameter * 0.5f);
                fDegreesOnSide = 360.0f;
                fDirectionX    = 1.0f;
            }
        }

        for(int nBone = 0; nBone < nBonesOnCoil; nBone++)
        {
            m_afCoilBoneAngles[nBone] -= fRemainderAngle;

            RopeNodes[0].segmentLinks[nBone].transform.position = CoilObject.transform.position + (v3CoilUp* m_afCoilBoneRadiuses[nBone]);
            RopeNodes[0].segmentLinks[nBone].transform.rotation = quatOrientation;//CoilObject.transform.rotation;
            RopeNodes[0].segmentLinks[nBone].transform.RotateAround(CoilObject.transform.position, -v3CoilRight, m_afCoilBoneAngles[nBone]);
            RopeNodes[0].segmentLinks[nBone].transform.position += v3CoilRight * m_afCoilBoneX[nBone];
        }

        Vector3 v3CoilStart     = CoilObject.transform.position + (v3CoilUp * fCurrentRadius) + (v3CoilRight * fCurrentBoneX);//
        Vector3 v3ToEndDirection = (RopeStart.transform.position - v3CoilStart).normalized;

        fLengthCounter   = (RopeNodes[0].segmentLinks[nBonesOnCoil - 1].transform.position - v3CoilStart).magnitude;
        float fMaxLength = (RopeStart.transform.position - v3CoilStart).magnitude;

        Quaternion qOrientationEnd = Quaternion.LookRotation((RopeStart.transform.position - CoilObject.transform.position).normalized, v3CoilUp);
        
        for(int nBone = nBonesOnCoil; nBone < RopeNodes[0].segmentLinks.Length; nBone++)
        {
            fLengthCounter += fLinkLength;

            if(fLengthCounter < fMaxLength)
            {
                RopeNodes[0].segmentLinks[nBone].transform.position = v3CoilStart + v3ToEndDirection * fLengthCounter;
                RopeNodes[0].segmentLinks[nBone].transform.rotation = qOrientationEnd;
            }
            else
            {
                RopeNodes[0].segmentLinks[nBone].transform.position = RopeStart.transform.position;
                RopeNodes[0].segmentLinks[nBone].transform.rotation = qOrientationEnd;
            }
        }

        m_fCurrentCoilLength = fCoilLength;
    }

    Quaternion GetLinkedObjectLocalRotation(float fTwistAngle = 0.0f)
    {
        if(LinkAxis == EAxis.X)      return Quaternion.LookRotation(Vector3.right)    * Quaternion.AngleAxis(fTwistAngle, Vector3.right);
        if(LinkAxis == EAxis.Y)      return Quaternion.LookRotation(Vector3.up)       * Quaternion.AngleAxis(fTwistAngle, Vector3.up);
        if(LinkAxis == EAxis.Z)      return Quaternion.LookRotation(Vector3.forward)  * Quaternion.AngleAxis(fTwistAngle, Vector3.forward);
        if(LinkAxis == EAxis.MinusX) return Quaternion.LookRotation(-Vector3.right)   * Quaternion.AngleAxis(fTwistAngle, -Vector3.right);
        if(LinkAxis == EAxis.MinusY) return Quaternion.LookRotation(-Vector3.up)      * Quaternion.AngleAxis(fTwistAngle, -Vector3.up);
        if(LinkAxis == EAxis.MinusZ) return Quaternion.LookRotation(-Vector3.forward) * Quaternion.AngleAxis(fTwistAngle, -Vector3.forward);

        return Quaternion.identity;
    }

    float GetLinkedObjectScale(float fSegmentLength, int nNumLinks)
    {
        if(LinkObject == null)
        {
            return 0.0f;
        }

        MeshFilter meshFilter = LinkObject.GetComponent<MeshFilter>();

        if(meshFilter == null)
        {
            return 0.0f;
        }

        float fLinkedObjectLength = 0.0f;

        if(RopeType == ERopeType.LinkedObjects)
        {
            if(LinkAxis == EAxis.X || LinkAxis == EAxis.MinusX) fLinkedObjectLength = meshFilter.sharedMesh.bounds.size.x;
            if(LinkAxis == EAxis.Y || LinkAxis == EAxis.MinusY) fLinkedObjectLength = meshFilter.sharedMesh.bounds.size.y;
            if(LinkAxis == EAxis.Z || LinkAxis == EAxis.MinusZ) fLinkedObjectLength = meshFilter.sharedMesh.bounds.size.z;
        }

        float fDesiredLength = (fSegmentLength / nNumLinks) - (LinkOffsetObject * (fSegmentLength / (nNumLinks - 1)));

        return fDesiredLength / fLinkedObjectLength;
    }

    float GetLinkDiameter()
    {
        if(RopeType == ERopeType.Procedural)
        {
            return RopeDiameter;
        }
        else if(RopeType == ERopeType.LinkedObjects)
        {
            if(LinkObject == null)
            {
                return 0.0f;
            }

            MeshFilter meshFilter = LinkObject.GetComponent<MeshFilter>();

            if(meshFilter == null)
            {
                return 0.0f;
            }

            float fLinkedObjectDiameter = 0.0f;

            if(RopeType == ERopeType.LinkedObjects)
            {
                if(LinkAxis == EAxis.X || LinkAxis == EAxis.MinusX) fLinkedObjectDiameter = Mathf.Max(meshFilter.sharedMesh.bounds.size.y, meshFilter.sharedMesh.bounds.size.z);
                if(LinkAxis == EAxis.Y || LinkAxis == EAxis.MinusY) fLinkedObjectDiameter = Mathf.Max(meshFilter.sharedMesh.bounds.size.x, meshFilter.sharedMesh.bounds.size.z);
                if(LinkAxis == EAxis.Z || LinkAxis == EAxis.MinusZ) fLinkedObjectDiameter = Mathf.Max(meshFilter.sharedMesh.bounds.size.x, meshFilter.sharedMesh.bounds.size.y);
            }

            return fLinkedObjectDiameter;
        }
        else if(RopeType == ERopeType.ImportBones)
        {
            return BoneColliderDiameter;
        }

        return 0.0f;
    }

    Vector3 GetLinkAxisOffset(float fValue)
    {
        EAxis eAxis = EAxis.Z;

        if(RopeType == ERopeType.LinkedObjects) eAxis = LinkAxis;
        if(RopeType == ERopeType.ImportBones)   eAxis = BoneAxis;

        if(eAxis == EAxis.X)      return new Vector3(fValue, 0.0f, 0.0f);
        if(eAxis == EAxis.Y)      return new Vector3(0.0f, fValue, 0.0f);
        if(eAxis == EAxis.Z)      return new Vector3(0.0f, 0.0f, fValue);
        if(eAxis == EAxis.MinusX) return new Vector3(-fValue, 0.0f, 0.0f);
        if(eAxis == EAxis.MinusY) return new Vector3(0.0f, -fValue, 0.0f);
        if(eAxis == EAxis.MinusZ) return new Vector3(0.0f, 0.0f, -fValue);

        return new Vector3(0.0f, 0.0f, fValue);
    }

    int GetLinkAxisIndex()
    {
        EAxis eAxis = EAxis.Z;

        if(RopeType == ERopeType.LinkedObjects) eAxis = LinkAxis;
        if(RopeType == ERopeType.ImportBones)   eAxis = BoneAxis;

        if(eAxis == EAxis.X)      return 0;
        if(eAxis == EAxis.Y)      return 1;
        if(eAxis == EAxis.Z)      return 2;
        if(eAxis == EAxis.MinusX) return 0;
        if(eAxis == EAxis.MinusY) return 1;
        if(eAxis == EAxis.MinusZ) return 2;

        return 2;
    }

    bool GetLinkBoxColliderCenterAndSize(float fLinkLength, float fRopeDiameter, ref Vector3 v3CenterInOut, ref Vector3 v3SizeInOut)
    {
        if(RopeType == ERopeType.Procedural)
        {
            v3CenterInOut = Vector3.zero;
            v3SizeInOut   = new Vector3(fRopeDiameter, fRopeDiameter, fLinkLength);
            return true;
        }
        else if(RopeType == ERopeType.LinkedObjects)
        {
            MeshFilter meshFilter = LinkObject.GetComponent<MeshFilter>();

            if(meshFilter == null)
            {
                return false;
            }

            v3CenterInOut = meshFilter.sharedMesh.bounds.center;
            v3SizeInOut   = meshFilter.sharedMesh.bounds.size;
            return true;
        }
        else if(RopeType == ERopeType.ImportBones)
        {
            // v3CenterInOut is already assigned

            if(BoneAxis == EAxis.X)      v3SizeInOut = new Vector3(fLinkLength, fRopeDiameter, fRopeDiameter);
            if(BoneAxis == EAxis.Y)      v3SizeInOut = new Vector3(fRopeDiameter, fLinkLength, fRopeDiameter);
            if(BoneAxis == EAxis.Z)      v3SizeInOut = new Vector3(fRopeDiameter, fRopeDiameter, fLinkLength);
            if(BoneAxis == EAxis.MinusX) v3SizeInOut = new Vector3(fLinkLength, fRopeDiameter, fRopeDiameter);
            if(BoneAxis == EAxis.MinusY) v3SizeInOut = new Vector3(fRopeDiameter, fLinkLength, fRopeDiameter);
            if(BoneAxis == EAxis.MinusZ) v3SizeInOut = new Vector3(fRopeDiameter, fRopeDiameter, fLinkLength);

            return true;
        }

        v3CenterInOut = Vector3.zero;
        v3SizeInOut   = new Vector3(fRopeDiameter, fRopeDiameter, fLinkLength);
        return true;
    }

    bool BuildImportedBoneList(GameObject goBoneFirst, GameObject goBoneLast, List<int> ListImportBonesStatic, List<int> ListImportBonesNoCollider, out List<RopeBone> outListImportedBones, out string strErrorMessage)
    {
        strErrorMessage      = "";
        outListImportedBones = new List<RopeBone>();

        // Parse first and last bone names

        int nDigitsFirst = 0;
        int nDigitsLast  = 0;
        int nIndexFirst  = 0;
        int nIndexLast   = 0;

        for(int i = goBoneFirst.name.Length - 1; i >= 0; i--)
        {
            if(Char.IsDigit(goBoneFirst.name[i]))
            {
                nDigitsFirst++;
            }
            else break;
        }

        if(nDigitsFirst == 0)
        {
            strErrorMessage = "First bone name needs to end with digits in order to infer bone sequence";
            return false;
        }

        nIndexFirst = int.Parse(goBoneFirst.name.Substring(goBoneFirst.name.Length - nDigitsFirst));

        for(int i = goBoneLast.name.Length - 1; i >= 0; i--)
        {
            if(Char.IsDigit(goBoneLast.name[i]))
            {
                nDigitsLast++;
            }
            else break;
        }

        if(nDigitsLast == 0)
        {
            strErrorMessage = "Last bone name needs to end with digits in order to infer bone sequence";
            return false;
        }

        nIndexLast = int.Parse(goBoneLast.name.Substring(goBoneLast.name.Length - nDigitsLast));

        string strPrefixFirst = goBoneFirst.name.Substring(0, goBoneFirst.name.Length - nDigitsFirst);
        string strPrefixLast  = goBoneLast.name.Substring (0, goBoneLast.name.Length  - nDigitsLast);

        if(strPrefixFirst != strPrefixLast)
        {
            strErrorMessage = string.Format("First bone name prefix ({0}) and last bone name prefix ({1}) don't match", strPrefixFirst, strPrefixLast);
            return false;
        }

        // Get a common parent

        if(BoneFirst.transform.parent == null || BoneLast.transform.parent == null)
        {
            strErrorMessage = string.Format("First and last bones need to share a common parent object");
            return false;
        }

        GameObject goRoot = BoneLast.transform.IsChildOf(BoneFirst.transform) ? BoneFirst.transform.parent.gameObject : BoneLast.transform.parent.gameObject;

        // Try to import the bones from the parent

        if(BuildImportedBoneListTry(goRoot, strPrefixFirst, nIndexFirst, nIndexLast, nDigitsFirst, nDigitsLast, ListImportBonesStatic, ListImportBonesNoCollider, out outListImportedBones, ref strErrorMessage))
        {
            return true;
        }

        // Error? Try to import the bones from the root parent, maybe some bones are scattered across the hierarchy but also maybe trying to search from the root can introduce bone name collisions

        goRoot = goRoot.transform.root.gameObject;
        string strErrorComplete = string.Format("Try1: {0}\nTry2: ", strErrorMessage);

        if(BuildImportedBoneListTry(goRoot, strPrefixFirst, nIndexFirst, nIndexLast, nDigitsFirst, nDigitsLast, ListImportBonesStatic, ListImportBonesNoCollider, out outListImportedBones, ref strErrorMessage))
        {
            return true;
        }

        strErrorMessage = strErrorComplete + strErrorMessage;

        return false;
    }

    bool BuildImportedBoneListTry(GameObject goRoot, string strPrefix, int nIndexFirst, int nIndexLast, int nDigitsFirst, int nDigitsLast, List<int> ListImportBonesStatic, List<int> ListImportBonesNoCollider, out List<RopeBone> outListImportedBones, ref string strErrorMessage)
    {
        outListImportedBones = new List<RopeBone>();

        // Build goRoot's child node hash table (string->GameObject)

        Dictionary<string, GameObject> hashString2Bones = new Dictionary<string,GameObject>();

        if(BuildBoneHashString2GameObject(goRoot, goRoot, ref hashString2Bones, ref strErrorMessage) == false)
        {
            return false;
        }

        // Search for bones in the hash table

        Dictionary<GameObject, Transform> hashBoneGameObjectsTransform = new Dictionary<GameObject,Transform>();

        int nSign = nIndexFirst <= nIndexLast ? 1 : -1;

        for(int nIndex = nIndexFirst; nSign == 1 ? nIndex <= nIndexLast : nIndex >= nIndexLast; nIndex += nSign)
        {
            bool bFound = false;

            for(int nDigits = nDigitsFirst; nDigits <= nDigitsLast; nDigits++)
            {
                string strBoneName = strPrefix + nIndex.ToString("D" + nDigits);

                if(hashString2Bones.ContainsKey(strBoneName) == true)
                {
                    RopeBone newBone = new RopeBone();

                    newBone.goBone             = hashString2Bones[strBoneName];
                    newBone.tfParent           = newBone.goBone.transform.parent;
                    newBone.bCreatedCollider   = ListImportBonesNoCollider.Contains(nIndex) == false;
                    newBone.bIsStatic          = ListImportBonesStatic.Contains(nIndex);
                    newBone.nOriginalLayer     = newBone.goBone.layer;

                    outListImportedBones.Add(newBone);
                    hashBoneGameObjectsTransform.Add(newBone.goBone, newBone.goBone.transform);
                    bFound = true;

                    break;
                }
            }

            if(bFound == false)
            {
                strErrorMessage = string.Format("Bone not found (bone number suffix {0}, trying to find below node {1}'s hierarchy)", nIndex, goRoot.name);
                return false;
            }
        }

        // Compute properties

        foreach(RopeBone bone in outListImportedBones)
        {
            // Find first non-bone parent:

            Transform tfFirstNonBoneParent = bone.goBone.transform.parent;

            while(tfFirstNonBoneParent != null)
            {
                if(hashBoneGameObjectsTransform.ContainsKey(tfFirstNonBoneParent.gameObject))
                {
                    tfFirstNonBoneParent = tfFirstNonBoneParent.parent;
                }
                else
                {
                    break;
                }
            }

            if(tfFirstNonBoneParent == null)
            {
                tfFirstNonBoneParent = goRoot.transform;
            }

            hashBoneGameObjectsTransform[bone.goBone] = tfFirstNonBoneParent;
        }

        // Re-parent

        foreach(RopeBone bone in outListImportedBones)
        {
            // Compute initial position in the local space of non-bone parent:

            Transform tfFirstNonBoneParent = hashBoneGameObjectsTransform[bone.goBone];

            GameObject dummy = new GameObject();

            bone.v3OriginalLocalScale = bone.goBone.transform.localScale;
            dummy.transform.position  = bone.goBone.transform.position;
            dummy.transform.rotation  = bone.goBone.transform.rotation;
            dummy.transform.parent    = tfFirstNonBoneParent.transform;
            bone.v3OriginalLocalPos   = dummy.transform.localPosition;
            bone.qOriginalLocalRot    = dummy.transform.localRotation;

            bone.tfNonBoneParent = tfFirstNonBoneParent;

            DestroyImmediate(dummy);

            // Re-parent

            if(bone.bIsStatic)
            {
                bone.goBone.transform.parent = tfFirstNonBoneParent;
            }
            else
            {
                bone.goBone.transform.parent = this.transform;
            }            
        }

        return true;
    }

    bool BuildBoneHashString2GameObject(GameObject goRoot, GameObject goCurrent, ref Dictionary<string, GameObject> outHashString2GameObjects, ref string strErrorMessage)
    {
        for(int i = 0; i < goCurrent.transform.childCount; i++)
        {
            GameObject goChild = goCurrent.transform.GetChild(i).gameObject;

            if(BuildBoneHashString2GameObject(goRoot, goChild, ref outHashString2GameObjects, ref strErrorMessage) == false)
            {
                return false;
            }
        }

        if(outHashString2GameObjects.ContainsKey(goCurrent.name))
        {
            strErrorMessage = string.Format("Bone name {0} is found more than once in GameObject {1}'s hierarchy. The name must be unique.", goCurrent.name, goRoot.name);
            return false;
        }

        outHashString2GameObjects.Add(goCurrent.name, goCurrent);

        return true;
    }

    bool ParseBoneIndices(string strBoneList, out List<int> outListBoneIndices, out string strErrorMessage)
    {
        outListBoneIndices = new List<int>();
        strErrorMessage    = "";

        if(strBoneList.Length == 0)
        {
            return true;
        }

        string[] strFields = strBoneList.Split(',');

        for(int i = 0; i < strFields.Length; i++)
        {
            string[] strNumbers = strFields[i].Split('-');

            if(strNumbers.Length == 1)
            {
                int nNumber = 0;

                try
                {
                    nNumber = int.Parse(strNumbers[0]);
                }
                catch
                {
                    strErrorMessage = "Field " + (i + 1) + " is invalid (error parsing number: " + strNumbers[0] + ")";
                    return false;
                }

                outListBoneIndices.Add(nNumber);
            }
            else if(strNumbers.Length == 2)
            {
                int nFirst = 0;
                int nLast  = 0;

                try
                {
                    nFirst = int.Parse(strNumbers[0]);
                }
                catch
                {
                    strErrorMessage = "Field " + (i + 1) + " is invalid (error parsing range start: " + strNumbers[0] + ")";
                    return false;
                }

                try
                {
                    nLast = int.Parse(strNumbers[1]);
                }
                catch
                {
                    strErrorMessage = "Field " + (i + 1) + " is invalid (error parsing range end: " + strNumbers[1] + ")";
                    return false;
                }

                if(nLast < nFirst)
                {
                    strErrorMessage = "Field " + (i + 1) + " has invalid range (" + nFirst + " is greater than " + nLast + ")";
                    return false;
                }

                for(int nIndex = nFirst; nIndex <= nLast; nIndex++)
                {
                    outListBoneIndices.Add(nIndex);
                }
            }
            else
            {
                strErrorMessage = "Field " + (i + 1) + " has invalid range (field content: " + strFields[i] + ")";
                return false;
            }
        }

        outListBoneIndices.Sort();

        List<int> listNoRepetitions = new List<int>();

        int nPreviousIndex = -1;

        foreach(int nIndex in outListBoneIndices)
        {
            if(nIndex != nPreviousIndex)
            {
                nPreviousIndex = nIndex;
                listNoRepetitions.Add(nIndex);
            }
        }

        outListBoneIndices = listNoRepetitions;

        return true;
    }

    void CheckLoadPersistentData()
    {
        if(Application.isEditor)
        {
            if(RopePersistManager.PersistentDataExists(this))
            {
                RopePersistManager.RetrievePersistentData(this);
                RopePersistManager.RemovePersistentData(this);
            }
        }
    }

    void CheckSavePersistentData()
    {
        if(Application.isEditor && PersistAfterPlayMode && m_bLastStatusIsError == false)
        {
            RopePersistManager.StorePersistentData(this);
        }
    }
}

