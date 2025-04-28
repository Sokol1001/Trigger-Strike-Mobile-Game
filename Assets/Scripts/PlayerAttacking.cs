using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacking : MonoBehaviour
{
    [SerializeField] LineRenderer LR;

    [SerializeField] FloatingJoystick attackJoystick;

    [SerializeField] Transform AttackLookAtPoint;

    [SerializeField] float TrailDistance = 1;

    [SerializeField] Transform Player;
    RaycastHit hit;

    void Start()
    {
        
    }

    void Update()
    {
        if(Mathf.Abs(attackJoystick.Horizontal) > 0.5f || Mathf.Abs(attackJoystick.Vertical) > 0.5f)
        {
            transform.position = new Vector3(Player.position.x, -1.54f, Player.position.z);
            AttackLookAtPoint.position = new Vector3(attackJoystick.Horizontal + transform.position.x, -1.54f, attackJoystick.Vertical + transform.position.z);
            transform.LookAt(new Vector3(AttackLookAtPoint.position.x, 0, AttackLookAtPoint.position.z));
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            LR.SetPosition(0, transform.position);
            if(Physics.Raycast(transform.position, transform.forward, out hit, TrailDistance))
            {
                LR.SetPosition(1, hit.point);
            }
        }
    }
}
