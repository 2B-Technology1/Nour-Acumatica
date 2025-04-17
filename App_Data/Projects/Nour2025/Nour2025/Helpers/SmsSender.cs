using PX.Data.Update;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nour2024.Helpers
{
    public class SmsSender
    {
        public static string SendMessage(string message, string recieverNumber,string senderName = "NOURELSHRIF")
        {
            int tenantID = PXInstanceHelper.CurrentCompany;
            if (tenantID == 6)
            {
                senderName = "N Auto Exp";
            }
            else if(tenantID == 3 || tenantID == 5)
            {
                senderName = "NOURELSHRIF";
            }
            var client = new RestClient();
            var request = new RestRequest("https://sms.brandencode.com/api/send/sms/dlr", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            var body = $@"{{
                                ""username"": ""Noureldinelsherif"",
                                ""password"": ""SDHB/_(=M$"",
                                ""message"": ""{message}"",
                                ""language"": ""e"",
                                ""receiver"": ""{recieverNumber}"",
                                ""sender"": ""{senderName}""
                            }}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            string responseContent = response.Content;
            return responseContent;
        }
    }
}
