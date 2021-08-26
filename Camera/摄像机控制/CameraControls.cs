using UnityEngine;

public class CameraControls : MonoBehaviour
{
    const string INPUT_MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    const string INPUT_MOUSE_X = "Mouse X";
    const string INPUT_MOUSE_Y = "Mouse Y";

    [Header ("目标")]
    public Transform Target;

    [Header ("切换第三人称视角")]
    public bool IsFollow = true;


    [Header ("旋转速度")]
    [Range (2f, 15f)]
    public float orbitSpeed = 6f;
    [Header ("缩放速度")]
    [Range (.3f, 2f)]
    public float zoomSpeed = .8f;
    [Header ("仰角限制")]
    [Range (0.01f, 89.9f)]
    public float EulerLimit = 85f;
    float limit = 0;


    [Header ("最小缩放距离")]
    public float MinDistance = 2f;
    [Header ("最大缩放距离")]
    public float MaxDistance = 10f;


    float distance = 0f;

    void Start()
    {
        if (Target == null)
        {
            GameObject Target = new GameObject ();
            Target.name = "Target";
            this.Target = Target.transform;
        }
        distance = Vector3.Distance (transform.position, Target.position);
    }

    void LateUpdate()
    {
        // orbits
        if (Input.GetMouseButton (0))
        {
            limit = 90 - EulerLimit;
            float rot_x = Input.GetAxis (INPUT_MOUSE_X);
            float rot_y = -Input.GetAxis (INPUT_MOUSE_Y);

            Vector3 eulerRotation = transform.localRotation.eulerAngles;
            eulerRotation.x += rot_y * orbitSpeed;
            eulerRotation.y += rot_x * orbitSpeed;
            eulerRotation.z = 0f;
            #region Limit
            if (eulerRotation.x > 90 - limit && eulerRotation.x < 200)
            {
                eulerRotation.x = 90 - limit;
            }
            //270--360
            if (eulerRotation.x > 200 && eulerRotation.x < 270 + limit)
            {
                eulerRotation.x = 270 + limit;
            }
            #endregion
            transform.localRotation = Quaternion.Euler (eulerRotation);
            if (IsFollow)
                transform.position = transform.localRotation * ( Vector3.forward * -distance ) + Target.position;
            else
                transform.position = Target.position;
        }
        if (Input.GetAxis (INPUT_MOUSE_SCROLLWHEEL) != 0f)
        {
            float delta = Input.GetAxis (INPUT_MOUSE_SCROLLWHEEL);

            distance -= delta * ( distance / MaxDistance ) * ( zoomSpeed * 1000 ) * Time.deltaTime;
            distance = Mathf.Clamp (distance, MinDistance, MaxDistance);

        }
        if (IsFollow)
        {
            Vector3 pos = transform.localRotation * ( Vector3.forward * -distance ) + Target.position;
            transform.position = Vector3.Lerp (transform.position, pos, 0.05f);//缩放插值，看起来更平滑;
        }
        else
            transform.position = Target.position;
    }
}

