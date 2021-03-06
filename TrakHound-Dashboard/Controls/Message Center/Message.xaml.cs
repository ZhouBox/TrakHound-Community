﻿// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrakHound_Dashboard.Controls.Message_Center
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    
    public partial class Message : UserControl
    {
        public Message(MessageData data)
        {
            InitializeComponent();
            DataContext = this;

            Data = data;
            Read = data.Read;
        }

        #region "Properties"

        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set { SetValue(ShownProperty, value); }
        }

        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(Message), new PropertyMetadata(false));


        public MessageData Data
        {
            get { return (MessageData)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(MessageData), typeof(Message), new PropertyMetadata(null));


        public bool Read
        {
            get { return (bool)GetValue(ReadProperty); }
            set { SetValue(ReadProperty, value); }
        }

        public static readonly DependencyProperty ReadProperty =
            DependencyProperty.Register("Read", typeof(bool), typeof(Message), new PropertyMetadata(false));


        #endregion

        #region "Click Events"

        public delegate void Clicked_Handler(Message message);
        public event Clicked_Handler Clicked;
        public event Clicked_Handler CloseClicked;

        private void Close_Clicked(TrakHound_UI.Button bt)
        {
            if (CloseClicked != null) CloseClicked(this);
        }

        //private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    Read = true;
        //    if (Clicked != null) Clicked(this);
        //}

        private void MoreInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Data != null)
            {
                if (Data.AdditionalInfo != null)
                {
                    MessageBox.Show(Data.AdditionalInfo);
                }
            }
        }

        #endregion

        #region "Mouse Over"

        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(Message), new PropertyMetadata(false));


        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseOver = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
        }

        #endregion

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Read = true;
            if (Clicked != null) Clicked(this);
        }

    }

}
