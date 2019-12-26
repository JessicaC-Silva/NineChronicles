using Libplanet;
using Nekoyume.Game.Mail;
using Nekoyume.Game.Quest;
using Nekoyume.Model;
using UniRx;
using Inventory = Nekoyume.Game.Item.Inventory;

namespace Nekoyume.State
{
    /// <summary>
    /// 현재 선택된 AvatarState가 포함하는 값의 변화를 각각의 ReactiveProperty<T> 필드를 통해 외부에 변화를 알린다.
    /// </summary>
    public static class ReactiveCurrentAvatarState
    {
        public static readonly ReactiveProperty<Address> Address = new ReactiveProperty<Address>();
        public static readonly ReactiveProperty<Inventory> Inventory = new ReactiveProperty<Inventory>();
        public static readonly ReactiveProperty<MailBox> MailBox = new ReactiveProperty<MailBox>();
        public static readonly ReactiveProperty<WorldInformation> WorldInformation = new ReactiveProperty<WorldInformation>();
        public static readonly ReactiveProperty<int> ActionPoint = new ReactiveProperty<int>();
        public static readonly ReactiveProperty<long> DailyRewardReceivedIndex = new ReactiveProperty<long>();
        public static readonly ReactiveProperty<QuestList> QuestList = new ReactiveProperty<QuestList>();

        public static void Initialize(AvatarState avatarState)
        {
            if (avatarState is null)
                return;
            
            Address.SetValueAndForceNotify(avatarState.address);
            Inventory.SetValueAndForceNotify(avatarState.inventory);
            MailBox.SetValueAndForceNotify(avatarState.mailBox);
            WorldInformation.SetValueAndForceNotify(avatarState.worldInformation);
            ActionPoint.SetValueAndForceNotify(avatarState.actionPoint);
            DailyRewardReceivedIndex.SetValueAndForceNotify(avatarState.dailyRewardReceivedIndex);
            QuestList.SetValueAndForceNotify(avatarState.questList);
        }
    }
}