using System;
using System.Threading;
using System.Collections.Generic;

namespace SafariGUIFinal
{
    // Represents a lake in the safari simulation. Handles animal entry, exit, and synchronization.
    public class Lake
    {
       
        public string Name { get; private set; } // Name of the lake
        private readonly int capacityUnits; // Total capacity units of the lake
        private readonly SemaphoreSlim semaphore; // Semaphore to control concurrent access to the lake
        private readonly object lakeLock = new object(); // Lock object for synchronizing animal entry/exit
        private readonly Mutex hippoMutex = new Mutex(); // Mutex for exclusive hippo access: ensures only one hippo can enter the lake at a time, and no other animals can enter while a hippo is present
        private int flamingoCount = 0; // Current number of flamingos in the lake
        private int zebraCount = 0; // Current number of zebras in the lake
        private bool hippoPresent = false; // True if a hippo is present in the lake
        private HashSet<int> occupiedSpots = new HashSet<int>(); // Set of occupied spots in the lake
        private List<Hippo> hippos = new List<Hippo>(); // List of hippos in the lake (for tracking)

        // Initializes a new instance of the Lake class.
        public Lake(string name, int totalUnits)
        {
            Name = name;
            capacityUnits = totalUnits;
            semaphore = new SemaphoreSlim(totalUnits);
        }

        // Returns a summary of animals currently in the lake
        public (int Flamingos, int Zebras, bool HippoPresent) GetAnimalSummary()
        {
            lock (lakeLock)
            {
                return (flamingoCount, zebraCount, hippoPresent);
            }
        }

        // Attempts to let a flamingo enter the lake at a given spot
        public bool TryEnterFlamingo(string name, int spot)
        {
            if (!semaphore.Wait(0)) return false;
            lock (lakeLock)
            {
                if (hippoPresent)
                {
                    semaphore.Release();
                    return false;
                }

                // Prevent entering an already occupied spot
                if (occupiedSpots.Contains(spot))
                {
                    semaphore.Release();
                    return false;
                }

                // If lake is empty, can enter any available spot
                if (flamingoCount == 0)
                {
                    if (spot >= 0 && spot < capacityUnits)
                    {
                        flamingoCount++;
                        occupiedSpots.Add(spot);
                        return true;
                    }
                    semaphore.Release();
                    return false;
                }

                // If lake has flamingos, must enter adjacent spot
                if (!IsAdjacentToExistingFlamingo(spot))
                {
                    semaphore.Release();
                    return false;
                }

                flamingoCount++;
                occupiedSpots.Add(spot);
                return true;
            }
        }

        private bool IsAdjacentToExistingFlamingo(int spot)
        {
            // Circular adjacency
            foreach (int occupiedSpot in occupiedSpots)
            {
                if ((occupiedSpot == (spot + 1) % capacityUnits) ||
                    (occupiedSpot == (spot - 1 + capacityUnits) % capacityUnits))
                {
                    return true;
                }
            }
            return false;
        }

        // Handles flamingo exit from the lake
        public void ExitFlamingo(int spot)
        {
            lock (lakeLock)
            {
                flamingoCount--;
                occupiedSpots.Remove(spot);
                if (flamingoCount == 0 && zebraCount == 0)
                {
                    Monitor.PulseAll(lakeLock);
                }
            }
            semaphore.Release();
        }

        // Attempts to let a zebra enter the lake at a given spot
        public bool TryEnterZebra(string name, int spot)
        {
            if (!semaphore.Wait(0)) return false;
            if (!semaphore.Wait(0)) // need two units
            {
                semaphore.Release();
                return false;
            }

            lock (lakeLock)
            {
                if (hippoPresent || zebraCount >= 5)
                {
                    semaphore.Release();
                    semaphore.Release();
                    return false;
                }
                if (occupiedSpots.Contains(spot) || occupiedSpots.Contains((spot + 1) % capacityUnits))
                {
                    semaphore.Release();
                    semaphore.Release();
                    return false;
                }
                zebraCount++;
                occupiedSpots.Add(spot);
                occupiedSpots.Add((spot + 1) % capacityUnits);
                return true;
            }
        }

        // Handles zebra exit from the lake
        public void ExitZebra(int spot)
        {
            lock (lakeLock)
            {
                zebraCount--;
                occupiedSpots.Remove(spot);
                occupiedSpots.Remove((spot + 1) % capacityUnits);
                if (flamingoCount == 0 && zebraCount == 0)
                {
                    Monitor.PulseAll(lakeLock);
                }
            }
            semaphore.Release(2);
        }

        // Handles hippo entry (exclusive access)
        public void EnterHippo()
        {
            hippoMutex.WaitOne();

            lock (lakeLock)
            {
                while (semaphore.CurrentCount < capacityUnits)
                {
                    Monitor.Wait(lakeLock);
                }
                hippoPresent = true;
                for (int i = 0; i < capacityUnits; i++)
                {
                    semaphore.Wait();
                }
            }
        }

        // Handles hippo exit (releases exclusive access)
        public void ExitHippo()
        {
            lock (lakeLock)
            {
                hippoPresent = false;
                semaphore.Release(capacityUnits);
                Monitor.PulseAll(lakeLock);
            }
            hippoMutex.ReleaseMutex();
        }

        // Returns a random available spot in the lake
        public int GetRandomAvailableSpot()
        {
            lock (lakeLock)
            {
                var available = new List<int>();
                for (int i = 0; i < capacityUnits; i++)
                {
                    if (!occupiedSpots.Contains(i))
                        available.Add(i);
                }
                if (available.Count == 0) return -1;
                return available[new Random().Next(available.Count)];
            }
        }

        // Returns the number of slots in the lake
        public int SlotCount() => capacityUnits;


        // Adds a hippo to the lake's tracking list
        public void AddHippo(Hippo hippo)
        {
            Console.WriteLine($"AddHippo: {hippo.Name} tries to enter lake {Name}");
            hippos.Add(hippo);
            Console.WriteLine($"AddHippo: {hippo.Name} entered lake {Name}");
        }

        // Removes a hippo from the lake's tracking list
        public void RemoveHippo(Hippo hippo)
        {
            Console.WriteLine($"RemoveHippo: {hippo.Name} leaves lake {Name}");
            hippos.Remove(hippo);
            Console.WriteLine($"RemoveHippo: {hippo.Name} left lake {Name}");
        }

        // Gets the mutex for hippo exclusive access
        public Mutex HippoMutex => hippoMutex;

        // Gets the list of hippos in the lake
        public List<Hippo> Hippos => hippos;

        // Returns a list of currently occupied spots
        public List<int> GetOccupiedSpots()
        {
            lock (lakeLock)
            {
                return new List<int>(occupiedSpots);
            }
        }

        // Returns a preferred spot for a flamingo (adjacent or random if empty)
        public int? GetPreferredFlamingoSpot()
        {
            lock (lakeLock)
            {
                var adjacentSpots = new List<int>();
                bool flamingoInLake = false;
                for (int i = 0; i < capacityUnits; i++)
                {
                    if (occupiedSpots.Contains(i))
                        continue;
                    int left = (i == 0) ? capacityUnits - 1 : i - 1;
                    int right = (i == capacityUnits - 1) ? 0 : i + 1;
                    if (occupiedSpots.Contains(left) || occupiedSpots.Contains(right))
                    {
                        flamingoInLake = true;
                        adjacentSpots.Add(i);
                    }
                }
                if (flamingoInLake && adjacentSpots.Count > 0)
                {
                    return adjacentSpots[new Random().Next(adjacentSpots.Count)];
                }
                if (!flamingoInLake)
                {
                    var available = new List<int>();
                    for (int i = 0; i < capacityUnits; i++)
                    {
                        if (!occupiedSpots.Contains(i))
                            available.Add(i);
                    }
                    if (available.Count == 0) return null;
                    return available[new Random().Next(available.Count)];
                }
                return null;
            }
        }

        // Returns a preferred spot for a zebra (pair of adjacent spots)
        public int? GetPreferredZebraSpot()
        {
            lock (lakeLock)
            {
                var availablePairs = new List<int>();
                for (int i = 0; i < capacityUnits; i++)
                {
                    int next = (i + 1) % capacityUnits;
                    if (!occupiedSpots.Contains(i) && !occupiedSpots.Contains(next))
                        availablePairs.Add(i);
                }
                if (availablePairs.Count == 0) return null;
                return availablePairs[new Random().Next(availablePairs.Count)];
            }
        }
    }
} 