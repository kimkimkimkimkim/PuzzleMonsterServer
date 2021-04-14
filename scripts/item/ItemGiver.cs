using System.Threading.Tasks;
using System.Collections.Generic;
using PlayFab.Samples;
using PlayFab;
using PlayFab.ServerModels;
using System.Linq;
using PM.Enum.Item;
using PM.Enum.Data;

public static class ItemGiver
{
    // ユーザーにアイテムを付与する
    // ユーザーにアイテムを付与するときは直接GrantItemsToUserを呼ぶんじゃなくてこれを呼ぶ
    public static async Task<List<GrantedItemInstance>> GrantItemsToUserAsync(FunctionExecutionContext<dynamic> context, List<string> itemIdList)
    {
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

        var result = await serverApi.GrantItemsToUserAsync(new GrantItemsToUserRequest()
        {
            PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
            ItemIds = itemIdList
        });
        var grantedItemList = result.Result.ItemGrantResults;

        // 付与したアイテムが未所持モンスターだった場合新規でモンスターデータを作成し追加
        var monsterList = grantedItemList.Where(i => ItemUtil.GetItemType(i) == ItemType.Monster).ToList();
        if(monsterList.Any()){
            var monsterMasterList = await DataProcessor.GetMasterAsyncOf<MonsterMB>(context);
            var userInventory = await DataProcessor.GetUserInventoryAsync(context);
            var notHaveMonsterList = monsterList.Where(i => !userInventory.userMonsterList.Any(u => u.monsterId == ItemUtil.GetItemId(i))).ToList();
            
            // 未所持のモンスターデータを作成する
            foreach(var itemInstance in notHaveMonsterList){
                var monster = monsterMasterList.First(m => m.id == ItemUtil.GetItemId(itemInstance));
                var customData = new UserMonsterCustomData(){
                    hp = 0,
                    attack = 0,
                    grade = monster.initialGrade,
                };
                await DataProcessor.UpdateUserInventoryCustomData(context, itemInstance.ItemInstanceId,customData);
            }
        }

        return grantedItemList;
    }
}
