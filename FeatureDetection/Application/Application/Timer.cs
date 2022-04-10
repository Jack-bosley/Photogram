using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class Timer
    {
        public float DeltaTime { get; private set; }
        public float FixedDeltaTime { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime FrameStartTime { get; private set; }
        public DateTime UpdateStartTime { get; private set; }


        public void StartTimer()
        {
            StartTime = DateTime.Now;
        }

        public void StartFrame()
        {
            FrameStartTime = DateTime.Now;
        }

        public void StartUpdate()
        {
            UpdateStartTime = DateTime.Now;
        }


        public void EndFrame()
        {
            TimeSpan frameTime = DateTime.Now - FrameStartTime;
            DeltaTime = (float)frameTime.TotalSeconds;
        }

        public void EndUpdate()
        {
            TimeSpan updateTime = DateTime.Now - UpdateStartTime;
            FixedDeltaTime = (float)updateTime.TotalSeconds;
        }
    }
}
