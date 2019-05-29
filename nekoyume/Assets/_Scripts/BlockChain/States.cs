using Nekoyume.Model;
using Nekoyume.State;
using UniRx;

namespace Nekoyume.BlockChain
{
    /// <summary>
    /// 각 주소 고유의 상태들을 모아서 데이터 지역성을 확보한다.
    /// </summary>
    public static class States
    {
        public static readonly ReactiveProperty<AgentState> AgentState = new ReactiveProperty<AgentState>();
        public static readonly ReactiveDictionary<int, AvatarState> AvatarStates = new ReactiveDictionary<int, AvatarState>();
        public static readonly ReactiveProperty<AvatarState> CurrentAvatarState = new ReactiveProperty<AvatarState>();
        public static readonly ReactiveProperty<RankingState> RankingState = new ReactiveProperty<RankingState>();
        public static readonly ReactiveProperty<ShopState> ShopState = new ReactiveProperty<ShopState>();

        static States()
        {
            AgentState.Subscribe(AgentSubscribe);
            CurrentAvatarState.Subscribe(CurrentAvatarSubscribe);
            RankingState.Subscribe(RankingSubscribe);
            ShopState.Subscribe(ShopSubscribe);
        }

        private static void AgentSubscribe(AgentState agentState)
        {
            ReactiveAgentState.Initialize(agentState);
        }
        
        private static void CurrentAvatarSubscribe(AvatarState avatarState)
        {
            ReactiveCurrentAvatarState.Initialize(avatarState);
        }
        
        private static void RankingSubscribe(RankingState rankingState)
        {
            ReactiveRankingState.Initialize(rankingState);
        }
        
        private static void ShopSubscribe(ShopState shopState)
        {
            ReactiveShopState.Initialize(shopState);
        }
    }
}
