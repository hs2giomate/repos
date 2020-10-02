﻿//*********************************************************  
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelInterop
{
    class Program
    {
        static AppServiceConnection connection = null;
        static AutoResetEvent appServiceExit;
        private static string fileName = "C:/Users/GMateusDP/Documents/SandBox/AW249ECSFaultSignalshs2_200604.xlsx";
        private static int m, n, rowDescription;
        private static string description;

        static void Main(string[] args)
        {
            // connect to app service and wait until the connection gets closed
            appServiceExit = new AutoResetEvent(false);
            InitializeAppServiceConnection();
            appServiceExit.WaitOne();
        }

        static async void InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "ExcelInteropService";
            connection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                // TODO: error handling
            }
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            // signal the event so the process can shut down
            appServiceExit.Set();
        }

        private async static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral because we use an awaitable API below to respond to the message
            // and we don't want this call to get cancelled while we are waiting.
            var messageDeferral = args.GetDeferral();

            string value = args.Request.Message["REQUEST"] as string;
            string result = "";
            switch (value)
            {
                case "CreateSpreadsheet":

                    try
                    {
                        result = "try";
                        // call Office Interop APIs to create the Excel spreadsheet
                        Excel.Application excel = new Excel.Application();
                        excel.Visible = true;
                        Excel.Workbook wb = excel.Workbooks.Add();
                        Excel.Worksheet sh = wb.Sheets.Add();
                        sh.Name = "EventsLog";
                        sh.Cells[1, "A"].Value2 = "Datetime";
                        sh.Cells[1, "B"].Value2 = "Event";
                        sh.Cells[1, "C"].Value2 = "Description";
                      

                        for (int i = 0; i < args.Request.Message.Values.Count / 3; i++)
                        {
                            sh.Cells[i + 2, "A"].Value2 = args.Request.Message["DateTime" + i.ToString()] as string;
                            sh.Cells[i + 2, "B"].Value2 = args.Request.Message["Event" + i.ToString()] as string;
                            sh.Cells[i + 2, "C"].Value2 = args.Request.Message["Description" + i.ToString()] as string;
                           
                        }

                        result = "SUCCESS";
                


                    }
                    catch (Exception exc)
                    {
                        result = exc.Message;
                    }
                    break;
                case "ErrorDescription":
                    string tag, EventTag;
                    EventTag = args.Request.Message["TAG"] as string;
                    try
                    {
                        // call Office Interop APIs to create the Excel spreadsheet
                        Excel.Application excel = new Excel.Application();
                        excel.Visible = true;
                        Excel.Workbook book = excel.Workbooks.Open(fileName);
                        Excel.Range localCell;

                        /*
                        Excel.Workbook wb = excel.Workbooks.Add();
                        Excel.Worksheet sh = wb.Sheets.Add();
                        sh.Name = "DataGrid";
                        sh.Cells[1, "A"].Value2 = "Id";
                        sh.Cells[1, "B"].Value2 = "Description";
                        sh.Cells[1, "C"].Value2 = "Quantity";
                        sh.Cells[1, "D"].Value2 = "UnitPrice";

                        for (int i = 0; i < args.Request.Message.Values.Count / 4; i++)
                        {
                            sh.Cells[i + 2, "A"].Value2 = args.Request.Message["Id" + i.ToString()] as string;
                            sh.Cells[i + 2, "B"].Value2 = args.Request.Message["Description" + i.ToString()] as string;
                            sh.Cells[i + 2, "C"].Value2 = args.Request.Message["Quantity" + i.ToString()].ToString();
                            sh.Cells[i + 2, "D"].Value2 = args.Request.Message["UnitPrice" + i.ToString()].ToString();
                        }
                        */
                        Excel.Worksheet sheet = book.Worksheets["FaultsList"];
                        sheet.Activate();
                        Excel.Range totalRange = sheet.UsedRange;
                        m = totalRange.Rows.Count;
                        n = totalRange.Row;
                        Excel.Range range = sheet.get_Range(string.Concat("G", n.ToString()), string.Concat("G", m.ToString()));
                        range.Select();

                        for (int i = 16; i < 60; i++)
                        {
                            localCell = sheet.get_Range(string.Concat("G", i.ToString()));
                            tag = localCell.Value;
                            if (tag != string.Empty)
                            {
                                if (tag.Contains(EventTag))
                                {
                                    rowDescription = i;
                                    range = sheet.get_Range(string.Concat("E", i.ToString()));
                                    description = range.Value;
                                    result = description;
                                    break;

                                }
                                else
                                {
                                    result = tag;
                                }
                            }

                        }
                        /*
                        range = sheet.get_Range(string.Concat("E", 6));
                        description = range.Value;
                        result = description;
                        */
                        book.Close();
                        excel.Quit();


                    }
                    catch (Exception exc)
                    {
                        result = exc.Message;
                    }
                    break;
                default:
                    result = "unknown request";
                    break;
            }

            ValueSet response = new ValueSet();
            response.Add("RESPONSE", result);

            try
            {
                await args.Request.SendResponseAsync(response);
            }
            finally
            {
                // Complete the deferral so that the platform knows that we're done responding to the app service call.
                // Note for error handling: this must be called even if SendResponseAsync() throws an exception.
                messageDeferral.Complete();
            }
        }
    }
}
