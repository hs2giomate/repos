//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;


namespace MaintenanceToolECSBOX
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "  ECS BOX  ";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Connect/Disconnect", ClassType=typeof(Scenario1_ConnectDisconnect)},
            new Scenario() { Title="Configure Device", ClassType=typeof(Scenario2_ConfigureDevice)},
            new Scenario() { Title="Read/Write", ClassType=typeof(Scenario3_ReadWrite)},
            new Scenario() { Title="USB Serial Events", ClassType=typeof(Scenario4_Events)},
            new Scenario() { Title="Flapper Valve Offset", ClassType=typeof(FlapperValveOffset)},
             new Scenario() { Title="Events Logger", ClassType=typeof(EventLoggerList)},
              new Scenario() { Title="Heater Operation", ClassType=typeof(HeaterOperation)}
        };
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
