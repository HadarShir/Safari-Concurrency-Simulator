using System;
using System.Threading;

namespace SafariGUIFinal
{
    // Represents a flamingo in the safari simulation. Inherits from AnimalThread.
    public class Flamingo : AnimalThread
    {
        private readonly Random random = new Random(); // Used for random spot selection (if needed)

        // Constructor for default arrival and drink times
        public Flamingo(string name, Lake lake)
            : base(name, lake, 1, 2.0, 3.5) { }

        // Constructor for custom arrival and drink times
        public Flamingo(string name, Lake lake, double arrival, double drink)
            : base(name, lake, 1, arrival, drink) { }

        // Returns the image path for the flamingo
        public override string ImagePath => "C:\\Users\\User\\source\\repos\\SafariGUIFinal\\SafariGUIFinal\\Image\\Flamengo.png";

        // Main logic for the flamingo's behavior
        protected override void Run()
        {
            Console.WriteLine($"{Name} is heading to lake {Lake.Name}");
            Sleep(ArrivalTime);

            for (int i = 0; i < 10; i++)
            {
                IsWaitingOutside = true;
                int? spot = Lake.GetPreferredFlamingoSpot();
                if (spot == null)
                {
                    Sleep(0.2);
                    continue;
                }
                Console.WriteLine($"{Name} tries to enter spot {spot.Value}. Occupied: [{string.Join(",", Lake.GetOccupiedSpots())}]");
                if (Lake.TryEnterFlamingo(Name, spot.Value))
                {
                    CurrentSpot = spot.Value;
                    IsWaitingOutside = false;
                    Console.WriteLine($"{Name} entered lake {Lake.Name} at spot {spot.Value} to drink");
                    Sleep(DrinkTime);
                    Lake.ExitFlamingo(spot.Value);
                    Console.WriteLine($"{Name} left lake {Lake.Name} from spot {spot.Value}");
                    CurrentSpot = null;
                    IsDone = true;
                    return;
                }
                Sleep(0.2);
            }

            Console.WriteLine($"{Name} gave up trying to enter lake {Lake.Name}");
            IsWaitingOutside = false;
            IsDone = true;
        }
        
    }
} 