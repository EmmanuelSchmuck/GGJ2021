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
		Radius = m_CircleCollider.radius * transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
		float dt = Time.deltaTime;
        transform.RotateAround(basePosition, Vector3.back, rotationSpeed * dt);
    }

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, Radius);
		Gizmos.DrawWireSphere(transform.position, gravityDistance);
	}
}
