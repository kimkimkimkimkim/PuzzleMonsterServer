using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using PlayFab.Json;
using PlayFab.Samples;
using PlayFab;

namespace SANGWOO.Function
{
    public static class DropItem
    {
        [FunctionName("DropItem")]
        public static async Task<dynamic> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
        {
        string body = await req.ReadAsStringAsync();
        var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
        var args = context.FunctionArgument;

        // 引数でテーブル名を渡す
        dynamic dropTableName = null;
        if (args != null && args["DropTableName"] != null)
            dropTableName = args["DropTableName"];

        // ドロップテーブルからアイテムを取得する
        var evaluateResult = await EvaluateRandomResultTable(context, dropTableName);
        // プレイヤーにアイテムを付与する
        var grantResult = await GrantItemsToUser(context, new List<string>() { evaluateResult });

        return PlayFabSimpleJson.SerializeObject(grantResult);
        }

        private static async Task<string> EvaluateRandomResultTable(FunctionExecutionContext<dynamic> context, dynamic dropTableName)
        {
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

        var result = await serverApi.EvaluateRandomResultTableAsync(new PlayFab.ServerModels.EvaluateRandomResultTableRequest()
        {
            TableId = dropTableName
        });

        return result.Result.ResultItemId;
        }

        private static async Task<List<PlayFab.ServerModels.GrantedItemInstance>> GrantItemsToUser(FunctionExecutionContext<dynamic> context, List<string> itemIds)
        {
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

        var result = await serverApi.GrantItemsToUserAsync(new PlayFab.ServerModels.GrantItemsToUserRequest()
        {
            PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
            ItemIds = itemIds
        });

        return result.Result.ItemGrantResults;
        }
    }
}
