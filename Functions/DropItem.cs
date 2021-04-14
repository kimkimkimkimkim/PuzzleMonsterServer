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
            if (args != null && args["dropTableName"] != null)
                dropTableName = args["dropTableName"];

            // ドロップテーブルからアイテムを取得する
            var evaluateResult = await EvaluateRandomResultTable(context, dropTableName);
            // プレイヤーにアイテムを付与する
            var grantResult = await ItemGiver.GrantItemsToUserAsync(context, new List<string>() { evaluateResult });
            // レスポンスの作成
            var response = new DropItemApiResponse(){
                itemInstanceList = grantResult,
            };
            return PlayFabSimpleJson.SerializeObject(response);
        }

        // ドロップテーブルから取得するアイテムを抽選
        private static async Task<string> EvaluateRandomResultTable(FunctionExecutionContext<dynamic> context, dynamic dropTableName)
        {
            var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

            var result = await serverApi.EvaluateRandomResultTableAsync(new EvaluateRandomResultTableRequest()
            {
                TableId = dropTableName
            });

            return result.Result.ResultItemId;
        }
    }
}
