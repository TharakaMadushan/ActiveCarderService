using LeaveData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace ActiveCarderService
{
    public partial class Service1 : ServiceBase
    {

        private Timer _timer = null;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Library.WriteErrorLog("Service initialize...");
            string appsetting = ConfigurationManager.AppSettings["TimeInterval"].ToString();
            _timer = new Timer();
            this._timer.Interval = Convert.ToDouble(appsetting); //every 1 hour
            this._timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            _timer.Enabled = true;
        }


        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                Library.WriteErrorLog("Service Started!");
                Library.PushActiveCarderData();
                
            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(ex.ToString());
            }
        }

        protected override void OnStop()
        {
            _timer.Enabled = false;
            Library.WriteErrorLog("window service stopped");
        }
    }
}
