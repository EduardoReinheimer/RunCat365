using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RunCat365
{
    public sealed class TeamsStatusKeeper
    {
        private static readonly Lazy<TeamsStatusKeeper> _instance =
            new Lazy<TeamsStatusKeeper>(() => new TeamsStatusKeeper());

        public static TeamsStatusKeeper Instance => _instance.Value;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const byte VK_NUMLOCK = 0x90;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x1;
        private const uint KEYEVENTF_KEYUP = 0x2;

        private Thread _workerThread;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private readonly object _lock = new object();

        // Costruttore privato per Singleton
        private TeamsStatusKeeper() { }

        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    Console.WriteLine("TeamsStatusKeeper è già in esecuzione.");
                    return;
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _isRunning = true;

                _workerThread = new Thread(() => WorkerLoop(_cancellationTokenSource.Token))
                {
                    IsBackground = true,
                    Name = "TeamsStatusKeeperWorker"
                };

                _workerThread.Start();
                Console.WriteLine("TeamsStatusKeeper avviato.");
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning)
                {
                    Console.WriteLine("TeamsStatusKeeper non è in esecuzione.");
                    return;
                }

                _cancellationTokenSource?.Cancel();
                _workerThread?.Join(TimeSpan.FromSeconds(5));

                _isRunning = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                Console.WriteLine("TeamsStatusKeeper arrestato.");
            }
        }

        public bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return _isRunning;
                }
            }
        }

        private void WorkerLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Simula pressione NumLock due volte
                    PressNumLock();
                    Thread.Sleep(100);
                    PressNumLock();

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Attività simulata - Teams online");

                    // Attendi 60 secondi o fino alla cancellazione
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(60));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore nel worker loop: {ex.Message}");

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                    }
                }
            }
        }

        private static void PressNumLock()
        {
            keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
    }
}
