using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Speaker))]
public class PlanetDude : MonoBehaviour
{
	public InstrumentKind DesiredInstrumentKind;

	private Inventory m_Inventory;
	private Speaker m_Speaker;

	private void Awake()
	{
		m_Inventory = GetComponent<Inventory>();
		m_Speaker = GetComponent<Speaker>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		Item item = other.GetComponent<Item>();
		if (item != null)
		{
			Debug.Log($"The item {item.name} is close to me, yay !!");

			Instrument instrument = item.GetComponent<Instrument>();

			if (instrument != null)
			{
				if (instrument.Kind == DesiredInstrumentKind)
				{
					// updates game state.
					GameState.Instance.OnItemReturnedToOwner(DesiredInstrumentKind);

					// take the item.
					m_Inventory.AcceptItem(item);

					// dialog.
					m_Speaker.Speak("omg awesom");
				}
				else
				{
					// dialog.
					m_Speaker.Speak("not my stuff!");
				}
			}
			else
			{
				// dialog.
				m_Speaker.Speak("woot?");
			}
		}
	}
}
