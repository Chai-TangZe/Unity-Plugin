using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using TFramework;

namespace UITool
{
    public enum MousePos
    {
        左上,
        右上,
        右下,
        左下
    }
    public class UI_Util : MonoBehaviour
    {
        public static Dictionary<string, T> PutObjIntoDic<T>( Transform t )
        {
            Dictionary<string, T> tempDic = new Dictionary<string, T> ();
            foreach (Transform item in t)
            {
                if (item != null && !tempDic.ContainsKey (item.name))
                {
                    T tempT = item.GetComponent<T> ();
                    if (tempT != null)
                    {
                        tempDic.Add (item.name, tempT);
                    }
                }
            }
            return tempDic;
        }
        public static void UIActiveControl( GameObject go, GameObject[] Group = null, bool one_way = false, string to = "active" )
        {
            bool tempBool = go.activeSelf;
            if (Group != null)
            {
                InactiveGroup (Group);
            }
            if (one_way)
            {
                go.SetActive (!( to == "close" ));
            }
            else
            {
                go.SetActive (!tempBool);
            }
        }
        public static void InactiveGroup( GameObject[] Group )
        {
            for (int i = 0; i < Group.Length; i++)
            {
                Group[i].SetActive (false);
            }
        }
        /// <summary>
        /// 隐藏所有子物体
        /// </summary>
        /// <param name="go"></param>
        public static void InactiveAllChild( GameObject go )
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                go.transform.GetChild (i).gameObject.SetActive (false);
            }
        }



        public static HierarchyPos GetHierarchyPos( Transform t )
        {
            HierarchyPos hierarchyPos = new HierarchyPos (t.GetSiblingIndex (), t.parent);
            return hierarchyPos;
        }
        public static void SetHierarchyPos( Transform t, HierarchyPos hierarchyPos )
        {
            t.SetParent (hierarchyPos.parent);
            t.SetSiblingIndex (hierarchyPos.sublingNumber);
        }
    }
    public class HierarchyPos
    {
        public int sublingNumber;
        public Transform parent;
        public HierarchyPos( int s, Transform p )
        {
            sublingNumber = s;
            parent = p;
        }
    }
    public class GetClickedUI
    {
        public static GameObject GetUI( GameObject canvas )
        {
            PointerEventData pointerEventData = new PointerEventData (EventSystem.current);
            pointerEventData.position = Input.mousePosition;
            GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster> ();
            List<RaycastResult> results = new List<RaycastResult> ();
            gr.Raycast (pointerEventData, results);
            if (results.Count != 0)
            {
                return results[0].gameObject;
            }
            return null;
        }
    }
    //UI拖拽功能类

    public class UI_Dragble : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header ("需要变换大小的UI")]
        public RectTransform panelRectTransform;
        [Header ("可根据变换改变缩放的物体(可以为null)")]
        public RectTransform zoomable;
        [Header("是否进行同比例缩放")]
        public bool IsPariPassu = false;
        [Header ("可根据变换改变自适应的物体(可以为null)")]
        public RectTransform adaptive;
        [Header ("锚点名字")]
        public string adjustname = "调整";
        [Header ("拖拽点名字")]
        public string dragname = "拖拽";

        [Header ("调整的最小值")]
        public Vector2 minSize = new Vector2 (50, 50);
        [Header ("调整的最大值")]
        public Vector2 maxSize = new Vector2 (500, 500);

        private Transform Ancestry;
        private GameObject Canvas;
        //[Header("是否精准拖拽")]
        bool m_isPrecision;

        //存储图片中心点与鼠标点击点的偏移量
        Vector3 m_offset;

        Transform originParent;
        HierarchyPos hierarchyPos;

        //存储当前拖拽图片的RectTransform组件
        private RectTransform m_rt;
        //记录原始UI的大小
        private Vector2 panelWidthAndHeight;
        void Start()
        {
            if (!Ancestry)
                Ancestry = transform.parent;
            hierarchyPos = UI_Util.GetHierarchyPos (transform);
            m_isPrecision = true;
            //初始化
            m_rt = gameObject.GetComponent<RectTransform> ();
            panelWidthAndHeight = panelRectTransform.sizeDelta;
            uIOriginalSizeDelta = panelRectTransform.sizeDelta;
            GetCanvas ();
            adaptivePanel ();
        }
        void adaptivePanel()
        {
            if (adaptive)
            {
                adaptive.SetAnchor (AnchorPresets.TopLeft);
                if (adaptive.sizeDelta.y < panelRectTransform.sizeDelta.y)
                {
                    Vector2 panelRectTransformSize = panelRectTransform.sizeDelta;
                    panelRectTransformSize.y = adaptive.sizeDelta.y;
                    panelRectTransform.sizeDelta = panelRectTransformSize;
                }
            }
            DragUIAdaptiveSize ();
        }
        //开始拖拽触发
        public void OnBeginDrag( PointerEventData eventData )
        {
            //如果精确拖拽则进行计算偏移量操作
            if (m_isPrecision)
            {
                // 存储点击时的鼠标坐标
                Vector3 tWorldPos;
                //UI屏幕坐标转换为世界坐标
                RectTransformUtility.ScreenPointToWorldPointInRectangle (m_rt, eventData.position, eventData.pressEventCamera, out tWorldPos);
                //计算偏移量   
                m_offset = transform.position - tWorldPos;
            }
            //否则，默认偏移量为0
            else
            {
                m_offset = Vector3.zero;
            }

            SetDraggedPosition (eventData);
        }

        bool isDrag = false;
        bool isZoom = false;
        /// <summary>
        /// 每次点击
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown( PointerEventData eventData )
        {
            isDrag = false;
            isZoom = false;
            if (IsUI (adjustname))
            {
                isZoom = true;
            }
            if (IsUI (dragname))
            {
                isDrag = true;
            }

            setToTop ();
            DragUIToResizeClickedDown (eventData);
        }

        //拖拽过程中触发
        public void OnDrag( PointerEventData eventData )
        {
            SetDraggedPosition (eventData);
            DragUIToResizeClicked (eventData);
            DragUIAdaptiveSize ();
        }
        void DragUIAdaptiveSize()
        {
            if (adaptive != null)
            {
                Vector2 adaptiveSize = adaptive.sizeDelta;
                adaptiveSize.x = panelRectTransform.sizeDelta.x - 17;
                adaptive.sizeDelta = adaptiveSize;
                adaptive.AdaptiveUI ();
            }
        }
        //结束拖拽触发
        public void OnEndDrag( PointerEventData eventData )
        {
            SetDraggedPosition (eventData);
        }
        public void OnPointerUp( PointerEventData eventData )
        {
            if (isZoom)
            {
                panelRectTransform.SetPivot (PivotPresets.MiddleCenter);
                computeTransformLocation (mousePos, false);
            }
        }

        // 鼠标按下的时候UI原来的大小
        private Vector2 uIOriginalSizeDelta;
        // 鼠标按下的时候鼠标位置转到UI的位置的大小
        private Vector2 originalLocalPointerPosition;
        // 判断鼠标的点击位置，是左上还是右上(让变化合理（鼠标向内drag是变小，向外drag是变大）)
        MousePos mousePos;
        PivotPresets currentPivotLocation;
        Vector2 thisPosition;

        /// <summary>
        /// 开始点击记录缩放代码
        /// </summary>
        /// <param name="data"></param>
        void DragUIToResizeClickedDown( PointerEventData data )
        {
            if (!isZoom)
                return;
            // 记录按下鼠标变化前的大小
            uIOriginalSizeDelta = panelRectTransform.sizeDelta;
            // 记录按下鼠标的位置
            RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
            thisPosition = panelRectTransform.sizeDelta;
            // 判断鼠标的点击位置(对角线法)
            if (originalLocalPointerPosition.x > 0 && originalLocalPointerPosition.y < 0)
            {
                mousePos = MousePos.右下;
                currentPivotLocation = PivotPresets.TopLeft;
            }
            if (originalLocalPointerPosition.x > 0 && originalLocalPointerPosition.y > 0)
            {
                mousePos = MousePos.右上;
                currentPivotLocation = PivotPresets.BottomLeft;
            }
            if (originalLocalPointerPosition.x < 0 && originalLocalPointerPosition.y < 0)
            {
                mousePos = MousePos.左下;
                currentPivotLocation = PivotPresets.TopRight;
            }
            if (originalLocalPointerPosition.x < 0 && originalLocalPointerPosition.y > 0)
            {
                mousePos = MousePos.左上;
                currentPivotLocation = PivotPresets.BottomRight;
            }
            computeTransformLocation (mousePos, true);
            panelRectTransform.SetPivot (currentPivotLocation);
            uIOriginalSizeDelta /= 2;
        }

        void computeTransformLocation( MousePos mMousePos, bool mIsClicked )
        {
            int IsClicked = -1;
            if (mIsClicked)
            {
                IsClicked = -1;
            }
            else
            {
                thisPosition = panelRectTransform.sizeDelta;
                IsClicked = 1;
            }
            switch (mMousePos)
            {
                case MousePos.左上:
                    thisPosition.x *= -1;
                    panelRectTransform.anchoredPosition += thisPosition / 2 * IsClicked;
                    break;
                case MousePos.右上:
                    panelRectTransform.anchoredPosition += thisPosition / 2 * IsClicked;
                    break;
                case MousePos.右下:
                    thisPosition.y *= -1;
                    panelRectTransform.anchoredPosition += thisPosition / 2 * IsClicked;
                    break;
                case MousePos.左下:
                    thisPosition *= -1;
                    panelRectTransform.anchoredPosition += thisPosition / 2 * IsClicked;
                    break;
            }
        }

        /// <summary>
        /// 点击中缩放代码
        /// </summary>
        /// <param name="data"></param>
        void DragUIToResizeClicked( PointerEventData data )
        {
            // UI 的安全校验
            if (panelRectTransform == null)
                return;
            if (!isZoom)
                return;

            // Drag 变化的变量
            Vector2 localPointerPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, data.position, data.pressEventCamera, out localPointerPosition);
            
            Vector2 ultimatelyDelta = GetWidthAndHeightInMouse (localPointerPosition);
            if (IsPariPassu)
                panelRectTransform.sizeDelta = GetRatioInMouse (ultimatelyDelta);
            else
                panelRectTransform.sizeDelta = ultimatelyDelta;
            //计算缩放物体大小
            if (zoomable != null)
            {
                if (ultimatelyDelta.y / panelWidthAndHeight.y > ultimatelyDelta.x / panelWidthAndHeight.x)
                    zoomable.GetComponent<RectTransform> ().localScale = new Vector3 (ultimatelyDelta.x / panelWidthAndHeight.x, ultimatelyDelta.x / panelWidthAndHeight.x, 1);
                else
                    zoomable.GetComponent<RectTransform> ().localScale = new Vector3 (ultimatelyDelta.y / panelWidthAndHeight.y, ultimatelyDelta.y / panelWidthAndHeight.y, 1);
            }
        }

        /// <summary>
        /// 计算UI宽高随着鼠标位置
        /// </summary>
        /// <param name="localPointerPosition"></param>
        /// <returns></returns>
        Vector2 GetWidthAndHeightInMouse( Vector2 localPointerPosition )
        {
            // Drag 变化值于鼠标按下拖拽前的值之间的差值
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
            //offsetToOriginal = offsetToOriginal * 2f;

            //offsetToOriginal = new Vector3 (offsetToOriginal.x, Mathf.Abs (offsetToOriginal.x) * Mathf.Sign (offsetToOriginal.y), 0);
            // UI RectTransform 差值变化 （注意左上和右下的区别对待）
            Vector2 sizeDelta;
            switch (mousePos)
            {
                case MousePos.左上:
                    sizeDelta = uIOriginalSizeDelta + new Vector2 (-offsetToOriginal.x, offsetToOriginal.y);//左上
                    break;
                case MousePos.右上:
                    sizeDelta = uIOriginalSizeDelta + new Vector2 (offsetToOriginal.x, offsetToOriginal.y);//右上
                    break;
                case MousePos.右下:
                    sizeDelta = uIOriginalSizeDelta + new Vector2 (offsetToOriginal.x, -offsetToOriginal.y);//右下
                    break;
                case MousePos.左下:
                    sizeDelta = uIOriginalSizeDelta + new Vector2 (-offsetToOriginal.x, -offsetToOriginal.y);//左下
                    break;
                default:
                    sizeDelta = new Vector2 (0, 0);
                    break;
            }

            sizeDelta = new Vector2 (
                        Mathf.Clamp (sizeDelta.x, minSize.x, maxSize.x),
                        Mathf.Clamp (sizeDelta.y, minSize.y, maxSize.y)
                );

            return sizeDelta;
        }

        /// <summary>
        /// 同比例缩放
        /// </summary>
        /// <param name="uIOriginalSizeDelta"></param>
        /// <returns></returns>
        Vector2 GetRatioInMouse( Vector2 ultimatelyDelta )
        {
            Vector2 sizeDelta = ultimatelyDelta / ( uIOriginalSizeDelta * 2 );
            //Debug.Log (panelRectTransform.sizeDelta + "__" + uIOriginalSizeDelta);

            if (ultimatelyDelta.y / ( uIOriginalSizeDelta * 2 ).y > ultimatelyDelta.x / ( uIOriginalSizeDelta * 2 ).x)
            {
                return ( uIOriginalSizeDelta * 2 ) * sizeDelta.x;
            }
            else
            {
                return ( uIOriginalSizeDelta * 2 ) * sizeDelta.y;
            }
        }

        /// <summary>
        /// 设置图片位置方法
        /// </summary>
        /// <param name="eventData"></param>
        private void SetDraggedPosition( PointerEventData eventData )
        {
            if (!isDrag)
                return;
            //存储当前鼠标所在位置
            Vector3 globalMousePos;
            //UI屏幕坐标转换为世界坐标
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle (m_rt, eventData.position, eventData.pressEventCamera, out globalMousePos))
            {
                //设置位置及偏移量
                m_rt.position = globalMousePos + m_offset;
            }
        }

        void setToTop()
        {
            if (!isDrag)
                return;
            if (Ancestry != null)
            {
                UI_Util.SetHierarchyPos (transform, new HierarchyPos (Ancestry.childCount, Ancestry));
            }
        }
        public void Recover()
        {
            UI_Util.SetHierarchyPos (transform, hierarchyPos);
        }
        private void OnDisable()
        {
            Invoke ("Recover", 0.2f);
        }
        bool IsUI( string UIName )
        {
            GameObject obj = GetClickedUI.GetUI (Canvas);
            if (obj)
            {
                if (obj.name == UIName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        void GetCanvas()
        {
            Canvas mCanvas;
            Transform par = gameObject.transform.parent;
            int i = 0;
            while (Canvas == null)
            {
                mCanvas = par.GetComponent<Canvas> ();
                if (mCanvas != null)
                {
                    Canvas = mCanvas.gameObject;
                    break;
                }
                par = par.transform.parent;
                i++;
                if (i > 100)
                {
                    break;
                }
            }
        }
    }
}