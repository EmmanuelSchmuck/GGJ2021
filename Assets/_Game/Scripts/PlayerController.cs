using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Speaker))]
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
	public AnimationCurve walkCurve;
	public float walkOffset;
	public float walkStepLength;
	public AnimationCurve landingCurve;
	public float landingAnimationDuration;
	public AnimationCurve boneEffectCurve;
	public SpriteRenderer rocketFlame;
	private Rigidbody2D m_Rigidbody;
	private Collider2D m_Collider;
	private Inventory m_Inventory;
	private Speaker m_Speaker;
	private List<Planet> planets;
	public SpriteRenderer body;
	private Planet currentPlanet;
	private bool jumpBuffered;
	private bool isAnimating;
	private bool invertDirectionOnPlanet;
	private Vector2 lastMovementInput;
	private FuelBar fuelBar;
	public float startingFuel;
	public float minFuel;
	public float fuelConsumptionRate;
	private float currentFuel;

	public bool canMove;
	public bool canGoToSpace;
	public bool canUseActionKey;

	public GameObject bonePrefab;

	private bool hasHelperBone;

	public float stepCycle;

	public event System.Action<PlayerController> ActionKeyDown, LeavePlanet, LandOnPlanet;
	public event System.Action<PlayerController,GameObject> ProximityEnter, ProximityLeave;

	public Speaker speaker => m_Speaker;
	public Inventory inventory => m_Inventory;

	private void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody2D>();
		m_Collider = GetComponent<Collider2D>();
		m_Inventory = GetComponent<Inventory>();
		m_Speaker = GetComponent<Speaker>();
		fuelBar = GameObject.FindObjectOfType<FuelBar>();
		rocketFlame.enabled = false;
		planets = GameObject.FindObjectsOfType<Planet>().ToList();

		currentFuel = startingFuel;
		fuelBar?.SetFuel(currentFuel);
	}


	private void FixedUpdate()
	{
		if(isAnimating) return;

		Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		movementInput *= canMove ? 1 : 0;

		if (currentPlanet == null) // in space
		{
			Vector2 movementDirection = -transform.right;
			rocketFlame.enabled = movementInput.y > Mathf.Epsilon;
			if(movementInput.y>Mathf.Epsilon)
			{
				currentFuel -= fuelConsumptionRate * Time.deltaTime;
			}

			float forwardInput = Mathf.Max(0, movementInput.y);
			float fuelMultiplier = Mathf.Sqrt(currentFuel);

			m_Rigidbody.AddForce(forwardInput * movementSpeedInSpace * movementDirection * fuelMultiplier, ForceMode2D.Force);
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
					StartCoroutine(LandingOnPlanet(planet));
				}
			}
		}
		else // on planet
		{
			
		}

		
	}

	private IEnumerator SpawnHelperBone()
	{
		int trials = 0;
		int maxtrial = 100;
		float boneDistance = 10;
		float minDistanceToStuff = 2f;
		bool success = false;
		Vector2 bonePosition = Vector2.zero;

		hasHelperBone = true;

		while(true)
		{
			trials++;
			// if(trials>maxtrial/2) boneDistance *= 0.66f;
			if(trials==maxtrial) break;
			yield return null;
			bonePosition = (Vector2)transform.position + Random.insideUnitCircle.normalized * boneDistance;
			if(Physics2D.OverlapCircle(bonePosition,minDistanceToStuff)==null)
			{
				success = true;
				break;
			}
		}

		if(success)
		{
			Instantiate(bonePrefab,bonePosition,Quaternion.identity);
			Debug.Log("here is a helper bone :)");
		}
		else
		{
			Debug.LogError("Cannot create helper bone :((");
		}
		
	}

	private IEnumerator LandingOnPlanet(Planet planet)
	{
		currentPlanet = planet;
		Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
		yield return StartCoroutine(LerpPositionAndRotation(
			currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset),
			Quaternion.Euler(0, 0, 180 - Vector2.SignedAngle(-planetCoreToDog.normalized, Vector2.up))));
		// m_Rigidbody.simulated = false;
		m_Rigidbody.velocity = Vector2.zero;
		m_Rigidbody.angularVelocity = 0f;
		// m_Rigidbody.inertia = 0;
		m_Collider.isTrigger = false;
		rocketFlame.enabled = false;
		transform.SetParent(currentPlanet.transform);

		LandOnPlanet?.Invoke(this);
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

	private IEnumerator LeavingPlanet()
	{
		Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
		transform.SetParent(null);
		body.transform.localScale = Vector3.one;
		yield return StartCoroutine(LerpPositionAndRotation(
			currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset + planetaryModeDistance + 2f),
			transform.rotation));
		transform.position = currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * (currentPlanet.Radius + altitudeOffset + planetaryModeDistance + 2f);
		// m_Rigidbody.simulated = true;
		m_Collider.isTrigger = false;
		m_Rigidbody.velocity = Vector2.zero;
		m_Rigidbody.angularVelocity = 0f;
		// m_Rigidbody.inertia = 0;
		m_Rigidbody.velocity = 3 * planetCoreToDog.normalized;
		currentPlanet = null;

		LeavePlanet?.Invoke(this);
		// yield return null;
	}

	private void Update()
	{
		currentFuel = Mathf.Clamp(currentFuel,minFuel,1f);
		fuelBar?.SetFuel(currentFuel);
		if(currentFuel < minFuel + 0.05f)
		{
			if(!hasHelperBone) StartCoroutine(SpawnHelperBone());
		}
		else
		{
			hasHelperBone = false;
		}

		if(isAnimating) return;

		if (Input.GetKeyDown(actionKey) && canUseActionKey)
		{
			if (m_Inventory.IsFree)
			{
				Collider2D[] colliders = Physics2D.OverlapCircleAll(catchPoint.position, catchRadius, catchLayerMask);
				Item item = colliders.Select(c => c.GetComponent<Item>()).FirstOrDefault();
				if (item != null) // catch
				{
					m_Inventory.AcceptItem(item);
				}
			}
			else // release
			{
				m_Inventory.DropItem();
			}

			// lets other components do stuff.
			ActionKeyDown?.Invoke(this);
		}

		if (canGoToSpace && currentPlanet != null && Input.GetKeyDown(KeyCode.UpArrow) && !isAnimating)
		{
			StartCoroutine(LeavingPlanet());
		}

		if(currentPlanet != null) // walk on planet
		{
			Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			movementInput *= canMove ? 1 : 0;
			stepCycle = Mathf.Repeat(stepCycle + Mathf.Abs(movementInput.x) * Time.deltaTime/walkStepLength, 1f);
			if(lastMovementInput.x * movementInput.x < Mathf.Epsilon)
			{
				invertDirectionOnPlanet = useDirectionInvertionBelowPlanet && transform.position.y - currentPlanet.transform.position.y < 0;
			}
			// movementInput *= invertDirectionOnPlanet ? -1 : +1;
			Vector2 planetCoreToDog = transform.position - currentPlanet.transform.position;
			// float altitude = planetCoreToDog.magnitude - currentPlanet.Radius + walkOffset * walkCurve.Evaluate(stepCycle);
			Vector2 movement = Vector3.Cross(Vector3.back, planetCoreToDog.normalized) * movementInput.x * movementSpeedOnPlanet;
			movement *= invertDirectionOnPlanet ? -1 : +1;
			transform.position += (Vector3)movement * Time.deltaTime;
			planetCoreToDog = transform.position - currentPlanet.transform.position;
			float altitude = currentPlanet.Radius + altitudeOffset + walkOffset * walkCurve.Evaluate(stepCycle);
			transform.position = currentPlanet.transform.position + (Vector3)planetCoreToDog.normalized * altitude;
			transform.rotation = Quaternion.Euler(0, 0, 180 - Vector2.SignedAngle(-planetCoreToDog.normalized, Vector2.up));
			
			if (Mathf.Abs(movementInput.x) > Mathf.Epsilon)
			{
				body.transform.localScale = new Vector3(movementInput.x*(invertDirectionOnPlanet ? -1 : +1)<0?1:-1,1,1);
			}

			lastMovementInput = movementInput;
		}
	}

	private IEnumerator BoneEffect(float intensity, float duration)
	{
		float startSpeed = movementSpeedInSpace;
		float endSpeed = movementSpeedInSpace + intensity;
		float timer = 0f;
		while(timer < duration)
		{
			timer += Time.deltaTime;
			float alpha = boneEffectCurve.Evaluate(timer/duration);
			movementSpeedInSpace = Mathf.Lerp(startSpeed, endSpeed, alpha);
			yield return null;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		Bone bone = other.GetComponent<Bone>();
		if(bone != null)
		{
			bone.OnBeingCollected();
			currentFuel += Mathf.Lerp(bone.extraFuelMin, bone.extraFuelMax, 1f-currentFuel);
			StartCoroutine(BoneEffect(bone.speedBoostIntensity, bone.speedBoostDuration));
		}

		Debug.Log($"Dog enter trigger: {other.gameObject.name}");
		ProximityEnter?.Invoke(this,other.gameObject);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		Debug.Log($"Dog exit trigger: {other.gameObject.name}");
		ProximityLeave?.Invoke(this,other.gameObject);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(catchPoint.position, catchRadius);
	}
}
