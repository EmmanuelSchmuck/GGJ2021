using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState : MonoBehaviour
{
	#region Static
	private static GameState instance;
	public static GameState Instance => instance ?? (instance = FindObjectOfType<GameState>());
	#endregion

	#region State
	private Dictionary<InstrumentKind, InstrumentState> instrumentStates; 
	#endregion

	private void Awake()
	{
        // init substates
        instrumentStates = System.Enum.GetValues(typeof(InstrumentKind)).OfType<InstrumentKind>()
            .ToDictionary(kind => kind, kind => new InstrumentState());
	}

	public void OnItemReturnedToOwner(InstrumentKind instrument)
	{
		// updates state
		instrumentStates[instrument].IsReturnedToOwner = true;

		int remaining = instrumentStates.Values.Count(state => !state.IsReturnedToOwner);
		Debug.Log($"Instrument {instrument} was returned to owner, {remaining} remaining.");

		// trigger game end if we are done
		if (remaining == 0)
		{
			Debug.Log("WIN!!");
		}
	}
}
