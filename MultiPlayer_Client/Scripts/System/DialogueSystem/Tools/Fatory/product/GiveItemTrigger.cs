public class GiveItemTrigger : IEventTrigger
{
    //物品Id
    private readonly string _itemId;

    public GiveItemTrigger(string ItemId)
    {
        this._itemId = ItemId;
    }

    public void Execute()
    {
        //通知拾取物品
    }
}