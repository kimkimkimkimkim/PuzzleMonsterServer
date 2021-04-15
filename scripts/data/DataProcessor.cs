using System.Threading.Tasks;
using System.Collections.Generic;
using PlayFab.Samples;
using PlayFab;
using PlayFab.ServerModels;
using System.Linq;
using Newtonsoft.Json;
using PM.Enum.Data;

// データ処理関係のクラス
public static class DataProcessor
{
    // ユーザーデータを取得する
    public static async Task<UserDataInfo> GetUserDataAsync(FunctionExecutionContext<dynamic> context){
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

        var result = await serverApi.GetUserDataAsync(new GetUserDataRequest()
        {
            PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
        });

        // ユーザーデータのパラム名とそのデータのJsonの辞書にしてユーザーデータを取得
        var userData = UserDataUtil.GetUserData(result.Result.Data);
        
        return userData;
    }

    // ユーザーインベントリ情報を取得する
    public static async Task<UserInventoryInfo> GetUserInventoryAsync(FunctionExecutionContext<dynamic> context){
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

        var result = await serverApi.GetUserInventoryAsync(new GetUserInventoryRequest()
        {
            PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
        });

        var userInventory = UserDataUtil.GetUserInventory(result.Result);
        
        return userInventory;
    }

    // マスタデータを取得する
    public static async Task<List<T>> GetMasterAsyncOf<T>(FunctionExecutionContext<dynamic> context) where T : MasterBookBase{
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);

        var masterDataName = TextUtil.GetDescriptionAttribute<T>();
        var result = await serverApi.GetTitleDataAsync(new GetTitleDataRequest()
        {
            Keys = new List<string>(){ masterDataName },
        });
        var masterDataJson = result.Result.Data[masterDataName];
        var masterDataList = JsonConvert.DeserializeObject<List<T>>(masterDataJson);
        return masterDataList;
    }

    // 指定したキーのユーザーデータを更新
    // key : ex) UserDataKey.userMonsterList
    // value : ex) new List<UserMonsterInfo>(){ }
    public static async Task UpdateUserDataAsync(FunctionExecutionContext<dynamic> context,Dictionary<UserDataKey,object> dict){
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);
        
        var data = dict.ToDictionary(kvp => kvp.Key.ToString(),kvp => JsonConvert.SerializeObject(kvp.Value));
        var result = await serverApi.UpdateUserDataAsync(new UpdateUserDataRequest(){
            PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
            Data = data,
        });
    }

    // 指定したモンスターのカスタムデータを更新
    public static async Task UpdateUserMonsterCustomDataAsync(FunctionExecutionContext<dynamic> context,string itemInstanceId,UserMonsterCustomData customData){
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);
        
        var customDataDict = UserDataUtil.GetCustomDataDict(customData);
        var result = await serverApi.UpdateUserInventoryItemCustomDataAsync(new UpdateUserInventoryItemDataRequest(){
            PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
            ItemInstanceId = itemInstanceId,
            Data = customDataDict,
        });
    }

    // 指定したアイテムを消費します
    public static async Task<ConsumeItemResult> ConsumeItemAsync(FunctionExecutionContext<dynamic> context,string itemInstanceId,int consumeCount){
        var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings,context.AuthenticationContext);
        
        var result = await serverApi.ConsumeItemAsync(new ConsumeItemRequest(){
            PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId,
            ItemInstanceId = itemInstanceId,
            ConsumeCount = consumeCount,
        });
        return result.Result;
    }
}
