using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 3f;
    float MoveSpeed = 0;

    void LateUpdate()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x,Camera.main.transform.eulerAngles.y,transform.eulerAngles.z);
        if (Input.GetKey(KeyCode.W))
        {
            MoveSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                MoveSpeed = moveSpeed * 2f;
            transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            MoveSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                MoveSpeed = moveSpeed * 2f;
            transform.Translate(Vector3.back * MoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            MoveSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                MoveSpeed = moveSpeed * 2f;
            transform.Translate(Vector3.left * MoveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            MoveSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                MoveSpeed = moveSpeed * 2f;
            transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime);
        }
    }
}
