using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes an entity be able to receive an item.
/// </summary>
public class Inventory : MonoBehaviour
{
    public Transform itemAnchor;

    private Item currentItem;

	public event System.Action<Inventory, Item> ItemAccepted, ItemDropped;

    public bool IsFree => currentItem == null;

	public Instrument CurrentInstrument => currentItem == null ? null : currentItem.GetComponent<Instrument>();

    public void AcceptItem(Item item)
	{
		// Drop existing item.
		if (currentItem != null)
		{
			DropItem();
		}

		// Set and transform target item.
		currentItem = item;
		currentItem.OnCatch(itemAnchor);

		ItemAccepted?.Invoke(this, item);

		Debug.Log($"{gameObject.name} accepted {item.gameObject.name}");
	}

    public void DropItem()
	{
		if (currentItem == null)
		{
			Debug.LogWarning($"{gameObject.name} has no item to drop.");
		}
		else
		{
			Debug.Log($"{gameObject.name} dropped {currentItem.gameObject.name}");

			var item = currentItem;
			currentItem.OnRelease();
			currentItem = null;

			ItemDropped?.Invoke(this, item);
		}
	}
}
