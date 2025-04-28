using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    public ParticleSystem smoke;

    private Rigidbody rb;
    private AudioSource audioSource;
    public AudioClip smokeSFX;
    private bool targetHit;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // make sure only to stick to the first target you hit
        if (targetHit)
            return;
        else
            targetHit = true;

        // check if you hit collider
        smoke.Play();
        audioSource.PlayOneShot(smokeSFX);
        GetComponentInChildren<MeshRenderer>().enabled = false;
        // make sure projectile sticks to surface
        rb.isKinematic = true;

        // make sure projectile moves with target
        transform.SetParent(collision.transform);
    }
}