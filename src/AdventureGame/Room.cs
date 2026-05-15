namespace AdventureGame;

public class Room
{
    private bool isLit;
    private bool hasLamp;
    private bool hasKey;
    private bool hasChest;
    private bool isExit;      // ← NEW: marks the escape room

    private bool hasNorth;
    private bool hasSouth;
    private bool hasEast;
    private bool hasWest;

    private string description;

    public Room()
    {
        SetLit(false);
        SetLamp(false);
        SetKey(false);
        SetChest(false);
        SetExit(false);
        SetNorth(false);
        SetSouth(false);
        SetEast(false);
        SetWest(false);
        SetDescription(string.Empty);
    }

    public bool IsLit()      => isLit;
    public bool HasLamp()    => hasLamp;
    public bool HasKey()     => hasKey;
    public bool HasChest()   => hasChest;
    public bool IsExit()     => isExit;
    public bool HasNorth()   => hasNorth;
    public bool HasSouth()   => hasSouth;
    public bool HasEast()    => hasEast;
    public bool HasWest()    => hasWest;
    public string GetDescription() => description;

    public void SetLit(bool b)         => isLit = b;
    public void SetLamp(bool b)        => hasLamp = b;
    public void SetKey(bool b)         => hasKey = b;
    public void SetChest(bool b)       => hasChest = b;
    public void SetExit(bool b)        => isExit = b;
    public void SetNorth(bool b)       => hasNorth = b;
    public void SetSouth(bool b)       => hasSouth = b;
    public void SetEast(bool b)        => hasEast = b;
    public void SetWest(bool b)        => hasWest = b;
    public void SetDescription(string d) => description = d;

    public override string ToString() =>
        $"Room[lit={isLit}, lamp={hasLamp}, key={hasKey}, chest={hasChest}, " +
        $"exit={isExit}, N={hasNorth}, S={hasSouth}, E={hasEast}, W={hasWest}, desc={description}]";
}