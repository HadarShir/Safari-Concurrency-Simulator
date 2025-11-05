# Safari Concurrency Simulator (C#)

## Overview
Academic project simulating a wildlife safari with **three lakes** and multiple animal species that drink water under **concurrency constraints**.  
The system demonstrates **thread synchronization**, **deadlock prevention**, and **fairness** between competing threads.  
Developed in **C# (.NET)** with a **WinForms GUI** showing real-time activity of animals in the lakes.

---

## 🎯 Project Goals
- Model concurrent access to **shared resources** (the lakes).  
- Define different behavioral rules for each animal species.  
- Prevent **deadlocks** using proper synchronization logic.  
- Ensure **fairness** among threads while avoiding starvation.  
- Visualize concurrency behavior via a simple, interactive interface.

---

## 🦓 Core Simulation Rules
- **Hippos** – require **exclusive access** to a lake (no other animals can drink simultaneously).  
- **Flamingos** – can share a lake with others but must avoid overcrowding or adjacency conflicts.  
- **Zebras** – limited number allowed per lake (capacity restriction).  
- **Global rules:**  
  - No circular waits → deadlock prevention policy.  
  - Fairness queue ensures no species monopolizes the lakes.  
  - Thread timeout/retry logic ensures progress under contention.

---

## 🧩 System Architecture
- **AnimalThread.cs** – manages each animal’s life cycle as an independent thread (approach → drink → leave).  
- **Lake.cs** – central shared resource implementing synchronization and capacity control.  
- **Flamingo.cs, Zebra.cs, Hippo.cs** – define species-specific behavior and constraints.  
- **MainForm.cs** – graphical interface (WinForms) displaying lake occupancy and animal movement in real time.  
- **Program.cs** – main entry point of the application.  
- **App.config** – configuration file controlling simulation parameters such as timing and lake capacities.

---

## ⚙️ Concurrency Techniques
- **Synchronization primitives:** `lock`, `Monitor`, `SemaphoreSlim`.  
- **Deadlock prevention:** resource ordering and timeout-based reentry logic.  
- **Starvation avoidance:** fair scheduling of threads and retry mechanisms.  
- **Thread model:** `Thread` or `Task` per animal with safe cancellation and UI updates via Invoke.

---

## 🖥 Running the Simulation
1. Open the solution in **Visual Studio**.  
2. Set `SafariGUIFinal` as the startup project.  
3. Configure runtime parameters in **App.config** (lake capacity, delay times, etc.).  
4. Run — the GUI will display animals entering and leaving the lakes according to synchronization rules.

---

## 🔬 Example Experiments
- **Stress test:** spawn many animals → verify no deadlocks occur.  
- **Fairness test:** ensure all species access the lakes over time.  
- **Performance test:** observe responsiveness under different lake capacities.

---

## 🧠 Technologies
- **Language:** C# (.NET Framework / .NET 6+)  
- **UI Framework:** WinForms  
- **Testing:** NUnit / MSTest  
- **IDE:** Visual Studio / Rider  

---

## 👩‍💻 Author
Developed by **Hadar Shir**  
B.Sc. Information Systems and Software Engineering  
Ben-Gurion University of the Negev  

---

## 📚 Notes
This project demonstrates practical application of **concurrency control**, **deadlock avoidance**, and **real-time system simulation** in C#.  
It emphasizes clean architecture, thread safety, and event-driven GUI updates for live visualization.
