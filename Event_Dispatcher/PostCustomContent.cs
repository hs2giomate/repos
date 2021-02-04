using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventLoggerManagment;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace Event_Dispatcher
{
    public struct ECS_JSON_Value
    {
        public string dev_code;
        public string error_code;
        public string log_detail;
    }

    public class PostCustomContent
    {
        private static PostCustomContent handler;
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        private string json_string_value;
        private ECS_JSON_Value json;
        private JsonObject jObject;

        public PostCustomContent()
        {
            handler = this;
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();
            json.dev_code = "Code01";
        }
        public static PostCustomContent PostMan
        {
            get
            {
                if (handler==null)
                {
                    handler = new PostCustomContent();
                }
               
                return handler;
            }
            
        }

        public async void SendEvent(DataLogItem dli)
        {
            Uri resourceUri = Helpers.TryParseHttpUri("https://waiotservices.azurewebsites.net/Logs/SetLog");
            if (resourceUri == null)
            {
      
                return;
            }
          //  IHttpContent jsonContent = new HttpJsonContent(JsonValue.Parse("{\"dev_code\":\"Code06\", \"err_code\": \"6568 : 689\",\"log_detail\":\"fdfsd : fds\"}"));
    //      IHttpContent jsonContent = new HttpJsonContent(JsonValue.Parse(CreateStringJsonValue(dli)));
            IHttpContent jsonContent = new HttpJsonContent(JsonValue.Parse(CreateJsonObject(dli).Stringify()));
            try
            {
                HttpRequestResult result = await httpClient.TryPostAsync(resourceUri, jsonContent).AsTask(cts.Token);
                if (result.Succeeded)
                {

                }

            }
            catch (TaskCanceledException)
            {
     
            }
        }

        private string CreateStringJsonValue(DataLogItem dli)
        {
            json.log_detail = dli.message;
            json_string_value = String.Concat("{\"dev_code\":\"Code01\", \"err_code\":\"W-0808-CCuCAN-X2-CCU\", \"log_detail\":\"", json.log_detail, "\"}");
            return json_string_value;
        }

        private JsonObject CreateJsonObject(DataLogItem dli)
        {
            jObject = new JsonObject();
            jObject.SetNamedValue("dev_code", JsonValue.CreateStringValue("Code01"));
            jObject.SetNamedValue("err_code", JsonValue.CreateStringValue("W-0808-CCuCAN-X2-CCU"));
            jObject.SetNamedValue("log_detail", JsonValue.CreateStringValue(dli.message));
            return jObject;
        }
    }
}
