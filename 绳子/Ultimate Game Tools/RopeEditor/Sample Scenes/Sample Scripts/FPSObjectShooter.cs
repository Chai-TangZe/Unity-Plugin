using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSObjectShooter : MonoBehaviour
{
    public GameObject        Element      = null;
    public float             InitialSpeed = 1.0f;
    public float             MouseSpeed   = 0.3f;
    public float             Scale        = 1.0f;
    public float             Mass         = 1.0f;
    public float             Life         = 10.0f;

    private Vector3          m_v3MousePosition;

	void Start()
    {
	    m_v3MousePosition = Input.mousePosition;
	}
	
	void Update()
    {
        if(Element != null)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                GameObject newObject = GameObject.Instantiate(Element) as GameObject;
                newObject.transform.position   = this.transform.position;
                newObject.transform.localScale = new Vector3(Scale, Scale, Scale);
                newObject.GetComponent<Rigidbody>().mass                 = Mass;
                newObject.GetComponent<Rigidbody>().solverIterations = 255;
                newObject.GetComponent<Rigidbody>().AddForce(this.transform.forward * InitialSpeed, ForceMode.VelocityChange);

                DieTimer dieTimer = newObject.AddComponent<DieTimer>() as DieTimer;
                dieTimer.SecondsToDie = Life;
            }
        }

        if(Input.GetMouseButton(0) && Input.GetMouseButtonDown(0) == false)
        {
            this.transform.Rotate      (-(Input.mousePosition.y - m_v3MousePosition.y) * MouseSpeed, 0.0f, 0.0f);
            this.transform.RotateAround(this.transform.position, Vector3.up, (Input.mousePosition.x - m_v3MousePosition.x) * MouseSpeed);
        }

        m_v3MousePosition = Input.mousePosition;
	}
}
