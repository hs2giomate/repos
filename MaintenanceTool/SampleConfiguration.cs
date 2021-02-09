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
#if ATP_TEST
#else
            new Scenario() { Title="Configure Device", ClassType=typeof(Scenario2_ConfigureDevice)},
            new Scenario() { Title="Read/Write", ClassType=typeof(Scenario3_ReadWrite)},
#endif
            //  new Scenario() { Title="USB Serial Events", ClassType=typeof(Scenario4_Events)},
            new Scenario() { Title="Minimum Air Positions", ClassType=typeof(MinimunFreshAir)},
            new Scenario() { Title="Flapper Valve Command", ClassType=typeof(FlapperValveControl)},
            new Scenario() { Title="Events Logger", ClassType=typeof(EventLoggerList)},
            new Scenario() { Title="Heater Operation", ClassType=typeof(HeaterOperation)},
            new Scenario() { Title="Fans Operation", ClassType=typeof(FansOperation)},
             new Scenario() { Title="VCS Operation", ClassType=typeof(Compresor)},
            new Scenario() { Title="Scavenge Fan ", ClassType=typeof(ScavengeFan)},
            new Scenario() { Title="Temperature Sensors", ClassType=typeof(TemperatureSensors)}
        };
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
