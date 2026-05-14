namespace AdventureGame;

    public class Room
    {
        private bool isLit;
        private bool hasLamp;
        private bool hasKey;
        private bool hasChest;


        private bool hasNorth;
        private bool hasSouth;
        private bool hasEast;
        private bool hasWest;


        private string description;

        public Room()
        {
            
        }

        public bool IsList()
        {
            return isLit;
        }

        public bool HasLamp()
        {
            return hasLamp;
        }

        public bool HasKey()
        {
            return hasKey;
        }

        public bool HasChest()
        {
            return hasChest;
        }

        public bool HasNorth()
        {
            return hasNorth;
        }

        public bool HasSouth()
        {
            return hasSouth;
        }

        public bool HasEast()
        {
            return hasEast;
        }

        public bool HasWest()
        {
            return hasWest;
        }

        public string GetDescription()
        {
            return description;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
