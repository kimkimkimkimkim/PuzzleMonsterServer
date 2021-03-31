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
    public static class ExecuteGacha
    {
        [FunctionName("ExecuteGacha")]
        public static async Task<dynamic> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var body = await req.ReadAsStringAsync();
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(body);
            var args = context.FunctionArgument;

            var monsterList = await GetTitleData(context);
            var monster = monsterList[ new Random().Next(0, monsterList.Count) ];
            return monster;
        }

        private static async Task<List<MonsterMB>> GetTitleData(FunctionExecutionContext<dynamic> context)
        {
            var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

            var result = await serverApi.GetTitleDataAsync(new PlayFab.ServerModels.GetTitleDataRequest()
            {
                Keys = new List<string>(){ "MonsterMB" },
            });

            var monsterList = JsonConvert.DeserializeObject<List<MonsterMB>>(result.Result.Data["MonsterMB"]);
            return monsterList;
        }
    }
}
