using UnityEngine;

public class UIConnectLine : MonoBehaviour
{
    Canvas canvas;
    public RectTransform Line;

    public UIConnectLine( Canvas canvas, RectTransform line )
    {
        this.canvas = canvas;
        Line = line;
    }

    /// <summary>
    /// 赋予开始位置,下一个位置就是鼠标点
    /// </summary>
    /// <param name="startPos"></param>
    public void UpdateLine( Vector3 startPos, Vector3 touchPos )
    {
        //Vector3 touchPos = Input.mousePosition;
        Vector3 uiStartPos = Vector3.zero;
        Vector3 uitouchPos = Vector3.zero;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiStartPos = startPos;
            uitouchPos = touchPos;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Camera camera = canvas.worldCamera;

            //UI世界的起点世界坐标转换为UGUI坐标
            Vector2 screenStartPos = RectTransformUtility.WorldToScreenPoint (camera, startPos);
            RectTransformUtility.ScreenPointToWorldPointInRectangle (canvas.GetComponent<RectTransform> (), screenStartPos,
                camera, out uiStartPos);

            //鼠标坐标转换为UGUI坐标
            RectTransformUtility.ScreenPointToWorldPointInRectangle (canvas.GetComponent<RectTransform> (), touchPos,
                camera, out uitouchPos);
        }

        Line.pivot = new Vector2 (0, 0.5f);
        Line.position = startPos;
        Line.eulerAngles = new Vector3 (0, 0, GetAngle (uiStartPos, uitouchPos));
        Line.sizeDelta = new Vector2 (GetDistance (uiStartPos, uitouchPos), Line.sizeDelta.y);
    }

    /// <summary>
    /// 直接生成线段
    /// </summary>
    /// <param name="lineSource">线段</param>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <returns></returns>
    public RectTransform SetLine( Vector3 startPos, Vector3 endPos)
    {
        RectTransform lineSource = Line;
        RectTransform line = Instantiate (lineSource, lineSource.parent);
        line.pivot = new Vector2 (0, 0.5f);
        line.position = startPos;
        line.eulerAngles = new Vector3 (0, 0, GetAngle (startPos, endPos));
        line.sizeDelta = new Vector2 (GetDistance (startPos, endPos), lineSource.sizeDelta.y);
        return line;
    }

    private float GetAngle( Vector3 startPos, Vector3 endPos )
    {
        Vector3 dir = endPos - startPos;
        float angle = Vector3.Angle (Vector3.right, dir);
        Vector3 cross = Vector3.Cross (Vector3.right, dir);
        float dirF = cross.z > 0 ? 1 : -1;
        angle = angle * dirF;
        return angle;
    }

    private float GetDistance( Vector3 startPos, Vector3 endPos )
    {
        float distance = Vector3.Distance (endPos, startPos);
        return distance * 1 / canvas.transform.localScale.x;
    }
}
