﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
	public AudioSource endingSong;
	public Planet concertPlanet;
	#region Static
	private static GameState instance;
	public static GameState Instance => instance ?? FindObjectOfType<GameState>();
	#endregion

	#region Scene Deps
	public PlayerController player;
	public FuelBar fuelBar;
	public HelperBox helperBox;
	public TextMeshPro concertBannerText;
	#endregion

	#region Config
	public bool skipTutorial;
	#endregion

	#region State
	private Dictionary<InstrumentKind, InstrumentState> instrumentStates;
	private HashSet<Item> proximityItems;
	private HashSet<PlanetDude> proximityMusicians;
	private bool canAutoManageHelperBox;
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

	private class WaitForPlayerLeavePlanet : CustomYieldInstruction
	{
		private readonly PlayerController player;

		private bool isPlayerInSpace;

		public WaitForPlayerLeavePlanet()
		{
			this.player = instance.player;

			player.LeftPlanet += OnPlayerLeftPlanet;
		}

		private void OnPlayerLeftPlanet(PlayerController obj)
		{
			player.LeftPlanet -= OnPlayerLeftPlanet;
			isPlayerInSpace = true;
		}

		public override bool keepWaiting => !isPlayerInSpace;
	}
	#endregion

	private void Awake()
	{
		instance = this;

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
		proximityItems = new HashSet<Item>();
		proximityMusicians = new HashSet<PlanetDude>();
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

		// sets up common handlers
		player.ProximityEnter += OnCommonPlayerProximityEnter;
		player.ProximityLeave += OnCommonPlayerProximityLeave;

		// starts a quest 
		if (!skipTutorial)
		{
			StartCoroutine(RunTutorial());
		}
		else
		{
			StartCoroutine(RunMainQuests());
		}
	}

	private IEnumerator RunTutorial()
	{
		// player starts initially on the concert planet
		canAutoManageHelperBox = false;
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
			(mikeOwner.speaker, "Help!", 1f),
			(player.speaker, "Oh hi there!", 2f),
			(mikeOwner.speaker, "Hi! Help!", 1f),
			(null, null, 0.5f),
			(player.speaker, "Sorry I can't hear you...", 3f),
			(mikeOwner.speaker, "...", 1f)));
		mikeOwner.speaker.SetFontSize(Speaker.FontSize.Small);
		yield return StartCoroutine(RunDialog(
			(mikeOwner.speaker, "HI! HELP!", 2f),
			(mikeOwner.speaker, "I'm very sad :(", 1f),
			(mikeOwner.speaker, "I lost my microphone!", 3f),
			(player.speaker, "Oh I'm sorry!", 2f),
			(mikeOwner.speaker, "I can't sing without it!", 3f),
			(mikeOwner.speaker, "Please find it!", 2f)));
		instrumentStates[InstrumentKind.Microphone].IsIntroduced = true;

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
		yield return new WaitForPlayerLeavePlanet();

		// start main quests
		StartCoroutine(RunMainQuests());

		// final hints
		helperBox.Hide();
		yield return new WaitForSeconds(3f);
		helperBox.DisplayText(HelperBoxText.SpaceMovement);
		yield return new WaitForSeconds(15f);
		helperBox.Hide();
	}

	private IEnumerator RunMainQuests()
	{
		// player is free now
		SetPlayerInteractive(true);
		SetPlayerCanTakeoff(true);
		fuelBar.gameObject.SetActive(true);
		helperBox.Hide();
		canAutoManageHelperBox = true;

		yield break;
	}

	private void OnCommonPlayerProximityEnter(PlayerController player, GameObject obj)
	{
		Item item = obj.GetComponent<Item>();
		if (item != null)
		{
			// keep track of items for hintbox
			proximityItems.Add(item);
			
			Instrument instrument = obj.GetComponent<Instrument>();
			if (instrument != null)
			{
				InstrumentState state = instrumentStates[instrument.Kind];
				if (!state.IsReturnedToOwner)
				{
					// dialog hint (first time)
					if (state.IsHintDisplayable)
					{
						player.speaker.Speak($"hey! it's {(state.Kind != InstrumentKind.Sticks ? "a " : "")}{state.Kind.ToString().ToLower()}!");

						state.IsHintDisplayable = false;
					}
				}
			}
		}

		PlanetDude dude = obj.GetComponent<PlanetDude>();
		if (dude != null)
		{
			// keep track of character for hintbox
			proximityMusicians.Add(dude);
			
			InstrumentState state = instrumentStates[dude.DesiredInstrumentKind];
			StartCoroutine(RunFirstEncounter(state));
		}

		UpdateGameHints();
	}

	private void OnCommonPlayerProximityLeave(PlayerController player, GameObject obj)
	{
		Item item = obj.GetComponent<Item>();
		if (item != null)
		{
			// stops tracking for hint box
			proximityItems.Remove(item);
		}

		PlanetDude dude = obj.GetComponent<PlanetDude>();
		if (dude != null)
		{
			// stops tracking for hint box
			proximityMusicians.Remove(dude);
		}

		UpdateGameHints();
	}

	private void UpdateGameHints()
	{
		if (canAutoManageHelperBox)
		{
			if (!player.inventory.IsFree && proximityMusicians.Count > 0)
			{
				helperBox.DisplayText(HelperBoxText.GiveItem);
			}
			else if (player.inventory.IsFree && proximityItems.Count > 0)
			{
				helperBox.DisplayText(HelperBoxText.CatchItem);
			}
			else
			{
				helperBox.Hide();
			} 
		}
	}

	private IEnumerator RunFirstEncounter(InstrumentState state)
	{
		if (state.Kind != InstrumentKind.Microphone && !state.IsIntroduced) // not in tutorial
		{
			state.IsIntroduced = true;

			Instrument currentInstrument = player.inventory.CurrentInstrument;

			if (currentInstrument == null || currentInstrument.Kind != state.Kind)
			{
				if (state.Kind == InstrumentKind.Harmonica)
				{
					yield return StartCoroutine(RunDialog(
						(state.Owner.speaker, "Astrodog!", 2f),
						(state.Owner.speaker, "I need a helping paw.", 3f),
						(null, null, 1f),
						(player.speaker, "Sure!", 1f),
						(null, null, 0.5f),
						(state.Owner.speaker, "My harmonica is lost!", 4f),
						(state.Owner.speaker, "Can you help me find it?", 3f)));
				}
				else if (state.Kind == InstrumentKind.Ukulele)
				{
					yield return StartCoroutine(RunDialog(
						(state.Owner.speaker, "Rrrititititi tititi", 2f),
						(null, null, 1f),
						(player.speaker, "What did the fox say?", 2f),
						(null, null, 0.5f),
						(state.Owner.speaker, "Sorry!", 1f),
						(null, null, 0.5f),
						(state.Owner.speaker, "Without my ukulele it's hard to jam", 4f),
						(state.Owner.speaker, "Can you find it for me?", 3f)));
				}
				else if (state.Kind == InstrumentKind.Sticks)
				{
					yield return StartCoroutine(RunDialog(
						(state.Owner.speaker, "Something is drumbling me...", 4f),
						(null, null, 1f),
						(player.speaker, "What's wrong?", 2f),
						(null, null, 0.5f),
						(state.Owner.speaker, "Cogito ergo drum!", 2f),
						(null, null, 0.5f),
						(state.Owner.speaker, "If I don't find my sticks soon...", 4f),
						(state.Owner.speaker, "I'll be six beats under!", 3f)));
				}
			}
			else // if (currentInstrument.Kind == state.Kind)
			{
				string that = state.Kind == InstrumentKind.Sticks ? "These are" : "That is";
				string it = state.Kind == InstrumentKind.Sticks ? "them" : "it";
				yield return StartCoroutine(RunDialog(
					(state.Owner.speaker, "Hey Astrodog!", 2f),
					(state.Owner.speaker, $"{that} my {state.Kind.ToString().ToLower()}", 3f),
					(state.Owner.speaker, $"You found {it}!", 2f)));
			}
			

			//yield return StartCoroutine(RunDialog(state.Owner.speaker,
			//	("hey", 1f),
			//	("where are we?", 2f),
			//	("hey!", 1f),
			//	($"where is my {state.Kind.ToString().ToLower()}?", 3f)));
		}
	}

	private IEnumerator RunDialogForItemReturned(InstrumentKind instrument, PlanetDude owner)
	{
		if (instrument != InstrumentKind.Microphone) // not in tutorial
		{
			if (instrument == InstrumentKind.Harmonica)
			{
				yield return StartCoroutine(RunDialog(
					(owner.speaker, "Thanks Astrodog!", 2f)));
			}
			else if (instrument == InstrumentKind.Ukulele)
			{
				yield return StartCoroutine(RunDialog(
					(owner.speaker, "Rrrititi titi!", 2f),
					(owner.speaker, "That's my jam!", 2f)));
			}
			else if (instrument == InstrumentKind.Sticks)
			{
				yield return StartCoroutine(RunDialog(
					(owner.speaker, "You're unbeatable, Astrodog!", 2f),
					(null, null, 0.5f),
					(owner.speaker, "Now back to work!", 2f)));
			}
		}
	}

	private IEnumerator RunDialogForItemDenied(InstrumentKind instrument, PlanetDude character)
	{
		if (instrument != InstrumentKind.Microphone) // not in tutorial
		{
			if (character.DesiredInstrumentKind == InstrumentKind.Harmonica)
			{
				yield return StartCoroutine(RunDialog(
					(character.speaker, "That's not my harmonica...", 4f)));
			}
			else if (character.DesiredInstrumentKind == InstrumentKind.Ukulele)
			{
				yield return StartCoroutine(RunDialog(
					(character.speaker, "No!", 1f),
					(character.speaker, "My ukulele is blue!", 3.25f),
					(character.speaker, "Da ba dee da ba dae.", 2f)));
			}
			else if (character.DesiredInstrumentKind == InstrumentKind.Sticks)
			{
				yield return StartCoroutine(RunDialog(
					(character.speaker, "Beats me what you have there...", 4f)));
			}
		}
	}

	private IEnumerator RunFinale()
	{
		// fade
		ScreenFader.Instance.Fade(Color.clear, Color.black, 0.6f);
		yield return new WaitForSeconds(1.0f);

		// set banner text
		concertBannerText.text = "CONGRATULATIONS!";
		concertBannerText.GetComponent<TMPro.Examples.WarpTextExample>().Restart();

		// cam teleport
		GameObject.FindObjectOfType<CameraController>().transform.position = new Vector3(
			concertPlanet.transform.position.x,
			concertPlanet.transform.position.y,
			GameObject.FindObjectOfType<CameraController>().transform.position.z
			
		);

		// everybody on the concert planet

		// remove event handlers
		player.ProximityEnter -= OnCommonPlayerProximityEnter;
		player.ProximityLeave -= OnCommonPlayerProximityLeave;

		concertPlanet.transform.rotation = Quaternion.identity;



		// player is now non interactive
		SetPlayerInteractive(false);
		SetPlayerCanTakeoff(false);

		// teleport everyone to their final destinations.
		var teleporters = FindObjectsOfType<Teleporter>()
			.Where(t => t.sourceObject.gameObject == player.gameObject
			|| instrumentStates.Values.Any(inst => inst.Owner.gameObject == t.sourceObject.gameObject));
		foreach (var teleporter in teleporters)
		{
			Debug.Log(teleporter);
			teleporter.TeleportAtChild();
		}
		foreach(Instrument inst in GameObject.FindObjectsOfType<Instrument>())
		{
			inst.GetComponent<AudioSource>().enabled = false;
		}

		// teleport instruments to their owners.

		yield return new WaitForSeconds(0.5f);
		ScreenFader.Instance.Fade(Color.black, Color.white, 0.33f);
		yield return new WaitForSeconds(0.33f);
		ScreenFader.Instance.Fade(Color.white, Color.clear, 1.0f);
		float timer = 0f;
		float corrsfadeduration = 2f;
		while(timer<corrsfadeduration)
		{
			timer += Time.deltaTime;
			yield return null;
			endingSong.volume = timer / corrsfadeduration;
		}
		yield return new WaitForSeconds(10f);
		ScreenFader.Instance.Fade(Color.clear, Color.black, 3f);
		timer = 0f;
		corrsfadeduration = 4f;
		while(timer<corrsfadeduration)
		{
			timer += Time.deltaTime;
			yield return null;
			endingSong.volume = 1f - timer / corrsfadeduration;
		}
		
		yield return new WaitForSeconds(1f);
		UnityEngine.SceneManagement.SceneManager.LoadScene("_StartMenu");
	}

	private void Update()
	{
		if (Application.isEditor && Input.GetKeyDown(KeyCode.RightControl))
		{
			StartCoroutine(RunFinale());
		}
	}

	public void OnItemReturnedToOwner(InstrumentKind instrument, PlanetDude owner)
	{
		StartCoroutine(RunItemReturned(instrumentStates[instrument]));
	}

	private IEnumerator RunItemReturned(InstrumentState state)
	{
		// updates state
		state.IsReturnedToOwner = true;

		// dialog
		yield return StartCoroutine(RunDialogForItemReturned(state.Kind, state.Owner));

		int remaining = instrumentStates.Values.Count(s => !s.IsReturnedToOwner);
		Debug.Log($"Instrument {state.Kind} was returned to owner, {remaining} remaining.");

		// trigger game end if we are done
		if (remaining == 0)
		{
			StartCoroutine(RunFinale());
		}
	}

	public void OnItemDeniedByCharacter(InstrumentKind kind, PlanetDude character)
	{
		// dialog
		StartCoroutine(RunDialogForItemDenied(kind, character));
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
