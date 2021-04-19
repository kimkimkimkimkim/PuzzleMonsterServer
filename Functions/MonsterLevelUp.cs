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
    public static class MonsterLevelUp
    {
        [FunctionName("MonsterLevelUp")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try{
                string body = await req.ReadAsStringAsync();
                var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
                var request = JsonConvert.DeserializeObject<FunctionExecutionContext<MonsterLevelUpApiRequest>>(body).FunctionArgument;

                // 対象のモンスターを取得
                var userInventory = await DataProcessor.GetUserInventoryAsync(context);
                var userMonster = userInventory.userMonsterList.FirstOrDefault(u => u.id == request.userMonsterId);
                PMApiUtil.ErrorIf(userMonster == null, PMErrorCode.Unknown, "invalid userMonsterId");

                // 経験値を十分に保持しているかチェック
                var exp = userInventory.userPropertyList.FirstOrDefault(u => u.propertyId == (long)PropertyType.MonsterExp);
                PMApiUtil.ErrorIf(exp == null || exp.num < request.exp,PMErrorCode.Unknown, "not enough exp");

                // 何レベになるか計算
                var levelUpTableList = await DataProcessor.GetMasterAsyncOf<MonsterLevelUpTableMB>(context);
                var afterExp = userMonster.customData.exp + request.exp;
                var targetLevelUpTable = levelUpTableList.OrderBy(m => m.id).LastOrDefault(m => m.totalRequiredExp <= afterExp);
                PMApiUtil.ErrorIf(targetLevelUpTable == null, PMErrorCode.Unknown, "invalid levelUpTable");
                var afterLevel = targetLevelUpTable.level;

                // 対象のモンスターがマスタに存在するかチェック
                var monsterList = await DataProcessor.GetMasterAsyncOf<MonsterMB>(context);
                var monster = monsterList.FirstOrDefault(m => m.id == userMonster.monsterId);
                PMApiUtil.ErrorIf(monster == null, PMErrorCode.Unknown, "invalie monsterId");

                // モンスターをレベルアップ
                var afterStatus = MonsterUtil.GetMonsterStatus(monster, afterLevel);
                var customData = new UserMonsterCustomData(){
                    level = afterLevel,
                    exp = afterExp,
                    hp = afterStatus.hp,
                    attack = afterStatus.attack,
                    heal = afterStatus.heal,
                    grade = userMonster.customData.grade,
                };
                await DataProcessor.UpdateUserMonsterCustomDataAsync(context, userMonster.id, customData);

                // 経験値を消費
                await DataProcessor.ConsumeItemAsync(context, exp.id, request.exp);

                // 強化後のレベルを返す
                var response = new MonsterLevelUpApiResponse(){ level = afterLevel };
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