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

        bool ShowForceOff()
        {
            Process p = Process.Start("");
            return p.Start();
        }

        void WorkerThreadFunc()
        {
            AlarmsFile file = new AlarmsFile();
            file.FileName = (string)ParentalControlsRegistry.GetRegistryKey().GetValue("AlarmFile", "");
            while (!_shutdownEvent.WaitOne(0))
            {
                if (file.IsValidForSaving())
                {
                    
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
