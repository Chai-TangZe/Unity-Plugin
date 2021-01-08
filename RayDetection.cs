using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayDetection : MonoBehaviour
{
    [Range(0f, 15f)]
    public float MaxDistance=5;
    // Update is called once per frame
    //void Update()
    //{
    //    GetRayObj();
    //}
    public GameObject GetRayObj()
    {
        Ray camerRay;                       //声明一个射线
        Vector3 mousePos = new Vector3();   //记录将鼠标（因为屏幕坐标没有z，所以下面是将z设为0）
        RaycastHit cameraHit;               //用于记录射线碰撞到的物体
        //这里将屏幕坐标的鼠标位置存入一个vector3里面
        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;
        mousePos.z = 0;

        //Ray ray=Camera.main.ScreenPointToRay(Vector3 Pos):返回一条射线由摄像机近裁面发射经过Pos的射线。
        camerRay = Camera.main.ScreenPointToRay(mousePos);
        //物理检测射线，out一个RaycastHit类型的 hitInfo 信息，float distance是射线长度，int layerMask需要转换二进制，所以有如下操作
        if (Physics.Raycast(camerRay, out cameraHit, MaxDistance))
        {
            GameObject go = cameraHit.transform.gameObject; //这是检测到的物体
            if (go != null)
            {
                return go;
            }
        }
        Debug.Log("距离不够？没有点到物体？物体没有碰撞体？");
        return null;
    }
}
