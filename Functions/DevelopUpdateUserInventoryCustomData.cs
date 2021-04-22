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

namespace SANGWOO.Function
{
    public static class DevelopUpdateUserInventoryCustomData
    {
        [FunctionName("DevelopUpdateUserInventoryCustomData")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try{
                string body = await req.ReadAsStringAsync();
                var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
                var request = JsonConvert.DeserializeObject<FunctionExecutionContext<DevelopUpdateUserInventoryCustomDataApiRequest>>(body).FunctionArgument;

                await UpdateUserInventoryCustomDataAsync(context, request);

                return PlayFabSimpleJson.SerializeObject(new DevelopUpdateUserInventoryCustomDataApiResponse());
            }catch(PMApiException e){
                // レスポンスの作成
                var response = new PMApiResponseBase(){
                    errorCode = e.errorCode,
                    message = e.message
                };
                return PlayFabSimpleJson.SerializeObject(response);
            }
        }

        // インベントリカスタムデータを更新
        private static async Task UpdateUserInventoryCustomDataAsync(FunctionExecutionContext<dynamic> context, DevelopUpdateUserInventoryCustomDataApiRequest request)
        {
            var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

            var result = await serverApi.UpdateUserInventoryItemCustomDataAsync(new UpdateUserInventoryItemDataRequest()
            {
                PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
                ItemInstanceId = request.itemInstanceId,
                Data = request.data,
            });
        }
    }
}