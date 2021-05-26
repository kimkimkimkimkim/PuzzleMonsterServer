using PlayFab.Samples;
using System.Collections.Generic;
using PM.Enum.Data;
using System;
using System.Threading.Tasks;

/// <summary>
/// スタミナ関係のUtil
/// </summary>
public static class StaminaUtil
{
    /// <summary>
    /// スタミナ計算をしてユーザーデータを更新する
    /// </summary>
    public static async Task SetStamina(FunctionExecutionContext<dynamic> context){
        var userData = await DataProcessor.GetUserDataAsync(context);
        var lastCalculatedStaminaDateTime = userData.lastCalculatedStaminaDateTime;
        var now = DateTimeUtil.Now();

        if(lastCalculatedStaminaDateTime == default(DateTime)){
            // 初ログイン時
            var stamina = 10; // TODO : スタミナの初期値
            await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>(){
                { UserDataKey.lastCalculatedStaminaDateTime, now },
                { UserDataKey.stamina, stamina }
            });
        }else{
            // TODO : 計算
            var span = now - lastCalculatedStaminaDateTime;
            var totalSeconds = span.TotalSeconds;
            var increasedStamina = (int)(totalSeconds / 180);
            var stamina = userData.stamina;
            stamina = Math.Min(stamina + increasedStamina, 999); // TODO : 最大スタミナ値
            await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>(){
                { UserDataKey.lastCalculatedStaminaDateTime, now },
                { UserDataKey.stamina, stamina }
            });
        }
    }
}
