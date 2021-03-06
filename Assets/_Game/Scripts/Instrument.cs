﻿using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class Instrument : MonoBehaviour
{
	public InstrumentKind Kind;

	public Item AsItem() => GetComponent<Item>();
}