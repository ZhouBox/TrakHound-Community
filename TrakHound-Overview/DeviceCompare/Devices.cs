﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound.Tools;

namespace TrakHound_Overview
{
    public partial class Overview
    {
        /// <summary>
        /// Handler for Devices.CollectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                ClearDeviceList();
            }

            if (e.NewItems != null)
            {
                foreach (DeviceConfiguration newConfig in e.NewItems)
                {
                    if (newConfig != null) AddDevice(newConfig);
                }

                SortDataItems();
                LoadHeaderView();
                SortDeviceDisplays();
            }

            if (e.OldItems != null)
            {
                foreach (DeviceConfiguration oldConfig in e.OldItems)
                {
                    Devices.Remove(oldConfig);

                    int index = DeviceDisplays.FindIndex(x => x.UniqueId == oldConfig.UniqueId);
                    if (index >= 0)
                    {
                        var dd = DeviceDisplays[index];
                        Headers.Remove(dd.Group.Header);
                        Columns.Remove(dd.Group.Column);
                        dd.CellAdded -= Display_CellAdded;
                        dd.CellSizeChanged -= display_CellSizeChanged;

                        //Overlays.Remove(dd.Group.Overlay);
                        DeviceDisplays.Remove(dd);
                    }
                }
            }
        }

        /// <summary>
        /// Create the DeviceDisplays and RowHeaders based on the Devices set for Overview
        /// </summary>
        /// <param name="devices"></param>
        void UpdateDeviceList(List<DeviceConfiguration> configs)
        {
            if (configs != null)
            {
                DeviceDisplays = new List<DeviceDisplay>();
                Headers.Clear();
                Columns.Clear();
                Overlays.Clear();

                var category = SubCategories.Find(x => x.Name == "Components");
                if (category != null)
                {
                    // Add the RowHeaders
                    AddRowHeaders(Plugins, category.PluginConfigurations);

                    foreach (var config in configs)
                    {
                        AddDevice(config);
                    }

                    SortDataItems();
                    LoadHeaderView();
                    SortDeviceDisplays();
                }
            }
        }

        private void ClearDeviceList()
        {
            DeviceDisplays = new List<DeviceDisplay>();
            Headers.Clear();
            Columns.Clear();
            Overlays.Clear();

            var category = SubCategories.Find(x => x.Name == "Components");
            if (category != null)
            {
                // Add the RowHeaders
                AddRowHeaders(Plugins, category.PluginConfigurations);

                SortDataItems();
                LoadHeaderView();
            }
        }

        private void AddDevice(DeviceConfiguration config)
        {
            var category = SubCategories.Find(x => x.Name == "Components");
            if (category != null)
            {
                var display = new DeviceDisplay(config, Plugins, category.PluginConfigurations);
                display.CellAdded += Display_CellAdded;
                display.CellSizeChanged += display_CellSizeChanged;

                DeviceDisplays.Add(display);
                if (display.Group.Header != null) Headers.Add(display.Group.Header);
                if (display.Group.Column != null) Columns.Add(display.Group.Column);
            }
        }

        private void Display_CellAdded(object sender, System.Windows.RoutedEventArgs e)
        {
            SortDataItems();
        }

        private void AddDeviceDisplay_GUI(DeviceDisplay display)
        {
            DeviceDisplays.Add(display);
            if (display.Group.Header != null) Headers.Add(display.Group.Header);
            if (display.Group.Column != null) Columns.Add(display.Group.Column);

            SortDataItems();
            LoadHeaderView();
            SortDeviceDisplays();
        }

        private void SortDeviceDisplays()
        {
            Headers.Sort();
            Columns.Sort();
            Overlays.Sort();
        }

    }
}
