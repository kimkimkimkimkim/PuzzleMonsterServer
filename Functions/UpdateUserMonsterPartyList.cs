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

namespace SANGWOO.Function
{
    public static class UpdateUserMonsterFormationList
    {
        [FunctionName("UpdateUserMonsterFormationList")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try{
                string body = await req.ReadAsStringAsync();
                var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
                var request = JsonConvert.DeserializeObject<FunctionExecutionContext<UpdateUserMonsterFormationListApiRequest>>(body).FunctionArgument;

                var userData = await DataProcessor.GetUserDataAsync(context);
                var userMonsterPartyList = userData.userMonsterPartyList ?? new List<UserMonsterPartyInfo>();
                var index = userMonsterPartyList.FindIndex(u => u.partyId == request.partyId);
                if(index < 0){
                    // 存在しない場合は新規作成して追加
                    var userMonsterParty = new UserMonsterPartyInfo(){
                        id = UserDataUtil.CreateUserDataId(),
                        userMonsterIdList = request.userMonsterIdList,
                    };
                    userMonsterPartyList.Add(userMonsterParty);
                }else{
                    // すでに存在する場合は更新
                    userMonsterPartyList[index].userMonsterIdList = request.userMonsterIdList;
                }
                await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>() { {UserDataKey.userMonsterPartyList, userMonsterPartyList} });

                var response = new UpdateUserMonsterFormationListApiResponse(){};
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
