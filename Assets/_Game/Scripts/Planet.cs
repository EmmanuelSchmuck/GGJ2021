using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	public float gravityDistance = 10;
	public float gravityMagnitude = 1f;
	[SerializeField] private CircleCollider2D m_CircleCollider;
    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnDrawGizmos()
	{
		float surfaceRadius = 2*m_CircleCollider.radius;

		Gizmos.DrawWireSphere(transform.position, surfaceRadius);
		Gizmos.DrawWireSphere(transform.position, gravityDistance);
	}
}
