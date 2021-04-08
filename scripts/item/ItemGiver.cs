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

        // 付与したアイテムが未所持モンスターだった場合新規でユーザーデータを作成し追加
        var monsterMasterList = await DataProcessor.GetMasterAsyncOf<MonsterMB>(context);
        var userData = await DataProcessor.GetUserDataAsync(context);
        var monsterList = grantedItemList.Where(i => ItemUtil.GetItemType(i) == ItemType.Monster).ToList();
        var existsNotHave = false; // 未所持のモンスターが付与されたかどうかを判定するフラグ
        monsterList.ForEach(i => {
            var isNotHave = !userData.userMonsterList.Any(u => u.monsterId == ItemUtil.GetItemId(i));
            if(isNotHave){
                existsNotHave = true;

                // 未所持ならユーザーデータを作成する
                var monster = monsterMasterList.First(m => m.id == ItemUtil.GetItemId(i));
                var userMonster = new UserMonsterInfo(){
                    id = UserDataUtil.CreateUserDataId(),
                    monsterId = ItemUtil.GetItemId(i),
                    hp = 0,
                    attack = 0,
                    grade = monster.initialGrade,
                };
                userData.userMonsterList.Add(userMonster);
            }
        });

        // 未所持モンスターが付与されていればユーザーデータを更新
        if(existsNotHave){
            await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>(){
                {UserDataKey.userMonsterList, userData.userMonsterList}
            });
        }

        return grantedItemList;
    }
}
