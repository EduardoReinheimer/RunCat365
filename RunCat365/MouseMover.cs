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

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // Costanti hotkey
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_NOREPEAT = 0x4000;
        private const uint VK_H = 0x48;

        private readonly int _intervalloMinuti;
        private readonly int _intervalloMs;
        private bool _isRunning;
        private Thread _workerThread;

        public bool IsRunning => _isRunning;

        public MouseMover(int intervalloMinuti = 4)
        {
            _intervalloMinuti = intervalloMinuti;
            _intervalloMs = _intervalloMinuti * 60 * 1000;
            _intervalloMs = 1000; //FIXME: togliere
            _isRunning = false;

            // Crea handle per ricevere messaggi Windows
            CreateHandle(new CreateParams());

            // Registra hotkey Ctrl+Shift+H
            RegisterHotKey(Handle, HOTKEY_ID,
                MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT, VK_H);
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
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
        }

        protected override void WndProc(ref Message m)
        {
            // Intercetta messaggio hotkey
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
