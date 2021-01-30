using System;
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

	#region Scene Deps
	public PlayerController player;
	public FuelBar fuelBar;
	public HelperBox helperBox;
	#endregion

	#region State
	private Dictionary<InstrumentKind, InstrumentState> instrumentStates;
	#endregion

	#region Extra Coroutines
	private class WaitForPlayerNearTarget : CustomYieldInstruction
	{
		private readonly GameObject target;
		private readonly PlayerController player;

		private bool isPlayerNearTarget;

		public WaitForPlayerNearTarget(PlanetDude dude) : this(dude.gameObject) { }

		public WaitForPlayerNearTarget(GameObject obj)
		{
			this.target = obj;
			this.player = instance.player;

			player.ProximityEnter += OnProximity;
		}

		private void OnProximity(PlayerController player, GameObject obj)
		{
			if (obj != null && obj == target)
			{
				player.ProximityEnter -= OnProximity;
				isPlayerNearTarget = true;
			}
		}

		public override bool keepWaiting => !isPlayerNearTarget;
	} 
	#endregion

	private void Awake()
	{
		instance = instance ?? this;

		// init substates
		var owners = FindObjectsOfType<PlanetDude>();
		instrumentStates = System.Enum.GetValues(typeof(InstrumentKind)).OfType<InstrumentKind>()
			.ToDictionary(kind => kind, kind => new InstrumentState()
			{
				Kind = kind,
				Owner = owners.FirstOrDefault(dude => dude.DesiredInstrumentKind == kind)
			});
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

		StartCoroutine(Tutorial_Start());
	}

	private IEnumerator Tutorial_Start()
	{
		// player starts initially on the concert planet
		SetPlayerInteractive(true);
		SetPlayerCanTakeoff(false);
		helperBox.DisplayText(HelperBoxText.PlanetMovement);

		// waits for the player to reach the NPC.
		PlanetDude mikeOwner = instrumentStates[InstrumentKind.Microphone].Owner;
		yield return new WaitForPlayerNearTarget(mikeOwner);

		// dialog sequence.
		helperBox.Hide();
		SetPlayerInteractive(false);
		mikeOwner.speaker.SetFontSize(Speaker.FontSize.Tiny);
		yield return StartCoroutine(RunDialog(
			(mikeOwner.speaker, "help!", 1f),
			(player.speaker, "oh hi there!", 2f),
			(mikeOwner.speaker, "hi! help!", 1f),
			(null, null, 0.5f),
			(player.speaker, "sorry I can't hear you...", 3f),
			(mikeOwner.speaker, "...", 1f)));
		mikeOwner.speaker.SetFontSize(Speaker.FontSize.Small);
		yield return StartCoroutine(RunDialog(
			(mikeOwner.speaker, "HI! HELP!", 2f),
			(player.speaker, "oh hi what's up?", 2f),
			(mikeOwner.speaker, "I'm very sad :(", 1f),
			(mikeOwner.speaker, "I lost my microphone!", 3f),
			(player.speaker, "oh I'm sorry!", 2f),
			(mikeOwner.speaker, "I can't sing without it!", 3f),
			(mikeOwner.speaker, "please find it!", 2f)));
	}

	//public void OnPlayerProximity(PlayerController player, GameObject gameObject)
	//{

	//}

	//public void OnPlayerProximityEnd(PlayerController player, GameObject gameObject)
	//{

	//}

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

	#region Dialogs

	private IEnumerator RunDialog(Speaker speaker, params (string line, float duration)[] lines) 
		=> RunDialog(lines.Select(l => (speaker, l.line, l.duration)).ToArray());

	private IEnumerator RunDialog(params (Speaker speaker, string line, float duration)[] lines)
	{
		foreach (var spec in lines)
		{
			if (spec.speaker != null)
			{
				spec.speaker.Speak(spec.line, spec.duration); 
			}
			yield return new WaitForSeconds(spec.duration * 0.95f);
			// spawn next dialog slightly before the last one faded out.
		}
	}
	#endregion

	#region HUD / Gameplay
	public void SetPlayerCanTakeoff(bool canTakeoff)
	{
		player.canGoToSpace = canTakeoff;
		fuelBar.gameObject.SetActive(canTakeoff);
	}

	public void SetPlayerInteractive(bool isInteractive)
	{
		player.canMove = isInteractive;
		player.canUseActionKey = isInteractive;
	}
	#endregion
}
