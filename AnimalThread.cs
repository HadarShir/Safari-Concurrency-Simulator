using System;
using System.Threading;

namespace SafariGUIFinal
{
    // Abstract base class for all animal threads in the safari simulation. 
    public abstract class AnimalThread
    {
        public string Name { get; protected set; } // Name of the animal
        public Lake Lake { get; protected set; } // The lake this animal is associated with
        protected int UnitSize; // How many capacity units this animal occupies in the lake
        public double ArrivalTime { get; set; } // Arrival time in seconds (randomized per animal)
        protected double DrinkTime; // Drinking time in seconds (randomized per animal)
        protected Thread thread; // The thread running this animal's behavior
        public bool IsWaitingOutside { get; set; } = true; // True if the animal is waiting outside the lake
        public bool IsDone { get; set; } = false; // True if the animal has finished its activity
        public abstract string ImagePath { get; } // Path to the animal's image file
        public int? CurrentSpot { get; set; } // The current spot occupied by the animal (if any)

        // Initializes a new animal thread with the given parameters
        public AnimalThread(string name, Lake lake, int unitSize, double arrival, double drink)
        {
            Name = name;
            Lake = lake;
            UnitSize = unitSize;
            ArrivalTime = arrival;
            DrinkTime = drink;
        }

        // Starts the animal's thread
        public void Start()
        {
            thread = new Thread(Run) { IsBackground = true };
            thread.Start();
        }

        // Sleeps the current thread for the given number of seconds
        protected void Sleep(double seconds)
        {
            Thread.Sleep((int)(seconds * 1000));
        }

        // The main logic for the animal's behavior (to be implemented by subclasses)
        protected abstract void Run();

        // Gets the thread object for this animal
        public Thread Thread => thread;
    }
} 