/* 
* Created by You Are Here LLC, 2020
* https://www.yahagency.com/
*
* For questions or assistance please contact
* pluginsupport@yahagency.com
*/

using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector Editor that shows an Logo at the top
/// </summary>
namespace com.yah.Core
{
    [CustomEditor(typeof(YAH_MonoBehavior), true)]
    [CanEditMultipleObjects]
    public class GenericYAHLogoInspector : Editor
    {
        private Texture2D logo;
        public string logoFilePath = "Assets/YAH/Core/Textures/yah_logo_lrg.png";

        public void OnEnable()
        {
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>(logoFilePath);
        }

        public override void OnInspectorGUI()
        {
            if(logo == null)
                logo = AssetDatabase.LoadAssetAtPath<Texture2D>(logoFilePath);

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            GUILayout.Label(logo, GUILayout.MaxWidth(300), GUILayout.MaxHeight(175));
            GUILayout.Label("");
            GUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }
}