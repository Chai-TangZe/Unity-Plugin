using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAndObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log (IsClickDownOverUI ());
        }
    }

    /// <summary>
    /// Whether touch down or mouse button down over UI GameObject.
    /// </summary>
    public static bool IsClickDownOverUI()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown (0))
#else
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject ())
#else
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#endif
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Whether touch up or mouse button up over UI GameObject.
    /// </summary>
    public static bool IsClickUpOverUI()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp (0))
#else
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject ())
#else
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#endif
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Raycast a ray from touch up or mouse button up and test whether hit transform,
    /// if hit transform return it or return null.
    /// [isCheckHitUI] Whether check ray hit ui transform,
    /// if hit ui just return null.
    /// </summary>
    public static Transform Raycast( bool isCheckHitUI = true )
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp (0))
        {
            if (isCheckHitUI)
            {
                if (EventSystem.current.IsPointerOverGameObject ())
                {
                    return null;
                }
            }

            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
#else
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (isCheckHitUI)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        return null;
                    }
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif

            RaycastHit hit;

            if (Physics.Raycast (ray, out hit))
            {
                // or hit.collider.transform;
                return hit.transform;
            }
        }

        return null;
    }
}
