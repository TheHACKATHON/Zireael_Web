using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class HoldMouseButton
    {
        private readonly Stopwatch _stopwatch;
        public bool SelectedNow { get; set; }
        public int MilisecondsToSelect { get; set; } = 450;

        public HoldMouseButton()
        {
            _stopwatch = new Stopwatch();
        }
        
        public async Task<bool> Start()
        {
            if (_stopwatch.IsRunning)
            {
                Stop();
            }
            _stopwatch.Start();
            return await Task.Factory.StartNew(() =>
            {
                while (_stopwatch.IsRunning)
                {
                    Thread.Sleep(20);
                    if (_stopwatch.ElapsedMilliseconds >= MilisecondsToSelect)
                    {
                        Stop();
                        return true;
                    }
                }
                return false;
            });
        }

        public void Stop()
        {
            _stopwatch.Reset();
        }
    }
}