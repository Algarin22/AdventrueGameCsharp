namespace AdventureGame;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Welcome to the Adventure Game!");
        Console.WriteLine("You find yourself in a mysterious room. What do you want to do?");

        // Create a new room
        Room r = new Room();

        // Game loop
        while (true)
        {
            Console.WriteLine("Enter a command (look, move, quit):");
            string command = Console.ReadLine();

            if (command == "look")
            {
                Console.WriteLine("You look around the room...");
                // Add logic to describe the room
            }
            else if (command == "move")
            {
                Console.WriteLine("Where do you want to move? (north, south, east, west)");
                string direction = Console.ReadLine();
                // Add logic to move in the specified direction
            }
            else if (command == "quit")
            {
                Console.WriteLine("Thanks for playing!");
                break;
            }
            else
            {
                Console.WriteLine("Invalid command. Please try again.");
            }
        }
    }
}

