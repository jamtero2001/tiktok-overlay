using System;
using System.Windows.Forms;
using System.Threading;

namespace LeagueTikTokOverlay
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            const int maxInstances = 5;
            Mutex? acquiredMutex = null;

            try
            {
                for (int i = 0; i < maxInstances; i++)
                {
                    var mutex = new Mutex(false, $"LeagueTikTokOverlayMutex_{i}");
                    bool lockTaken = false;

                    try
                    {
                        lockTaken = mutex.WaitOne(0, false);
                    }
                    catch (AbandonedMutexException)
                    {
                        lockTaken = true; // Take over abandoned mutex
                    }

                    if (lockTaken)
                    {
                        acquiredMutex = mutex;
                        break;
                    }

                    mutex.Dispose();
                }

                if (acquiredMutex == null)
                {
                    MessageBox.Show($"You already have {maxInstances} overlays running.",
                                  "Instance Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    var overlay = new LeagueOverlay();
                    Application.Run(overlay);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start overlay: {ex.Message}",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (acquiredMutex != null)
                {
                    try { acquiredMutex.ReleaseMutex(); } catch (ApplicationException) { }
                    acquiredMutex.Dispose();
                }
            }
        }
    }
}