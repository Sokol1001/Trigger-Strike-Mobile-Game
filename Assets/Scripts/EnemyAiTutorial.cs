using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAiTutorial : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSFX;

    public LayerMask whatIsGround, whatIsPlayer;
    public Animator animator; // Add animator reference

    public float health;
    public Slider healthSlider;
    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = FindClosestPlayer().transform;
        agent = GetComponent<NavMeshAgent>();
    }
    private GameObject FindClosestPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = collider.gameObject;
            }
        }

        return closestPlayer;
    }
    public void DeactivateEnemy()
    {
        enabled = false;
        agent.enabled = false;
    }
    private void Update()
    {
        // Check for players and deactivate if none found
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);
        if (hitColliders.Length == 0)
        {
            DeactivateEnemy();
        }
        else
        {
            player = FindClosestPlayer().transform;
        }
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Update animation state based on behavior
        UpdateAnimationState();

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(25);
        }
    }
    private void UpdateAnimationState()
    {
        animator.SetBool("IsIdle", !playerInSightRange && !playerInAttackRange && !walkPointSet); // Not patrolling
        animator.SetBool("IsRunning", playerInSightRange && !playerInAttackRange && health > 0); // Patrolling 
        animator.SetBool("IsAiming", playerInAttackRange && playerInSightRange); // Attacking
        animator.SetBool("IsDying", health <= 0); // Assuming health <= 0 triggers death animation
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if(player != null)
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            ///Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position, gameObject.transform.rotation).GetComponent<Rigidbody>();
            audioSource.PlayOneShot(shootSFX);

            rb.AddForce(transform.forward * 90f, ForceMode.Impulse);
            rb.AddForce(transform.up * 1f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthSlider.value -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.1f);
    }
    private void DestroyEnemy()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
        gameObject.GetComponent<CapsuleCollider>().height = 2.2f;
        gameObject.GetComponent<CapsuleCollider>().center = new Vector3(0, 2.25f, 0);
        agent.enabled = false;
        gameObject.GetComponent<EnemyAiTutorial>().enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
