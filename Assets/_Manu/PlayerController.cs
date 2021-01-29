using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float movementSpeedInSpace = 10f;
	public float turnSpeedInSpace = 10f;
	public SpriteRenderer rocketFlame;
	private Rigidbody2D m_Rigidbody;
    
    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
		rocketFlame.enabled = false;
    }

   
    void FixedUpdate()
    {
        Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
		Vector2 movementDirection = -transform.right;
		rocketFlame.enabled = movementInput.y > Mathf.Epsilon;

		float forwardInput = Mathf.Max(0,movementInput.y);

		m_Rigidbody.AddForce(forwardInput * movementSpeedInSpace * movementDirection, ForceMode2D.Force);
		m_Rigidbody.AddTorque(-movementInput.x * turnSpeedInSpace);
    }
}
