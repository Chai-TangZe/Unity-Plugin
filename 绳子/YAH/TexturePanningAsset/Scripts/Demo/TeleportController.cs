/* 
* Created by You Are Here LLC, 2020
* https://www.yahagency.com/
*
* For questions or assistance please contact
* pluginsupport@yahagency.com
*/

using System.Collections;
using UnityEngine;

using com.yah.Core;

namespace com.yah.LineRendererDemo
{
    /// <summary>
    /// Handles the looking of the Line Renderer to connect the line, and handle input.
    /// </summary>
    public class TeleportController : YAH_MonoBehavior
    {
        [Header("Player Movement Params")]
        [SerializeField] private Transform playerTransform = null; // Transform to move on teleport
        [SerializeField] private CanvasGroup playerViewVignette = null;
        public float playerMoveTime = 0.25f;

        [Header("Input Params")]
        [SerializeField] private KeyCode teleportButton = KeyCode.E;

        [Header("Raycast Params")]
        [SerializeField] private LayerMask tpNodeLayer = -1; //Cast only to this layer
        [SerializeField] private Transform castPoint = null; //Cast from this point
        [SerializeField] private float castDistance = 10.0f; //Cast this far

        [Header("Line Animation params")]
        [SerializeField] private LineRenderer castLine = null; // Actual line renderer
        [Space(10)]
        [SerializeField] private Gradient inActiveColorWay = null;
        [SerializeField] private Gradient activeColorWay = null;
        [SerializeField] private float inActiveLength = 0.25f;
        [SerializeField] private float inActiveWidth = 0.01f;
        [SerializeField] private float activeWidth = 0.1f;
        [Space(10)]
        //These are for the Bezier curve smooth follow
        [SerializeField] private float curveActiveFollowSpeed = 5;
        [SerializeField] private float curveInActiveFollowSpeed = 100;
        [SerializeField] private float curveHitPointOffset = 0.25f;
        [SerializeField] private Transform[] curvePoints = null;
        [SerializeField] private int numberOfPointsOnCurve = 25;
        private Bezier curveGenerator = null;
        private Vector3 curvePointPosition = Vector3.zero;
        private bool curveLocked = false;

        private bool check = false;
        private bool canTP = false;

        private TPNode curNode = null;

        /// <summary>
        /// Initialized Bezier Curve, and castline params
        /// </summary>
        private void Start()
        {
            curveGenerator = new Bezier(numberOfPointsOnCurve);
            castLine.positionCount = numberOfPointsOnCurve;
            curveLocked = false;

            castLine.colorGradient = inActiveColorWay;

            canTP = true;

            ActivateCheck(true);
        }

        void Update()
        {
            if (check)
            {
                CheckForTPNode();

                Vector3[] newPositions = curveGenerator.GetQuadraticCurvePoints(curvePoints[0].position, curvePoints[1].position, curvePoints[2].position);
                castLine.SetPositions(newPositions);

                if (!curveLocked)
                {
                    curvePoints[2].position = Vector3.Lerp(curvePoints[2].position, curvePoints[1].position, curveInActiveFollowSpeed * Time.deltaTime);
                }
            }

            if (curNode != null)
            {
                ActivateLine();

                if (Input.GetKeyDown(teleportButton))
                {
                    Teleport();
                }

            }
        }

        /// <summary>
        /// Raycast out looking for a TPNode GameObject
        /// </summary>
        private void CheckForTPNode()
        {
            RaycastHit hit;

            if (Physics.Raycast(castPoint.position, castPoint.forward, out hit, castDistance, tpNodeLayer))
            {
                TPNode curSeenNode = hit.collider.GetComponentInParent<TPNode>();

                if (curSeenNode != null)
                {
                    if (curNode == null && !curSeenNode.isHovered)
                    {
                        curNode = curSeenNode;
                        curNode.OnHover();
                    }

                    curvePointPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z) + (hit.normal * curveHitPointOffset);
                }
                else
                {
                    curveLocked = false;

                    if (curNode != null)
                    {
                        curNode.OnHoverLost();
                        curNode = null;
                    }

                    DeActivateLine();
                }
            }
            else
            {
                curveLocked = false;

                if (curNode != null)
                {
                    curNode.OnHoverLost();
                    curNode = null;
                }
                
                DeActivateLine();
            }
        }

        /// <summary>
        /// Helper method to enable the curve
        /// </summary>
        /// <param name="t"></param>
        public void ActivateCheck(bool t)
        {
            check = t;

            if (check)
            {
                curvePoints[2].position = curvePoints[1].position;

                Vector3[] newPositions = curveGenerator.GetQuadraticCurvePoints(curvePoints[0].position, curvePoints[1].position, curvePoints[2].position);
                castLine.SetPositions(newPositions);

                castLine.enabled = true;
            }
        }

        /// <summary>
        /// Throw line at the TPNode
        /// </summary>
        private void ActivateLine()
        {
            if (castLine.colorGradient != activeColorWay)
                castLine.colorGradient = activeColorWay;

            castLine.startWidth = activeWidth;
            castLine.endWidth = activeWidth;

            curvePoints[1].position = Vector3.Lerp(curvePoints[1].position, curvePointPosition, curveActiveFollowSpeed * Time.deltaTime);

            if (Vector3.Distance(curvePoints[1].position, curvePointPosition) < 0.15f)
                curveLocked = true;

            if (curveLocked)
                curvePoints[2].position = Vector3.Lerp(curvePoints[2].position, curNode.lockPoint.position, curveActiveFollowSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Pull line back
        /// </summary>
        private void DeActivateLine()
        {
            castLine.enabled = false;

            castLine.colorGradient = inActiveColorWay;

            castLine.startWidth = inActiveWidth;
            castLine.endWidth = inActiveWidth;

            curvePoints[1].localPosition = curvePoints[0].localPosition + new Vector3(0, 0, inActiveLength);

            Vector3[] newPositions = curveGenerator.GetQuadraticCurvePoints(curvePoints[0].position, curvePoints[1].position, curvePoints[2].position);
            castLine.SetPositions(newPositions);

            castLine.enabled = true;
        }

        /// <summary>
        /// Call this when User Input is fired and TPNode is visible
        /// </summary>
        public void Teleport()
        {
            if (canTP)
            {
                canTP = false;

                if (curNode != null)
                {
                    curNode.Teleport();
                }

                DeActivateLine();

                StartCoroutine(BlinkPlayer(curNode.playerMovePos));
                curNode = null;

            }
        }

        /// <summary>
        /// Move player to the given target
        /// </summary>
        /// <param name="target"> new player position </param>
        /// <returns></returns>
        public IEnumerator BlinkPlayer(Transform target)
        {
            //Black player vision
            playerViewVignette.alpha = 1;

            //Move Player Transform
            playerTransform.position = new Vector3(target.position.x, playerTransform.position.y, target.position.z);
            playerTransform.rotation = target.rotation;

            yield return new WaitForSeconds(playerMoveTime);

            //Reveal view
            playerViewVignette.alpha = 0;

            canTP = true;
        }
    }
}