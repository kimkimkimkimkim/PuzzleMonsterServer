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

namespace SANGWOO.Function
{
    public static class MonsterLevelUp
    {
        [FunctionName("MonsterLevelUp")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string body = await req.ReadAsStringAsync();
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
            var request = JsonConvert.DeserializeObject<FunctionExecutionContext<MonsterLevelUpApiRequest>>(body).FunctionArgument;

            // 対象のモンスターを取得
            var userInventory = await DataProcessor.GetUserInventoryAsync(context);
            var userMonster = userInventory.userMonsterList.FirstOrDefault(u => u.id == request.userMonsterId);
            if(userMonster == null) throw new System.Exception();

            // 何レベになるか計算
            var levelUpTableList = await DataProcessor.GetMasterAsyncOf<MonsterLevelUpTableMB>(context);
            var afterExp = userMonster.customData.exp + request.exp;
            var targetLevelUpTable = levelUpTableList.OrderBy(m => m.id).FirstOrDefault(m => m.totalRequiredExp >= afterExp);
            if(targetLevelUpTable == null) throw new System.Exception();
            var afterLevel = targetLevelUpTable.level;

            var response = new MonsterLevelUpApiResponse(){ level = afterLevel };
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