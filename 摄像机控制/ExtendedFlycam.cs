using UnityEngine;
using System.Collections;

public class ExtendedFlycam : MonoBehaviour
{
    [Header ("按键控制")]
    public KeyCode Key=KeyCode.Mouse1;
    [Header ("旋转速度")]
    public float cameraSensitivity = 180;
    [Header ("攀升速度")]
    public float climbSpeed = 5;
    [Header ("正常移动速度")]
    public float normalMoveSpeed = 10;
    [Header ("下降速度")]
    public float slowMoveFactor = 2f;
    [Header ("加速度")]
    public float fastMoveFactor = 3;
    [Header ("旋转平滑")]
    public float rotationlerp = 7;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Key== KeyCode.None)
        {
            cameraController ();
        }
        else if(Input.GetKey (Key))
        {
            cameraController ();
        }
    }
    void cameraController()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            rotationX += Input.GetAxis ("Mouse X") * cameraSensitivity * Time.deltaTime;
            rotationY += Input.GetAxis ("Mouse Y") * cameraSensitivity * Time.deltaTime;
        }
        rotationY = Mathf.Clamp (rotationY, -90, 90);

        Quaternion temp = Quaternion.AngleAxis (rotationX, Vector3.up);
        temp *= Quaternion.AngleAxis (rotationY, Vector3.left);

        transform.localRotation = Quaternion.Lerp (transform.localRotation, temp, Time.deltaTime * rotationlerp);

        //transform.localRotation = Quaternion.AngleAxis( rotationX, Vector3.up );
        //transform.localRotation *= Quaternion.AngleAxis( rotationY, Vector3.left );

        if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
        {
            transform.position += transform.forward * ( normalMoveSpeed * fastMoveFactor ) * Input.GetAxis ("Vertical") * Time.deltaTime;
            transform.position += transform.right * ( normalMoveSpeed * fastMoveFactor ) * Input.GetAxis ("Horizontal") * Time.deltaTime;

            if (Input.GetKey (KeyCode.Space)) { transform.position += Vector3.up * climbSpeed * fastMoveFactor * Time.deltaTime; }
            if (Input.GetKey (KeyCode.LeftControl)) { transform.position -= Vector3.up * climbSpeed * fastMoveFactor * Time.deltaTime; }
        }
        else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))
        {
            transform.position += transform.forward * ( normalMoveSpeed * slowMoveFactor ) * Input.GetAxis ("Vertical") * Time.deltaTime;
            transform.position += transform.right * ( normalMoveSpeed * slowMoveFactor ) * Input.GetAxis ("Horizontal") * Time.deltaTime;

            if (Input.GetKey (KeyCode.Space)) { transform.position += Vector3.up * climbSpeed * slowMoveFactor * Time.deltaTime; }
            if (Input.GetKey (KeyCode.LeftControl)) { transform.position -= Vector3.up * climbSpeed * slowMoveFactor * Time.deltaTime; }
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * Input.GetAxis ("Vertical") * Time.deltaTime;
            transform.position += transform.right * normalMoveSpeed * Input.GetAxis ("Horizontal") * Time.deltaTime;

            if (Input.GetKey (KeyCode.Space)) { transform.position += Vector3.up * climbSpeed * Time.deltaTime; }
            if (Input.GetKey (KeyCode.LeftControl)) { transform.position -= Vector3.up * climbSpeed * Time.deltaTime; }
        }

        if (Input.GetKeyDown (KeyCode.End) || Input.GetKeyDown (KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            //Cursor.lockState = ( Cursor.lockState == CursorLockMode.Locked ) ? CursorLockMode.None : CursorLockMode.Locked;
            //Screen.lockCursor = ( Screen.lockCursor == false ) ? true : false;
        }
    }
}
