using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
	#region Static
	private static GameState instance;
	public static GameState Instance => instance ?? (instance = FindObjectOfType<GameState>());
	#endregion

	#region Scene
	public PlayerController player;
	public FuelBar fuelBar;
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

	private void Start()
	{
		// starts the game when the game scene starts
		if (SceneManager.GetActiveScene().name.Contains("MainGame"))
		{
			OnGameStart();
		}
	}

	#region Story / Progression
	public void OnGameStart()
	{
		Debug.Log("GameState starting game!");
		
		// player starts initially on the concert planet
		SetPlayerCanTakeoff(false);
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
	#endregion

	#region HUD / Gameplay
	public void SetPlayerCanTakeoff(bool canTakeoff)
	{
		//player.canGoToSpace = canTakeoff;
		fuelBar.gameObject.SetActive(canTakeoff);
	}
	#endregion
}
