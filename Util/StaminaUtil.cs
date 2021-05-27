using PlayFab.Samples;
using System.Collections.Generic;
using PM.Enum.Data;
using System;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// スタミナ関係のUtil
/// </summary>
public static class StaminaUtil
{
    /// <summary>
    /// スタミナ計算をしてユーザーデータを更新する
    /// </summary>
    public static async Task SetStamina(FunctionExecutionContext<dynamic> context){
        await CalculateAndSetCurrentStaminaAsync(context);
    }

    /// <summary>
    /// 現在のスタミナを取得する
    /// </summary>
    public static async Task<int> GetCurrentStaminaAsync(FunctionExecutionContext<dynamic> context){
        var response = await CalculateAndSetCurrentStaminaAsync(context);
        return response.currentStamina;
    }

    /// <summary>
    /// スタミナを消費する
    /// </summary>
    public static async Task<int> ConsumeAsync(FunctionExecutionContext<dynamic> context,int consumeStamina){
        var response = await CalculateAndSetCurrentStaminaAsync(context);
        PMApiUtil.ErrorIf(response.currentStamina < consumeStamina, PMErrorCode.Unknown, "not enough stamina.",new Dictionary<string, object>(){
            {"currentStamina", response.currentStamina},
            {"consumeStamina", consumeStamina},
        });

        var remainStamina = response.currentStamina - consumeStamina;
        // スタミナ消費時は時間の更新は行わない
        await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>(){
            { UserDataKey.stamina, remainStamina }
        });
        return remainStamina;
    }

    /// <summary>
    /// 時間経過によるスタミナ回復を考慮した現在のスタミナを計算し保存し返す
    /// </summary>
    private static async Task<CalculateStaminaResult> CalculateAndSetCurrentStaminaAsync(FunctionExecutionContext<dynamic> context){
        var userData = await DataProcessor.GetUserDataAsync(context);
        var staminaList = await DataProcessor.GetMasterAsyncOf<StaminaMB>(context);
        var lastCalculatedStaminaDateTime = userData.lastCalculatedStaminaDateTime;
        var now = DateTimeUtil.Now();

        if(lastCalculatedStaminaDateTime == default(DateTime)){
            // 初ログイン時
            var rank = 1; // 初ログイン時はランク1としている
            var stamina = staminaList.First(m => m.rank == rank).stamina;
            await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>(){
                { UserDataKey.lastCalculatedStaminaDateTime, now },
                { UserDataKey.stamina, stamina }
            });
            return new CalculateStaminaResult(){
                currentStamina = stamina,
                lastCalculatedDateTime = now,
            };
        }else{
            var staminaMB = staminaList.FirstOrDefault(m => m.rank == userData.rank);
            PMApiUtil.ErrorIf(staminaMB == null,PMErrorCode.Unknown, $"invalid user rank => userRank:{userData.rank}");

            var maxStamina = staminaMB.stamina;
            var stamina = userData.stamina;
            var currentStamina = 0;
            var newLastCalculatedStaminaDateTime = new DateTime();

            if(stamina >= maxStamina){
                // すでに最大スタミナ値を超えている場合は時間経過でのスタミナを追加しない
                currentStamina = stamina;
                newLastCalculatedStaminaDateTime = now;
            }else{
                // 時間経過による回復スタミナを追加する
                var span = now - lastCalculatedStaminaDateTime;
                var totalMilliSeconds = span.TotalMilliseconds;
                var increasedStamina = (int)Math.Floor(totalMilliSeconds / ConstManager.User.millSecondsPerStamina); // 経過時間を間隔で割った商が回復したスタミナ
                var remainMilliSeconds = totalMilliSeconds - (increasedStamina * ConstManager.User.millSecondsPerStamina); // 今のスタミナになってから経過した時間
                currentStamina = Math.Min(stamina + increasedStamina, maxStamina);
                newLastCalculatedStaminaDateTime = now.AddMilliseconds(-remainMilliSeconds); // lastCalculatedStaminaDateTimeには今のスタミナになったちょうどの日時を登録する
            }

            await DataProcessor.UpdateUserDataAsync(context, new Dictionary<UserDataKey, object>(){
                { UserDataKey.lastCalculatedStaminaDateTime, newLastCalculatedStaminaDateTime },
                { UserDataKey.stamina, currentStamina }
            });
            return new CalculateStaminaResult(){
                currentStamina = currentStamina,
                lastCalculatedDateTime = newLastCalculatedStaminaDateTime,
            };
        }
    }
}

class CalculateStaminaResult{
    public int currentStamina { get; set; }
    public DateTime lastCalculatedDateTime { get; set; }
}
