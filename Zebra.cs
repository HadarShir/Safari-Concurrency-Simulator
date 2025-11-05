using System;
using System.Threading;

namespace SafariGUIFinal
{
    // Represents a zebra in the safari simulation. Inherits from AnimalThread.
    public class Zebra : AnimalThread
    {
        // Constructor for default arrival and drink times
        public Zebra(string name, Lake lake)
            : base(name, lake, 2, 3.0, 5.0) { }

        // Returns the image path for the zebra
        public override string ImagePath => "C:\\Users\\User\\source\\repos\\SafariGUIFinal\\SafariGUIFinal\\Image\\Zebra.png";

        // Main logic for the zebra's behavior with DEBUG prints
        protected override void Run()
        {
            Console.WriteLine($"{Name} is heading to lake {Lake.Name}");
            Sleep(ArrivalTime);

            for (int i = 0; i < 10; i++)
            {
                IsWaitingOutside = true;
                int? spot = Lake.GetPreferredZebraSpot();
                if (spot == null)
                {
                    Sleep(0.3);
                    continue;
                }
                if (Lake.TryEnterZebra(Name, spot.Value))
                {
                    CurrentSpot = spot.Value;
                    IsWaitingOutside = false;
                    Console.WriteLine($"{Name} entered lake {Lake.Name} at spots {spot.Value} and {(spot.Value+1)%Lake.SlotCount()} to drink");
                    Sleep(DrinkTime);
                    Lake.ExitZebra(spot.Value);
                    Console.WriteLine($"{Name} left lake {Lake.Name}");
                    CurrentSpot = null;
                    IsDone = true;
                    return;
                }
                Sleep(0.3);
            }
            Console.WriteLine($"{Name} gave up trying to enter lake {Lake.Name}");
            IsWaitingOutside = false;
            IsDone = true;
        }
    }
} 