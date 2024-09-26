using Azure.Core;
using crs.core.DbModels;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace crs.core.Services
{
    public class Crs_GptService
    {
        public class GuideParameter
        {
            [JsonProperty("language")] public string Language { get; set; } = "Chinese";
            [JsonProperty("module_name")] public string ModuleName { get; set; }
            [JsonProperty("patient_name")] public string PatientName { get; set; }
            [JsonProperty("question")] public string Question { get; set; }
        }

        public readonly static Crs_GptService Instance = new Lazy<Crs_GptService>(() => new Crs_GptService()).Value;

        readonly int timeOut = 30;
        readonly string domain = "https://u477648-bd7b-a6cf61d5.westb.seetacloud.com:8443";
        readonly string guideApi = "/gpt/guide/invoke";

        private Crs_GptService()
        {
            var path = @".\args\gpt-args.ini";
            if (File.Exists(path))
            {
                var settings = File.ReadAllLines(path);

                this.domain = settings.FirstOrDefault(m => m.StartsWith("--domain="))?.Split("=")[1];
            }
        }

        public async Task<(bool status, string msg, JToken data)> Guide(GuideParameter parameter, CancellationToken token = default)
        {
            var logMsg = new StringBuilder(512);

            try
            {
                var request = new Uri(domain).AppendPathSegment(guideApi).WithTimeout(timeOut);

                var response = await request.PostJsonAsync(new
                {
                    input = new
                    {
                        language = parameter.Language,
                        module_name = parameter.ModuleName,
                        patient_name = parameter.PatientName,
                        question = parameter.Question,
                    }
                }, cancellationToken: token);

                var data = await response.GetStringAsync();

                logMsg.Append($"Url：{request.Url}\r\n");
                logMsg.Append($"Method：{request.Verb.Method}\r\n");
                logMsg.Append($"Header：{JsonConvert.SerializeObject(request.Headers)}\r\n");
                logMsg.Append($"Content：{((CapturedStringContent)request.Content).Content}\r\n");
                logMsg.Append($"Result：{data}\r\n");

                return (true, null, JToken.Parse(data));
            }
            catch (Exception ex)
            {
                logMsg.Append(logMsg.ToString());
                return (false, ex.Message, null);
            }
            finally
            {
                Debug.WriteLine(logMsg.ToString());
                Console.WriteLine(logMsg.ToString());
            }
        }
    }
}
