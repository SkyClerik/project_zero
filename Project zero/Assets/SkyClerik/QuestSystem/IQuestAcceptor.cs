using System;

namespace SkyClerik
{
    public interface IQuestAcceptor<TQuestId> where TQuestId : Enum
    {
        bool TryAcceptQuest(TQuestId id, out QuestAcceptFailedInfo failedInfo);
        void CompleteQuest(TQuestId id);
    }
}