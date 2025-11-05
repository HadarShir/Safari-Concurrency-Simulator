using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace SafariGUIFinal
{
    // Main form for the Safari simulation GUI. Handles all UI, simulation logic, and animal management.
    public partial class MainForm : Form
    {
        private List<PictureBox> lakesVisual = new List<PictureBox>(); // Visual representations of the lakes
        private List<Lake> lakes = new List<Lake>(); // Logical lake objects
        private Dictionary<Lake, Point[]> lakeSlotPositions = new Dictionary<Lake, Point[]>(); // Slot positions for each lake
        private Random rnd; // Random number generator
        private System.Windows.Forms.Timer refreshTimer; // Timer for refreshing the animal display
        private System.Windows.Forms.Timer simulationTimer; // Timer for simulation duration
        private List<AnimalThread> allAnimals = new List<AnimalThread>(); // All animals currently in the simulation
        private readonly string imageDir = @"C:\Users\User\source\repos\SafariGUIFinal\SafariGUIFinal\Image"; // Directory for images
        private bool isSimulationRunning = false; // True if the simulation is running
        private DateTime simulationEndTime; // When the simulation should end
        private Button btnStartSimulation; // starts the simulation

        // Initializes the main form, lakes, and UI
        public MainForm()
        {
            InitializeComponent();

            // Create logical lakes
            lakes.Clear();
            lakes.Add(new Lake("Lake A", 5));   // Small
            lakes.Add(new Lake("Lake B", 7));   // Medium
            lakes.Add(new Lake("Lake C", 10));  // Large

            rnd = new Random();

            //  Start the timer for refreshing the animal display
            refreshTimer = new System.Windows.Forms.Timer { Interval = 10 };
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();

            // 3) Add the simulation start button
            btnStartSimulation = new Button
            {
                Text = "START",
                Location = new Point(10, 10),
                Size = new Size(350, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                Font = new Font("Arial", 16, FontStyle.Bold)
            };
            btnStartSimulation.FlatAppearance.BorderSize = 0;
            btnStartSimulation.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnStartSimulation.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnStartSimulation.Click += BtnStartSimulation_Click;
            Controls.Add(btnStartSimulation);
        }

        // Handles the click event for the simulation start button
        private void BtnStartSimulation_Click(object sender, EventArgs e)
        {
            if (!isSimulationRunning)
            {
                StartSimulation();
            }
        }

        // Starts the simulation, spawns animals, and sets up the simulation timer
        private void StartSimulation()
        {
            isSimulationRunning = true;
            simulationEndTime = DateTime.Now.AddSeconds(20);
            btnStartSimulation.Text = "Welcome To The Safari";

            // Start all animal spawners
            StartAnimalSpawner<Hippo>(7.0);     // Hippos every ~7 seconds
            StartAnimalSpawner<Flamingo>(2.0);  // Flamingos every ~2 seconds
            StartAnimalSpawner<Zebra>(3.0);     // Zebras every ~3 seconds

            // Timer to check for simulation end
            simulationTimer = new System.Windows.Forms.Timer { Interval = 10 };
            simulationTimer.Tick += (s, e) =>
            {
                if (DateTime.Now >= simulationEndTime)
                {
                    simulationTimer.Stop();
                    simulationTimer.Dispose();
                    isSimulationRunning = false;
                    btnStartSimulation.Text = "START";
                    btnStartSimulation.Enabled = true;
                }
            };
            simulationTimer.Start();
        }

        // Spawns animals of a given type at random intervals
        private void StartAnimalSpawner<T>(double meanInterval) where T : AnimalThread
        {
            new Thread(() =>
            {
                while (isSimulationRunning)
                {
                    // Wait a random interval before spawning
                    double delay = meanInterval * (0.5 + rnd.NextDouble());
                    Thread.Sleep((int)(delay * 1000));

                    if (!isSimulationRunning) break;

                    // Pick a random lake
                    Lake randomLake = lakes[rnd.Next(lakes.Count)];

                    // Create a new animal
                    AnimalThread animal;
                    if (typeof(T) == typeof(Flamingo))
                        animal = new Flamingo($"Flamingo_{DateTime.Now.Ticks}", randomLake);
                    else if (typeof(T) == typeof(Zebra))
                        animal = new Zebra($"Zebra_{rnd.Next(1000, 9999)}", randomLake);
                    else if (typeof(T) == typeof(Hippo))
                        animal = new Hippo($"Hippo_{rnd.Next(1000, 9999)}", randomLake);
                    else
                        continue;

                    // Add to the animal list and start its thread
                    allAnimals.Add(animal);
                    animal.Start();
                }
            })
            { IsBackground = true }.Start();
        }

        // Handles form load: sets up the background and lake visuals
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Set the savanna background
            this.BackgroundImage = Image.FromFile(Path.Combine(imageDir, "background.png"));
            this.BackgroundImageLayout = ImageLayout.Stretch;

            // Set up lake positions and sizes
            var lakeCenters = new[]
            {
                new Point(345, 750),    // Lake A
                new Point(1235, 825),   // Lake B
                new Point(950, 520)     // Lake C
            };
            var lakeSizes = new[]
            {
                new Size(506, 350),
                new Size(878, 500),
                new Size(1400, 400)
            };
            // Different vertical radius for each lake
            var radiusYs = new[] { 70, 100, 75 };

            // Create a PictureBox for each lake, cut to ellipse, and store in lakesVisual
            for (int i = 0; i < lakes.Count; i++)
            {
                // Load PNG, make white transparent
                var img = Image.FromFile(Path.Combine(imageDir, "lake.png"));
                var bmp = new Bitmap(img);
                bmp.MakeTransparent(Color.White);
                img.Dispose();

                var pic = new PictureBox
                {
                    Image = bmp,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Width = lakeSizes[i].Width,
                    Height = lakeSizes[i].Height,
                    Left = lakeCenters[i].X - lakeSizes[i].Width / 2,
                    Top = lakeCenters[i].Y - lakeSizes[i].Height / 2,
                    BackColor = Color.Transparent
                };
                // Cut to ellipse
                using (var gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, pic.Width, pic.Height);
                    pic.Region = new Region(gp);
                }

                Controls.Add(pic);
                pic.SendToBack();
                lakesVisual.Add(pic);

                // Calculate slot positions (just positions, no PictureBox)
                int slotCount = lakes[i].SlotCount();
                Point[] positions = new Point[slotCount];
                double angleStep = 2 * Math.PI / slotCount;
                int radiusX = pic.Width / 2 - 97;
                int radiusY = radiusYs[i];
                int animalSize = 90;

                for (int j = 0; j < slotCount; j++)
                {
                    double angle = j * angleStep;
                    int cx = pic.Left + pic.Width / 2;
                    int cy = pic.Top + pic.Height / 2 - 35;
                    int x = cx + (int)(radiusX * Math.Cos(angle)) - animalSize / 2;
                    int y = cy + (int)(radiusY * Math.Sin(angle)) - animalSize / 2;
                    positions[j] = new Point(x, y);
                }
                lakeSlotPositions[lakes[i]] = positions;
            }
        }

        // Refreshes the animal display for all lakes
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // 1. Remove all previous animals from each lake
            foreach (var lakePic in lakesVisual)
            {
                var animalsToRemove = new List<PictureBox>();
                foreach (Control ctrl in lakePic.Controls)
                {
                    if (ctrl is PictureBox pb && pb.Tag as string == "animal")
                        animalsToRemove.Add(pb);
                }
                foreach (var pb in animalsToRemove)
                {
                    pb.Image?.Dispose();
                    this.Controls.Remove(pb);
                    pb.Dispose();
                }
            }

            // Remove finished animals from the list
            allAnimals.RemoveAll(a => a.IsDone);

            // Add animals currently in each lake
            for (int lakeIdx = 0; lakeIdx < lakes.Count; lakeIdx++)
            {
                var lake = lakes[lakeIdx];
                var lakePic = lakesVisual[lakeIdx];
                var slots = lakeSlotPositions[lake];
                var animals = allAnimals
                    .Where(a => !a.IsWaitingOutside && a.Lake == lake)
                    .ToList();

                foreach (var a in animals)
                {
                    if (a is Hippo)
                    {
                        AddHippoInLake(a, lakePic);
                    }
                    else if (a.CurrentSpot.HasValue)
                    {
                        var abs = slots[a.CurrentSpot.Value];
                        var rel = new Point(abs.X - lakePic.Left, abs.Y - lakePic.Top);
                        AddAnimalInLake(a, rel, lakePic);
                    }
                }

                // Ensure all animals are above everything else
                foreach (Control ctrl in lakePic.Controls)
                {
                    if (ctrl is PictureBox pb && pb.Tag as string == "animal")
                        pb.BringToFront();
                }
            }
        }

        // Adds a non-hippo animal to the lake's visual at the given location
        private void AddAnimalInLake(AnimalThread animal, Point location, PictureBox lakePic)
        {
            var bmp = new Bitmap(animal.ImagePath);
            bmp.MakeTransparent(Color.White);

            var pic = new PictureBox
            {
                Image = bmp,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(102, 102),
                BackColor = Color.Transparent,
                Tag = "animal",
                Location = location // location should be relative to lakePic
            };

            lakePic.Controls.Add(pic);
            pic.BringToFront();
        }

        // Adds a hippo to the center of the lake's visual
        private void AddHippoInLake(AnimalThread animal, PictureBox lakePic)
        {
            var bmp = new Bitmap(animal.ImagePath);
            bmp.MakeTransparent(Color.White);

            var pic = new PictureBox
            {
                Image = bmp,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(180, 120),
                BackColor = Color.Transparent,
                Tag = "animal"
            };
            // Center the hippo in the lakePic
            pic.Left = (lakePic.Width - pic.Width) / 2;
            pic.Top = (lakePic.Height - pic.Height) / 2;
            lakePic.Controls.Add(pic);
            pic.BringToFront();
        }

        // Handles form closing: cleans up timers and images
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            refreshTimer?.Stop();
            refreshTimer?.Dispose();

            // Clean up lake images
            foreach (var pic in lakesVisual)
            {
                pic.Image?.Dispose();
                pic.Dispose();
            }
            lakesVisual.Clear();
            allAnimals.Clear();
        }
    }
}
