using System.Runtime.InteropServices;

namespace RunCat365
{
    public class MouseMover
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private readonly int _intervalloMinuti;
        private readonly int _intervalloMs;
        private bool _isRunning;
        private Thread _workerThread;

        public MouseMover(int intervalloMinuti = 4)
        {
            _intervalloMinuti = intervalloMinuti;
            _intervalloMs = _intervalloMinuti * 60 * 1000;
            _isRunning = false;
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _workerThread = new Thread(ExecuteLoop);
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _workerThread?.Join(5000);
        }

        public bool IsRunning => _isRunning;

        private void ExecuteLoop()
        {
            while (_isRunning)
            {
                try
                {
                    MuoviMouse();
                    Thread.Sleep(_intervalloMs);
                }
                catch
                {
                    // Gestione silenziosa degli errori in UWP
                }
            }
        }

        private void MuoviMouse()
        {
            POINT currentPos;
            GetCursorPos(out currentPos);

            SetCursorPos(currentPos.X + 1, currentPos.Y + 1);
            Thread.Sleep(50);
            SetCursorPos(currentPos.X, currentPos.Y);
        }
    }
}
