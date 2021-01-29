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

    public bool IsFree => currentItem == null;

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

			currentItem.OnRelease();
			currentItem = null;
		}
	}
}
