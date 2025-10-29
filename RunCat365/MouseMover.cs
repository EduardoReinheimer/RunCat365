using System.Runtime.InteropServices;

namespace RunCat365
{
    public class MouseMover : NativeWindow, IDisposable
    {
        // Mouse API
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        // Hotkey API
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Aggiungi questa API per prevenire sleep/screen lock
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // Aggiungi questo enum
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        // Costanti hotkey
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_NOREPEAT = 0x4000;
        private const uint VK_H = 0x48;

        private readonly int _intervalloSecondi;
        private readonly int _intervalloMs;
        private bool _isRunning;
        private Thread _workerThread;

        public bool IsRunning => _isRunning;

        public MouseMover(int intervalloSecondi = 4)
        {
            _intervalloSecondi = intervalloSecondi;
            _intervalloMs = _intervalloSecondi * 1000;
            _isRunning = false;

            CreateHandle(new CreateParams());
            RegisterHotKey(Handle, HOTKEY_ID,
                MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, VK_H);
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;

            // Previeni sleep e screen lock
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS |
                                   EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                                   EXECUTION_STATE.ES_SYSTEM_REQUIRED);

            _workerThread = new Thread(ExecuteLoop)
            {
                IsBackground = true
            };
            _workerThread.Start();
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _workerThread?.Join(5000);

            // Ripristina comportamento normale del sistema
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                Stop();
            }
            base.WndProc(ref m);
        }

        private void ExecuteLoop()
        {
            while (_isRunning)
            {
                try
                {
                    MuoviMouse();
                    Thread.Sleep(_intervalloMs);
                }
                catch { }
            }
        }

        private void MuoviMouse()
        {
            GetCursorPos(out POINT currentPos);
            SetCursorPos(currentPos.X + 1, currentPos.Y + 1);
            Thread.Sleep(50);
            SetCursorPos(currentPos.X, currentPos.Y);
        }

        public void Dispose()
        {
            Stop();
            UnregisterHotKey(Handle, HOTKEY_ID);
            DestroyHandle();
        }
    }
}
