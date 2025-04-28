using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private FloatingJoystick _joystick;
    [SerializeField] private Transform headPoint;
    [SerializeField] private GameObject plantButton;
    [SerializeField] private TextMeshProUGUI redTeamWinsText;
    [SerializeField] private TextMeshProUGUI blueTeamWinsText;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Animator anim;
    [SerializeField] private Image fillImage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip plantingSFX;
    [SerializeField] private AudioClip explosionSFX;

    private GameObject c4;
    public float targetTime = 10.0f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int medAttempts = 2;
    public bool readyToHeal = true;
    public int healCooldown = 10;
    bool isCooldown = false;

    [SerializeField] private float sightRange = 10f;
    [SerializeField] private LayerMask whatIsEnemy;
    public GameObject closestEnemy;

    public float shootCooldown = 1f; // Time in seconds between shots
    private bool canShoot = true;
    private bool canPlant = false;
    private bool planted = false;
    private bool hasTheBomb = false;
    private bool shooting = false;
    private bool walking = false;
    private bool planting = false;

    public float health;
    public Slider healthSlider;

    public float projectileForce = 10f;

    private void Awake()
    {
        plantButton.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("C4"))
        {
            other.transform.position = headPoint.position;
            other.GetComponent<BoxCollider>().enabled = false;
            other.transform.SetParent(headPoint);
            plantButton.SetActive(true);
            other.transform.GetChild(2).gameObject.SetActive(false);

            hasTheBomb = true;
            c4 = other.gameObject;
        }
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(15);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlantZone"))
        {
            if(hasTheBomb)
            canPlant = true;
        }
    }
    public void medHeal()
    {
        if(readyToHeal && medAttempts > 0)
        {
            if (!isCooldown)
            {
                isCooldown = true;
                fillImage.fillAmount = 1;
            }
            readyToHeal = false;
            medAttempts--;
            TakeDamage(-30);
            // implement throwCooldown
            Invoke(nameof(ResetHeal), healCooldown);
        }
    }
    private void ResetHeal()
    {
        readyToHeal = true;
    }
    private GameObject FindClosestEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sightRange, whatIsEnemy);
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = collider.gameObject;
            }
        }

        return closestEnemy;
    }
    private void Update()
    {
        if (isCooldown)
        {
            fillImage.fillAmount -= 0.1f * Time.deltaTime;
            if (fillImage.fillAmount <= 0)
            {
                fillImage.fillAmount = 0;
                isCooldown = false;
            }
        }
        float threshold = 2.5f;
        if (_rigidBody.velocity.magnitude < threshold * Time.deltaTime)
        {
            walking = false;
        }
        else
        {
            walking = true;
        }

        // Update animation states based on player actions and booleans
        anim.SetBool("IsIdle", !walking && !shooting && !planting && health >= 0);
        anim.SetBool("IsRunning", walking && !shooting && !planting && health >= 0);
        anim.SetBool("IsAiming", !walking && shooting && !planting && health >= 0);
        anim.SetBool("IsShootwalking", walking && shooting && !planting && health >= 0);
        anim.SetBool("IsPlanting", planting && !walking);
        anim.SetBool("IsDying", health <= 0); // Assuming health <= 0 triggers death animation

        if (planted)
        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f && planted)
        {
            timerEnded();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }
    private void FixedUpdate()
    {
        closestEnemy = FindClosestEnemy();

        // Do something with the closest enemy ( target)
        if (closestEnemy != null)
        {
            shooting = true;
            Quaternion offsetRotation = Quaternion.Euler(0f, 66f, 0f); // 66 degrees on the Y-axis
            Vector3 targetDir = closestEnemy.transform.position - transform.position;

            transform.LookAt(closestEnemy.transform); //Quaternion.LookRotation(targetDir) * offsetRotation;
        }
        else
            shooting = false;

        JoystickMovement();

        //Check if character is moving
        if ((_joystick.Horizontal != 0 || _joystick.Vertical != 0) && closestEnemy == null)
        {
            //Rotation
            transform.rotation = Quaternion.LookRotation(_rigidBody.velocity);
        }
    }
    void timerEnded()
    {
        c4.GetComponentInChildren<ParticleSystem>().Play();
        audioSource.PlayOneShot(explosionSFX);
        redTeamWinsText.enabled = true;
        planted = false;
    }
    public void PB()
    {
        StartCoroutine(PlantBomb());
    }
    public IEnumerator PlantBomb()
    {
        // Check if player has the bomb
        if (canPlant)
        {
            // Raycast down to find nearest ground
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                // Calculate position a little further from the player
                Vector3 plantingOffset = new Vector3(0f, 1f, 6f); // Adjust offset as needed

                Vector3 plantingPosition = hit.point + plantingOffset;

                // Check if new position is valid (not colliding with anything)
                if (Physics.OverlapSphere(plantingPosition, 0.2f, LayerMask.GetMask("Ground")).Length == 0)
                {
                    // Place the bomb at the valid position
                    GameObject bomb = Instantiate(c4, plantingPosition, Quaternion.identity);
                    bomb.transform.GetChild(2).gameObject.SetActive(false);

                    //Play planting animation/sound effect
                    audioSource.PlayOneShot(plantingSFX);
                    planting = true;
                    _joystick.enabled = false;
                    yield return new WaitForSeconds(5f);
                    _joystick.enabled = true;
                    planting = false;
                    // Remove bomb from player inventory
                    Destroy(c4);

                    c4 = bomb;
                    c4.transform.localScale = new Vector3(22f, 22f, 22f);

                    planted = true;

                    plantButton.SetActive(false);
                }
                else
                {
                    // Show error message or handle no valid position situation (optional)
                    Debug.Log("No valid spot to plant bomb nearby!");
                }
            }
            else
            {
                // Show error message or handle no ground found situation (optional)
                Debug.Log("No ground found to plant bomb!");
            }
        }
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        healthSlider.value = health;

        if (health <= 0)
        {
            blueTeamWinsText.enabled = true;
            Invoke(nameof(DestroyPlayer), 0.1f); 
        }
    }
    private void DestroyPlayer()
    {
        gameObject.GetComponent<CapsuleCollider>().height = 3.5f;
        gameObject.GetComponent<CapsuleCollider>().center = new Vector3(0, 2.25f, 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        gameObject.GetComponent<PlayerController>().enabled = false;
    }
    private void JoystickMovement()
    {
        walking = true;
        float horizontalInput = 0;
        float verticalInput = 0;

        // Check for WASD keys
        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }

        // Apply movement with move speed
        _rigidBody.velocity = new Vector3(horizontalInput * _moveSpeed, _rigidBody.velocity.y, verticalInput * _moveSpeed);

        _rigidBody.velocity = new Vector3(_joystick.Horizontal * _moveSpeed, _rigidBody.velocity.y, _joystick.Vertical * _moveSpeed);
    }

    public void Shoot()
    {
        if (canShoot)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, gameObject.transform.rotation);
            audioSource.PlayOneShot(shootSFX);
            shooting = true;
            Destroy(projectile, 4f);
            // Get the player's forward direction
            Vector3 forwardDirection = transform.forward;
            // Optionally add force to the projectile in the forward direction
            projectile.GetComponent<Rigidbody>().AddForce(forwardDirection * projectileForce);

            // Start cooldown timer
            canShoot = false;
            StartCoroutine(ShootCooldown());
        }
    }
    
    IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
        shooting = false;
    }

}
