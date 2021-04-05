/* 
* Created by You Are Here LLC, 2020
* https://www.yahagency.com/
*
* For questions or assistance please contact
* pluginsupport@yahagency.com
*/

using UnityEngine;

using com.yah.Core;

namespace com.yah.LineRendererDemo
{
    /// <summary>
    /// Looks at target object removing the lookAt y position
    /// </summary>
    public class LookAtWithoutTilt : YAH_MonoBehavior
    {
        [SerializeField] private Transform lookTarget = null;

        private void Update()
        {
            Vector3 lookPos = new Vector3(lookTarget.position.x, transform.position.y, lookTarget.position.z);

            transform.LookAt(lookPos);
        }
    }
}