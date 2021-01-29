using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
	public KeyCode actionKey = KeyCode.Space;
	public float catchRadius = 2f;
	public LayerMask catchLayerMask;
	public Transform itemAnchor;
	public Transform catchPoint;
	public float movementSpeedInSpace = 10f;
	public float turnSpeedInSpace = 10f;
	public float planetaryModeDistance;
	public SpriteRenderer rocketFlame;
	private Rigidbody2D m_Rigidbody;
	private List<Planet> planets;
	public SpriteRenderer body;

	private Item currentItem;

	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody2D>();
		rocketFlame.enabled = false;
		planets = GameObject.FindObjectsOfType<Planet>().ToList();
	}


	private void FixedUpdate()
	{
		Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		Vector2 movementDirection = -transform.right;
		rocketFlame.enabled = movementInput.y > Mathf.Epsilon;

		float forwardInput = Mathf.Max(0, movementInput.y);

		m_Rigidbody.AddForce(forwardInput * movementSpeedInSpace * movementDirection, ForceMode2D.Force);
		m_Rigidbody.AddTorque(-movementInput.x * turnSpeedInSpace);

		foreach (Planet planet in planets)
		{
			Vector2 toPlanet = planet.transform.position - transform.position;
			if (toPlanet.magnitude < planet.gravityDistance)
			{
				Vector2 force = toPlanet.normalized * planet.gravityMagnitude / toPlanet.sqrMagnitude;
				m_Rigidbody.AddForce(force, ForceMode2D.Force);
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(actionKey))
		{
			if (currentItem == null)
			{
				Collider2D[] colliders = Physics2D.OverlapCircleAll(catchPoint.position, catchRadius, catchLayerMask);
				Item item = colliders.Select(c => c.GetComponent<Item>()).FirstOrDefault();
				if (item != null) // catch
				{
					currentItem = item;
					currentItem.OnCatch(itemAnchor);
				}
			}
			else // release
			{
				currentItem.OnRelease();
				currentItem = null;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(catchPoint.position, catchRadius);
	}
}
