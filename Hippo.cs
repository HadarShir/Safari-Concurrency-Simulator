using System;
using System.Threading;
using System.Threading.Tasks;

namespace SafariGUIFinal
{
    // Represents a hippopotamus in the safari simulation. Inherits from AnimalThread.
    public class Hippo : AnimalThread
    {
        // Constructor for default arrival and drink times
        public Hippo(string name, Lake lake)
            : base(name, lake, 999, 10.0, 5.0) { }

        // Returns the image path for the hippo
        public override string ImagePath => "C:\\Users\\User\\source\\repos\\SafariGUIFinal\\SafariGUIFinal\\Image\\Hippo.png";

        // Main logic for the hippo's behavior with DEBUG prints
        protected override void Run()
        {
            Console.WriteLine($"{Name} is heading to lake {Lake.Name}");
            Sleep(ArrivalTime);
            IsWaitingOutside = true;
            Console.WriteLine($"{Name} is waiting for exclusive access to lake {Lake.Name}");
            Lake.EnterHippo();
            CurrentSpot = null;
            IsWaitingOutside = false;
            Console.WriteLine($"{Name} entered lake {Lake.Name} EXCLUSIVELY to drink");
            Sleep(DrinkTime);
            Lake.ExitHippo();
            CurrentSpot = null;
            Console.WriteLine($"{Name} left lake {Lake.Name}");
            IsDone = true;
        }


    }
} 