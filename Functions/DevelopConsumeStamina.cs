using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab.Json;
using PlayFab.Samples;

namespace SANGWOO.Function
{
    public static class DevelopConsumeStamina
    {
        [FunctionName("DevelopConsumeStamina")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try{
                string body = await req.ReadAsStringAsync();
                var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
                var request = JsonConvert.DeserializeObject<FunctionExecutionContext<DevelopConsumeStaminaApiRequest>>(body).FunctionArgument;

                var stamina = await StaminaUtil.ConsumeAsync(context, request.consumeStamina);

                return PlayFabSimpleJson.SerializeObject(new DevelopConsumeStaminaApiResponse(){ stamina = stamina});
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