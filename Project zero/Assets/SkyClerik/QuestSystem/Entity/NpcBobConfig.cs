using System;

namespace SkyClerik
{
    public enum NpcBobQuest : byte
    {
        Unknown = 0,
        OpenMainGate = 1,
        TradeFirstStep = 2,
    }

    [NpcQuestEnum(typeof(NpcBobQuest))]
    [Serializable]
    public class NpcBobConfig : NpcConfigBase, IQuestAcceptor<NpcBobQuest>
    {
        // ВАЖНО прямо тут указать NpcID (Это больше ни где не меняется)
        private NpcID _npcId = NpcID.NPC_ID_BOB;
        public override NpcID NpcID => _npcId;

        public bool TryAcceptQuest(NpcBobQuest id, out QuestAcceptFailedInfo failedInfo) => TryAcceptQuestInternal(QuestKey.FromEnum(id), out failedInfo);

        public bool IsWillRewardsFit(NpcBobQuest id) => IsWillRewardsFitInternal(QuestKey.FromEnum(id));

        public void CompleteQuest(NpcBobQuest id) => CompleteQuestInternal(QuestKey.FromEnum(id));
    }
}
