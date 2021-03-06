﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using TrakHound.Configurations;
using TrakHound.Tools;
using TrakHound.Plugins;

namespace TH_DeviceCompare_CNC.Text.Execution_Mode
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Plugin : UserControl
    {
        public Plugin()
        {
            InitializeComponent();
            DataContext = this;
        }

        const string SNAPSHOT_LINK = "Execution Mode";
        const string STATUS_LINK = "EXECUTION";


        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(Plugin), new PropertyMetadata(null));


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;

        bool snapshotFound = false;

        void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(DeviceConfiguration))
            {
                // Snapshot Table Data
                if (data.Id.ToLower() == "statusdata_snapshots")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateSnapshot), Priority_Context, new object[] { data.Data02 });
                }

                // Status Table Data
                if (data.Id.ToLower() == "statusdata_status" && !snapshotFound)
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateStatus), Priority_Context, new object[] { data.Data02 });
                }
            }
        }


        void UpdateSnapshot(object data)
        {
            var dt = data as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", SNAPSHOT_LINK, "value");

                if (value != null)
                {
                    ProcessValue(value);
                    snapshotFound = true;
                }
                else snapshotFound = false;
            }
        }

        void UpdateStatus(object data)
        {
            var dt = data as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "type", STATUS_LINK, "value1");
                ProcessValue(value);
            }
        }

        void ProcessValue(string value)
        {
            if (Value != value)
            {
                Value = value;

                if (value != null)
                {
                    switch (value)
                    {
                        case "ACTIVE":
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                            Background = new SolidColorBrush(Color.FromRgb(25, 180, 25));
                            break;
                        default:
                            Foreground = (Brush)TryFindResource("Foreground_Normal");
                            Background = new SolidColorBrush(Colors.Transparent);
                            break;
                    }
                }
            }
        }


        //void Update(EventData data)
        //{
        //    if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(Configuration))
        //    {
        //        Snapshot Table Data
        //        if (data.Id.ToLower() == "statusdata_snapshots")
        //        {
        //            this.Dispatcher.BeginInvoke(new Action<object>(UpdateText), Priority_Context, new object[] { data.Data02 });
        //        }
        //    }
        //}


        //void UpdateText(object snapshotData)
        //{
        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        string value = DataTable_Functions.GetTableValue(dt, "name", link, "value");

        //        Value = value;

        //        if (value != null)
        //        {
        //            switch (value)
        //            {
        //                case "ACTIVE":
        //                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        //                    Background = new SolidColorBrush(Color.FromRgb(25, 180, 25));
        //                    break;
        //                default:
        //                    Foreground = (Brush)TryFindResource("Foreground_Normal");
        //                    Background = new SolidColorBrush(Colors.Transparent);
        //                    break;
        //            }
        //        }
        //    }
        //}

    }
}
