﻿using System;
using System.Linq;


using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;


namespace TrakHound_Dashboard.Pages.Dashboard.ControllerStatus
{
    public partial class ControllerStatus
    {

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;
        const System.Windows.Threading.DispatcherPriority PRIORITY_CONTEXT_IDLE = System.Windows.Threading.DispatcherPriority.ContextIdle;

        void Update(EventData data)
        {
            if (data != null && data.Id != null && data.Data01 != null)
            {
                var config = data.Data01 as DeviceConfiguration;
                if (config != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        int index = Rows.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                        if (index >= 0)
                        {
                            var row = Rows[index];
                            row.UpdateData(data);   
                        }
                    }));
                }
            }
        }
        
    }
}
