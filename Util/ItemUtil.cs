using System;
using PlayFab.ServerModels;
using PM.Enum.Item;

public static class ItemUtil{
    /// <summary>
    /// ItemInstanceからIdを返す
    /// </summary>
    public static long GetItemId(GrantedItemInstance itemInstance)
    {
        // ItemInstanceのIdは「ItemType名+ID値」となっている
        var itemTypeWordCount = itemInstance.ItemClass.Length;
        var itemInstanceId = itemInstance.ItemId;
        var id = itemInstanceId.Substring(itemTypeWordCount);
        return long.Parse(id);
    }

    /// <summary>
    /// ItemInstanceからIdを返す
    /// </summary>
    public static long GetItemId(ItemInstance itemInstance)
    {
        // ItemInstanceのIdは「ItemType名+ID値」となっている
        var itemTypeWordCount = itemInstance.ItemClass.Length;
        var itemInstanceId = itemInstance.ItemId;
        var id = itemInstanceId.Substring(itemTypeWordCount);
        return long.Parse(id);
    }

    /// <summary>
    /// ItemInstanceからItemTypeを返す
    /// </summary>
    public static ItemType GetItemType(GrantedItemInstance itemInstance)
    {
        // ItemInstanceのClassはItemTypeと等しい
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            if (itemInstance.ItemClass == itemType.ToString()) return itemType;
        }

        return ItemType.None;
    }

    /// <summary>
    /// ItemInstanceからItemTypeを返す
    /// </summary>
    public static ItemType GetItemType(ItemInstance itemInstance)
    {
        // ItemInstanceのClassはItemTypeと等しい
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            if (itemInstance.ItemClass == itemType.ToString()) return itemType;
        }

        return ItemType.None;
    }
}