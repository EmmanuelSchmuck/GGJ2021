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
	private class WaitForPlayerGrabItem : CustomYieldInstruction
	{
		private readonly Item target;
		private readonly PlayerController player;

		private bool hasPlayerGrabbedItem;

		public WaitForPlayerGrabItem(Item obj)
		{
			this.target = obj;
			this.player = instance.player;

			player.inventory.ItemAccepted += OnItemAccepted;
		}

		private void OnItemAccepted(Inventory inventory, Item item)
		{
			if (item != null && item == true)
			{
				player.inventory.ItemAccepted -= OnItemAccepted;
				hasPlayerGrabbedItem = true;
			}
		}

		public override bool keepWaiting => !hasPlayerGrabbedItem;
	}

	private class WaitForPlayerNearTarget : CustomYieldInstruction
	{
		private readonly GameObject target;
		private readonly PlayerController player;

		private bool isPlayerNearTarget;

		public WaitForPlayerNearTarget(Component cmp) : this(cmp.gameObject) { }

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

	private class WaitForInstrumentReturned : CustomYieldInstruction
	{
		private readonly InstrumentKind kind;

		public WaitForInstrumentReturned(InstrumentKind kind)
		{
			this.kind = kind;
		}

		public override bool keepWaiting => !instance.instrumentStates[kind].IsReturnedToOwner;
	}

	private class WaitForPlayerInSpace : CustomYieldInstruction
	{
		private readonly PlayerController player;

		private bool isPlayerInSpace;

		public WaitForPlayerInSpace()
		{
			this.player = instance.player;

			player.LeftPlanet += OnPlayerLeftPlanet;
		}

		private void OnPlayerLeftPlanet(PlayerController obj)
		{
			player.LeftPlanet -= OnPlayerLeftPlanet;
			isPlayerInSpace = false;
		}

		public override bool keepWaiting => !isPlayerInSpace;
	}
	#endregion

	private void Awake()
	{
		instance = instance ?? this;

		// init substates
		var owners = FindObjectsOfType<PlanetDude>();
		var instrObjects = FindObjectsOfType<Instrument>();
		instrumentStates = System.Enum.GetValues(typeof(InstrumentKind)).OfType<InstrumentKind>()
			.ToDictionary(kind => kind, kind => new InstrumentState()
			{
				Kind = kind,
				Owner = owners.FirstOrDefault(dude => dude.DesiredInstrumentKind == kind),
				Object = instrObjects.FirstOrDefault(instru => instru.Kind == kind)
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

		StartCoroutine(RunTutorial());
	}

	private IEnumerator RunTutorial()
	{
		// player starts initially on the concert planet
		SetPlayerInteractive(true, alsoActionKey: false);
		SetPlayerCanUseActionKey(false);
		SetPlayerCanTakeoff(false);
		helperBox.DisplayText(HelperBoxText.PlanetMovement);

		// waits for the player to reach the NPC.
		PlanetDude mikeOwner = instrumentStates[InstrumentKind.Microphone].Owner;
		yield return new WaitForPlayerNearTarget(mikeOwner);

		// dialog sequence.
		helperBox.Hide();
		SetPlayerInteractive(false, alsoActionKey: false);
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

		// waits for player to be in reach of microphone
		SetPlayerInteractive(true);
		var mikeObject = instrumentStates[InstrumentKind.Microphone].Object;
		yield return new WaitForPlayerNearTarget(mikeObject);

		// waits for player to pick up.
		helperBox.DisplayText(HelperBoxText.CatchItem);
		yield return new WaitForPlayerGrabItem(mikeObject.AsItem());

		// waits for player to be close to NPC.
		helperBox.Hide();
		yield return new WaitForPlayerNearTarget(mikeOwner);

		// waits for player to give back item.
		helperBox.DisplayText(HelperBoxText.GiveItem);
		yield return new WaitForInstrumentReturned(InstrumentKind.Microphone);

		// final dialog
		helperBox.Hide();
		SetPlayerInteractive(false);
		yield return StartCoroutine(RunDialog(
			(mikeOwner.speaker, "...", 2f)));
		mikeOwner.speaker.SetFontSize(Speaker.FontSize.Huge);
		yield return StartCoroutine(RunDialog(
			(mikeOwner.speaker, "WOW!!", 2f)));
		mikeOwner.speaker.SetFontSize(Speaker.FontSize.Big);
		yield return StartCoroutine(RunDialog(
			(mikeOwner.speaker, "Thanks!", 1f),
			(mikeOwner.speaker, "Ready to rock!", 1f)));
		mikeOwner.speaker.SetFontSize(Speaker.FontSize.Regular);
		yield return StartCoroutine(RunDialog(
			(null, null, 0.5f),
			(mikeOwner.speaker, "But wait...", 3f),
			(mikeOwner.speaker, "My band...", 2f),
			(mikeOwner.speaker, "Where are they?", 2f),
			(null, null, 1f),
			(mikeOwner.speaker, "...", 2f),
			(mikeOwner.speaker, "Can you bring them back?", 3f)));
		SetPlayerInteractive(true);

		// wait for player to lift up in space
		SetPlayerCanTakeoff(true);
		helperBox.DisplayText(HelperBoxText.JumpToSpace);
		yield return new WaitForPlayerInSpace();

		// final hints
		helperBox.Hide();
		yield return new WaitForSeconds(1f);
		helperBox.DisplayText(HelperBoxText.SpaceMovement);
		yield return new WaitForSeconds(15f);
		helperBox.Hide();
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

	#region Dialogs

	private IEnumerator RunDialog(Speaker speaker, params (string line, float duration)[] lines) 
		=> RunDialog(lines.Select(l => (speaker, l.line, l.duration)).ToArray());

	private IEnumerator RunDialog(params (Speaker speaker, string line, float duration)[] lines)
	{
		foreach (var spec in lines)
		{
			if (Input.GetKey(KeyCode.Backspace))
			{
				yield break;
			}
			
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

	public void SetPlayerInteractive(bool isInteractive, bool alsoActionKey = true)
	{
		player.canMove = isInteractive;
		if (alsoActionKey)
		{
			player.canUseActionKey = isInteractive; 
		}
	}

	public void SetPlayerCanUseActionKey(bool canUse)
	{
		player.canUseActionKey = canUse;
	}
	#endregion
}
