using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayEffect : MonoBehaviour
{
    private ParticleSystem particleSystem;
    private void OnEnable()
    {
        if (!particleSystem)
            particleSystem = GetComponent<ParticleSystem> ();
        particleSystem.Play ();
    }
    private void OnDisable()
    {
        if (!particleSystem)
            particleSystem = GetComponent<ParticleSystem> ();
        particleSystem.Stop ();
    }
}
