using System;

namespace SkyClerik
{
    public enum NpcConnorQuest : byte
    {
        Unknown = 0,
        BringBeer = 1,
        CleanGarage = 2,
    }

    [NpcQuestEnum(typeof(NpcConnorQuest))]
    [Serializable]
    public class NpcConnorConfig : NpcConfigBase, IQuestAcceptor<NpcConnorQuest>
    {
        // ВАЖНО прямо тут указать NpcID (Это больше ни где не меняется)
        private NpcID _npcId = NpcID.NPC_ID_CONNOR;
        public override NpcID NpcID => _npcId;

        public bool TryAcceptQuest(NpcConnorQuest id, out QuestAcceptFailedInfo failedInfo) => TryAcceptQuestInternal(QuestKey.FromEnum(id), out failedInfo);

        public bool IsWillRewardsFit(NpcConnorQuest id) => IsWillRewardsFitInternal(QuestKey.FromEnum(id));

        public void CompleteQuest(NpcConnorQuest id) => CompleteQuestInternal(QuestKey.FromEnum(id));
    }
}
