using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	public float gravityDistance = 10;
	public float gravityMagnitude = 1f;

	public float rotationSpeed;
	[SerializeField] private CircleCollider2D m_CircleCollider;

	public float Radius {get; private set;}

	private Vector2 basePosition;
    // Start is called before the first frame update
    void Awake()
    {
		basePosition = transform.position;
		Radius = 2*m_CircleCollider.radius;
    }

    // Update is called once per frame
    void Update()
    {
		float dt = Time.deltaTime;
        transform.RotateAround(basePosition, Vector3.back, rotationSpeed * dt);
    }

	private void OnDrawGizmos()
	{
		float surfaceRadius = 2*m_CircleCollider.radius;

		Gizmos.DrawWireSphere(transform.position, surfaceRadius);
		Gizmos.DrawWireSphere(transform.position, gravityDistance);
	}
}
