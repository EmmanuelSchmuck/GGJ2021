using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDude : MonoBehaviour
{
	public InstrumentKind DesiredInstrumentKind;
	
	private void OnTriggerEnter2D(Collider2D other)
	{
		Item item = other.GetComponent<Item>();
		if (item != null)
		{
			Debug.Log($"The item {item.name} is close to me, yay !!");

			if (item.GetComponent<Instrument>()?.Kind == DesiredInstrumentKind)
			{
				// updates game state.
				GameState.Instance.OnItemReturnedToOwner(DesiredInstrumentKind);
			}
		}
	}
}
