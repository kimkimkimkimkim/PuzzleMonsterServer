using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using PlayFab.Json;
using PlayFab.Samples;
using PlayFab;
using PlayFab.ServerModels;
using System.Linq;
using PM.Enum.Item;
using PM.Enum.Data;
using System;

namespace SANGWOO.Function
{
    public static class Login
    {
        [FunctionName("Login")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try{
                string body = await req.ReadAsStringAsync();
                var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
                var request = JsonConvert.DeserializeObject<FunctionExecutionContext<LoginApiRequest>>(body).FunctionArgument;

                var now = DateTimeUtil.Now();
                await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>(){ {UserDataKey.lastLoginDateTime, now} });

                var response = new LoginApiResponse(){};
                return PlayFabSimpleJson.SerializeObject(response);
            }catch(PMApiException e){
                // レスポンスの作成
                var response = new PMApiResponseBase(){
                    errorCode = e.errorCode,
                    message = e.message
                };
                return PlayFabSimpleJson.SerializeObject(response);
            }
        }
    }
}
