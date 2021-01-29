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
	public float movementSpeedOnPlanet = 10f;
	public float altitudeOffset = 1f;
	public SpriteRenderer rocketFlame;
	private Rigidbody2D m_Rigidbody;
	private Collider2D m_Collider;
	private List<Planet> planets;
	public SpriteRenderer body;

	private Item currentItem;
	private Planet currentPlanet;
	private bool jumpBuffered;

	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody2D>();
		m_Collider = GetComponent<Collider2D>();
		rocketFlame.enabled = false;
		planets = GameObject.FindObjectsOfType<Planet>().ToList();
	}


	private void FixedUpdate()
	{
		Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		if (currentPlanet == null) // in space
		{
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
				if (toPlanet.magnitude - planet.Radius < planetaryModeDistance && currentPlanet == null)
				{
					LandOnPlanet(planet);
				}
			}
		}
		else // on planet
		{
			Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
			float altitude = planetCoreToDog.magnitude - currentPlanet.Radius;
			Vector2 movement = Vector3.Cross(Vector3.back, planetCoreToDog.normalized) * movementInput.x * movementSpeedOnPlanet;
			transform.position += (Vector3)movement * Time.deltaTime;
			planetCoreToDog = transform.position - currentPlanet.transform.position;
			transform.position = currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset);
			transform.rotation = Quaternion.Euler(0, 0, 180 - Vector2.SignedAngle(-planetCoreToDog.normalized, Vector2.up));
			if (Mathf.Abs(movementInput.x) > Mathf.Epsilon)
			{
				body.transform.localScale = new Vector3(movementInput.x<0?1:-1,1,1);
			}
		}
	}

	private void LandOnPlanet(Planet planet)
	{
		currentPlanet = planet;
		transform.SetParent(currentPlanet.transform);
		m_Rigidbody.simulated = false;
		m_Collider.isTrigger = true;
		rocketFlame.enabled = false;
	}

	private void LeavePlanet(Planet planet)
	{
		Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
		transform.SetParent(null);
		transform.position = currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset + planetaryModeDistance + 2f);
		body.transform.localScale = Vector3.one;
		m_Rigidbody.simulated = true;
		m_Collider.isTrigger = false;
		rocketFlame.enabled = false;
		m_Rigidbody.AddForce(5 * planetCoreToDog.normalized, ForceMode2D.Impulse);
		currentPlanet = null;
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

		if (currentPlanet != null && Input.GetAxisRaw("Vertical") > Mathf.Epsilon)
		{
			LeavePlanet(currentPlanet);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(catchPoint.position, catchRadius);
	}
}
