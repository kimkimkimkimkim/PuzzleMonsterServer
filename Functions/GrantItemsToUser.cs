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

namespace SANGWOO.Function
{
    public static class GrantItemsToUser
    {
        [FunctionName("GrantItemsToUser")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string body = await req.ReadAsStringAsync();
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<Dictionary<string,List<string>>>>(body);
            var args = context.FunctionArgument;

            // 引数でテーブル名を渡す
            dynamic itemIdList = null;
            if (args != null && args["itemIdList"] != null) itemIdList = args["itemIdList"];

            // ドロップテーブルからアイテムを取得する
            var grantedItemList = await GrantItemsToUserTask(context, itemIdList);

            return PlayFabSimpleJson.SerializeObject(grantedItemList);
        }

        // ドロップテーブルから取得するアイテムを抽選
        private static async Task<List<GrantedItemInstance>> GrantItemsToUserTask(FunctionExecutionContext<Dictionary<string,List<string>>> context, dynamic itemIdList)
        {
            var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

            var result = await serverApi.GrantItemsToUserAsync(new GrantItemsToUserRequest()
            {
                PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
                ItemIds = itemIdList
            });

            return result.Result.ItemGrantResults;
        }
    }
}
