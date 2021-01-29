﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// can be grabbed & released by dog
[RequireComponent(typeof(Collider2D))]
public class Item : MonoBehaviour
{
	public bool IsInBackpack;
	protected void Awake()
	{
		this.gameObject.layer = LayerMask.NameToLayer("Item");
	}	
}
