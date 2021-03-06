﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound.Server.Plugins.GeneratedEvents;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

namespace TrakHound.Server.Plugins.Parts
{
    public class Plugin : IServerPlugin
    {

        public string Name { get { return "TrakHound.Server.Plugins.Parts"; } }

        public void Initialize(DeviceConfiguration config)
        {
            PartsConfiguration pc = PartsConfiguration.Read(config.ConfigurationXML);
            if (pc != null)
            {
                config.CustomClasses.Add(pc);

                configuration = config;
            }
        }


        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "generatedeventitems")
                {
                    var genEventItems = (List<GeneratedEvent>)data.Data02;

                    var pc = PartsConfiguration.Get(configuration);
                    if (pc != null)
                    {
                        genEventItems = genEventItems.FindAll(x => x.EventName == pc.PartsEventName);

                        var infos = new List<PartInfo>();

                        foreach (var item in genEventItems)
                        {
                            PartInfo info = PartInfo.Get(configuration, item);
                            if (info != null && info.Count > 0) infos.Add(info);
                        }

                        if (infos.Count > 0)
                        {
                            Database.AddRows(configuration, infos);
                            SendPartInfos(infos);
                        }
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Starting()
        {
            Database.CreatePartsTable(configuration);
        }

        public void Closing() { }

        //public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }


        private DeviceConfiguration configuration;

        private void SendPartInfos(List<PartInfo> infos)
        {
            var data = new EventData();
            data.Id = "PartInfos";
            data.Data01 = configuration;
            data.Data02 = infos;
            if (SendData != null) SendData(data);
        }
    }
}
