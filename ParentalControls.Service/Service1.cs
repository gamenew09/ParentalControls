using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ParentalControls.Common;

namespace ParentalControls.Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        bool ShowBlocker()
        {
            Process p = Process.Start(ParentalControlsRegistry.GetRegistryKey(true).GetValue("Path") + @"\ParentalControls.GUI.exe");
            return p.Start();
        }

        DayOfWeek GetCurrentDay()
        {
            DateTime time = DateTime.Now;
            return time.DayOfWeek;
        }

        Time GetCurrentTime()
        {
            DateTime dt = DateTime.Now;
            Time t = new Time();
            t.Hour = dt.Hour;
            t.Minutes = dt.Minute;
            t.Seconds = dt.Second;
            return t;
        }

        bool IsTimeToAlarm(Alarm alarm)
        {
            Time t = GetCurrentTime();
            return (alarm.AlarmTime.Hour == t.Hour && alarm.AlarmTime.Minutes == t.Minutes);
        }

        void WorkerThreadFunc()
        {
            AlarmsFile file = new AlarmsFile();
            file.FileName = (string)ParentalControlsRegistry.GetRegistryKey().GetValue("AlarmFile", "");
            while (!_shutdownEvent.WaitOne(0))
            {
                if (file.IsValidForSaving())
                {
                    foreach (Alarm alarm in file.Alarms)
                    {
                        if (alarm.RepeatDays.HasFlag(GetCurrentDay()) && IsTimeToAlarm(alarm))
                        {
                            if (ShowBlocker())
                            {
                                Time a = GetCurrentTime();
                                Console.WriteLine("The Alarm Blocker showed at {0}:{1} {2}.", a.Hour, a.Minutes, (a.Hour > 12 ? "PM" : "AM"));
                            }
                        }
                    }
                }
                Thread.Sleep(waitTime);
            }
        }

        int waitTime = 500;

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        Thread _thread;

        protected override void OnStart(string[] args)
        {
            _thread = new Thread(WorkerThreadFunc);
            _thread.Name = "ParentalControls.Worker";
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected override void OnStop()
        {
            _shutdownEvent.Set();
            if (!_thread.Join(3000))
            { 
                // give the thread 3 seconds to stop
                _thread.Abort();
            }
        }
    }
}
