/* 
* Created by You Are Here LLC, 2020
* https://www.yahagency.com/
*
* For questions or assistance please contact
* pluginsupport@yahagency.com
*/

using UnityEngine;
using UnityEngine.Events;

using com.yah.Core;

namespace com.yah.LineRendererDemo
{
    /// <summary>
    /// Small example of a TeleportNode sequence
    /// </summary>
    public class TPNode : YAH_MonoBehavior
    {
        public Transform lockPoint = null;

        //Event that fires when the Teleport method is called.
        [SerializeField] private UnityEvent OnTeleportEvent = null;

        [Header("TPNode References")]
        [SerializeField] private Renderer[] tpRenderers = null;

        [Header("Player Movement Params")]
        public Transform playerMovePos = null;

        [SerializeField] private ParticleSystem pulseParticleSystem = null;

        public bool isHovered = false;

        private ParticleSystem.EmissionModule pulseEmitter;

        private bool isTeleporting = false;

        private void Start()
        {
            isTeleporting = false;

            pulseEmitter = pulseParticleSystem.emission;
        }

        /// <summary>
        /// When user hovers over the Node
        /// </summary>
        public void OnHover()
        {
            if (!isHovered)
            {
                pulseEmitter.enabled = true;
                isHovered = true;

                foreach (Renderer r in tpRenderers)
                {
                    foreach (Material m in r.materials)
                        m.color = Color.green;
                }
            }
        }

        /// <summary>
        /// Reset method for when Hover is lost but not fired
        /// </summary>
        public void OnHoverLost()
        {
            if (isHovered)
            {
                pulseEmitter.enabled = false;
                pulseParticleSystem.Clear();
                isHovered = false;

                foreach (Renderer r in tpRenderers)
                {
                    foreach (Material m in r.materials)
                        m.color = Color.clear;
                }
            }
        }

        /// <summary>
        /// When player triggers the TPNode
        /// </summary>
        public void Teleport()
        {
            if (!isTeleporting)
            {
                isTeleporting = true;

                OnHoverLost();

                OnTeleportEvent.Invoke();

                isTeleporting = false;

                this.gameObject.SetActive(false);
            }
        }
    }
}