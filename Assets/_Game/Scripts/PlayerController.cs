using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Inventory))]
public class PlayerController : MonoBehaviour
{
	public KeyCode actionKey = KeyCode.Space;
	public float catchRadius = 2f;
	public LayerMask catchLayerMask;
	public Transform catchPoint;
	public float movementSpeedInSpace = 10f;
	public float turnSpeedInSpace = 10f;
	public float planetaryModeDistance;
	public bool useDirectionInvertionBelowPlanet;
	public float movementSpeedOnPlanet = 10f;
	public float altitudeOffset = 1f;
	public AnimationCurve landingCurve;
	public float landingAnimationDuration;
	public SpriteRenderer rocketFlame;
	private Rigidbody2D m_Rigidbody;
	private Collider2D m_Collider;
	private Inventory m_inventory;
	private List<Planet> planets;
	public SpriteRenderer body;

	private Planet currentPlanet;
	private bool jumpBuffered;
	private bool isAnimating;

	private bool invertDirectionOnPlanet;

	private Vector2 lastMovementInput;

	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody2D>();
		m_Collider = GetComponent<Collider2D>();
		m_inventory = GetComponent<Inventory>();
		rocketFlame.enabled = false;
		planets = GameObject.FindObjectsOfType<Planet>().ToList();
	}


	private void FixedUpdate()
	{
		if(isAnimating) return;

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
				if (toPlanet.magnitude - planet.Radius < planetaryModeDistance && currentPlanet == null && !isAnimating)
				{
					StartCoroutine(LandOnPlanet(planet));
				}
			}
		}
		else // on planet
		{
			if(lastMovementInput.x * movementInput.x < Mathf.Epsilon)
			{
				invertDirectionOnPlanet = useDirectionInvertionBelowPlanet && transform.position.y - currentPlanet.transform.position.y < 0;
			}
			// movementInput *= invertDirectionOnPlanet ? -1 : +1;
			Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
			float altitude = planetCoreToDog.magnitude - currentPlanet.Radius;
			Vector2 movement = Vector3.Cross(Vector3.back, planetCoreToDog.normalized) * movementInput.x * movementSpeedOnPlanet;
			movement *= invertDirectionOnPlanet ? -1 : +1;
			transform.position += (Vector3)movement * Time.deltaTime;
			planetCoreToDog = transform.position - currentPlanet.transform.position;
			transform.position = currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset);
			transform.rotation = Quaternion.Euler(0, 0, 180 - Vector2.SignedAngle(-planetCoreToDog.normalized, Vector2.up));
			if (Mathf.Abs(movementInput.x) > Mathf.Epsilon)
			{
				body.transform.localScale = new Vector3(movementInput.x*(invertDirectionOnPlanet ? -1 : +1)<0?1:-1,1,1);
			}
		}

		lastMovementInput = movementInput;
	}

	private IEnumerator LandOnPlanet(Planet planet)
	{
		currentPlanet = planet;
		Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
		yield return StartCoroutine(LerpPositionAndRotation(
			currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset),
			Quaternion.Euler(0, 0, 180 - Vector2.SignedAngle(-planetCoreToDog.normalized, Vector2.up))));
		m_Rigidbody.simulated = false;
		m_Collider.isTrigger = true;
		rocketFlame.enabled = false;
		transform.SetParent(currentPlanet.transform);
		// yield return null;
	}

	private IEnumerator LerpPositionAndRotation(Vector3 location, Quaternion rotation)
	{
		isAnimating = true;
		float duration = landingAnimationDuration;
		float timer = 0f;
		Vector3 startPosition = transform.position;
		Quaternion startRotation = transform.rotation;
		while(timer < duration)
		{	
			float alpha = landingCurve.Evaluate(timer/duration);
			transform.position = Vector3.Lerp(startPosition, location, alpha);
			transform.rotation = Quaternion.Slerp(startRotation, rotation, alpha);
			timer += Time.deltaTime;
			yield return null;
		}
		isAnimating = false;
	}

	private IEnumerator LeavePlanet()
	{
		Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
		transform.SetParent(null);
		body.transform.localScale = Vector3.one;
		yield return StartCoroutine(LerpPositionAndRotation(
			currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset + planetaryModeDistance + 2f),
			transform.rotation));
		transform.position = currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset + planetaryModeDistance + 2f);
		m_Rigidbody.simulated = true;
		m_Collider.isTrigger = false;
		m_Rigidbody.velocity = 3 * planetCoreToDog.normalized;
		currentPlanet = null;
		// yield return null;
	}

	private void Update()
	{
		if(isAnimating) return;

		if (Input.GetKeyDown(actionKey))
		{
			if (m_inventory.IsFree)
			{
				Collider2D[] colliders = Physics2D.OverlapCircleAll(catchPoint.position, catchRadius, catchLayerMask);
				Item item = colliders.Select(c => c.GetComponent<Item>()).FirstOrDefault();
				if (item != null) // catch
				{
					m_inventory.AcceptItem(item);
				}
			}
			else // release
			{
				m_inventory.DropItem();
			}
		}

		if (currentPlanet != null && Input.GetAxisRaw("Vertical") > Mathf.Epsilon && !isAnimating)
		{
			StartCoroutine(LeavePlanet());
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(catchPoint.position, catchRadius);
	}
}
