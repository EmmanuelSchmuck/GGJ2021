using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Speaker))]
public class PlanetDude : MonoBehaviour
{
	public GameObject questMarker;
	public Speaker speaker => m_Speaker;
	
	public InstrumentKind DesiredInstrumentKind;
	public float maxDropAgeForResponse = 5f;

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
					GameState.Instance.OnItemReturnedToOwner(DesiredInstrumentKind, this);
					questMarker.SetActive(false);
					

					// take the item.
					m_Inventory.AcceptItem(item);
				}
				else
				{
					// shows dialog if this can be interpreted as intentional from the player
					if (Time.time - item.lastTimeOfDrop < maxDropAgeForResponse)
					{
						GameState.Instance.OnItemDeniedByCharacter(instrument.Kind, this);
					}
				}
			}
			else
			{
				// dialog.
				//m_Speaker.Speak("woot?");
			}
		}
	}
}
