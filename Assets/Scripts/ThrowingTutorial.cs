using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ThrowingTutorial : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject smokeGrenade;
    public GameObject arrow;
    public Image[] fillImages;
    public AudioClip arrowSFX;

    [Header("Settings")]
    public int totalThrows;
    public float throwCooldown;
    private float lastThrowTime = 0f;

    [Header("Throwing")]
    public float throwForce;
    public float throwUpwardForce;

    bool isCooldown = false;
    bool isCooldownArrow = false;

    bool readyToThrow;

    private void Start()
    {
        readyToThrow = true;
    }
    private void Update()
    {
        if (isCooldown)
        {
            fillImages[0].fillAmount -= 1 / throwCooldown * Time.deltaTime;

            if (fillImages[0].fillAmount <= 0)
            {
                fillImages[0].fillAmount = 0;
                isCooldown = false;
            }
        }
        if (isCooldownArrow)
        {
            fillImages[1].fillAmount -= 1 / throwCooldown * Time.deltaTime;

            if (fillImages[1].fillAmount <= 0)
            {
                fillImages[1].fillAmount = 0;
                isCooldownArrow = false;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            ThrowSmoke();
        }
    }
    public void ThrowSmoke()
    {
        if (readyToThrow && totalThrows > 0)
        {
            if(!isCooldown)
            {
                isCooldown = true;
                fillImages[0].fillAmount = 1;
            }
            readyToThrow = false;

            // instantiate object to throw
            GameObject projectile = Instantiate(smokeGrenade, attackPoint.position, Quaternion.Euler(0f, gameObject.transform.rotation.eulerAngles.y, 0f));

            // get rigidbody component
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

            // calculate direction
            Vector3 forceDirection = cam.transform.forward;

            RaycastHit hit;

            if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
            {
                forceDirection = (hit.point - attackPoint.position).normalized;
            }

            // add force
            Vector3 forceToAdd = forceDirection * throwForce + transform.up / 2;

            projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

            totalThrows--;

            // implement throwCooldown
            Invoke(nameof(ResetThrow), throwCooldown);
        }
    }
    public void ThrowArrow()
    {
        if (readyToThrow && totalThrows > 0)
        {
            readyToThrow = false;
            if (!isCooldownArrow)
            {
                isCooldownArrow = true;
                fillImages[1].fillAmount = 1;
            }
            // instantiate object to throw
            GameObject projectile = Instantiate(arrow, attackPoint.position, Quaternion.Euler(0f, gameObject.transform.rotation.eulerAngles.y, 0f));
        projectile.transform.Rotate(0, 85, 0);

            gameObject.GetComponent<AudioSource>().PlayOneShot(arrowSFX);
        // get rigidbody component
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // calculate direction
        Vector3 forceDirection = cam.transform.forward;

        Destroy(projectile, 6f);
        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        // add force
        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;

        // implement throwCooldown
        Invoke(nameof(ResetThrow), throwCooldown);
        }
    }

    private void ResetThrow()
    {
        readyToThrow = true;
    }
}