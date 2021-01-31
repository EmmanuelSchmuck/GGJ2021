
public class InstrumentState
{
	public InstrumentKind Kind { get; set; }
	public bool IsReturnedToOwner { get; set; }
	public PlanetDude Owner { get; set; }
	public Instrument Object { get; set; }
	public bool IsHintDisplayable { get; set; } = true;
	public bool IsIntroduced { get; set; }
}