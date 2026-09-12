using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace HW
{
    public partial class Form1 : Form
    {
        public SynchronizationContext context;

        int finishedHorses = 0;

        string[] horseNames =
        {
            "Lightning",
            "Thunder",
            "Storm",
            "Speedy",
            "Flash"
        };

        public Form1()
        {
            InitializeComponent();
            context = SynchronizationContext.Current;
        }

        public int SpeedOfhorse()
        {
            Random random = new Random();
            return random.Next(1, 11);
        }

        public void Run(
            object state,
            List<(string Name, TimeSpan Time)> durations,
            int horseIndex)
        {
            ProgressBar bar = (ProgressBar)state;

            context.Send(d => bar.Minimum = 0, null);
            context.Send(d => bar.Maximum = 230, null);
            context.Send(d => bar.Value = 0, null);

            DateTime timeFirst = DateTime.Now;

            int speed = SpeedOfhorse();

            for (int i = 0; i < 230; i += speed)
            {
                Thread.Sleep(50);

                int value = Math.Min(i, 230);

                context.Send(d => bar.Value = value, null);
            }

            context.Send(d => bar.Value = 230, null);

            DateTime timeSecond = DateTime.Now;

            TimeSpan duration = timeSecond - timeFirst;

            lock (durations)
            {
                durations.Add((horseNames[horseIndex], duration));
            }

            int finished = Interlocked.Increment(ref finishedHorses);

            if (finished == 5)
            {
                GetWinner(durations);
            }
        }

        public void GetWinner(List<(string Name, TimeSpan Time)> durations)
        {
            List<(string Name, TimeSpan Time)> top =
                durations.OrderBy(x => x.Time).ToList();

            context.Send(d =>
            {
                label12.Text =
                    $"1. {top[0].Name} — {top[0].Time.TotalSeconds:F2} сек";

                label13.Text =
                    $"2. {top[1].Name} — {top[1].Time.TotalSeconds:F2} сек";

                label14.Text =
                    $"3. {top[2].Name} — {top[2].Time.TotalSeconds:F2} сек";

                label15.Text =
                    $"4. {top[3].Name} — {top[3].Time.TotalSeconds:F2} сек";

                label16.Text =
                    $"5. {top[4].Name} — {top[4].Time.TotalSeconds:F2} сек";

                button1.Enabled = true;

            }, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread[] threads = new Thread[5];

            ProgressBar[] bars =
            {
                progressBar1,
                progressBar2,
                progressBar3,
                progressBar4,
                progressBar5
            };

            List<(string Name, TimeSpan Time)> durations =
                new List<(string Name, TimeSpan Time)>();

            Interlocked.Exchange(ref finishedHorses, 0);

            label12.Text = "";
            label13.Text = "";
            label14.Text = "";
            label15.Text = "";
            label16.Text = "";

            button1.Enabled = false;

            for (int i = 0; i < 5; i++)
            {
                int index = i;

                threads[i] = new Thread(() =>
                    Run(bars[index], durations, index));

                threads[i].Start();
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
        }
    }
}