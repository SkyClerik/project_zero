using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

namespace SkyClerik
{
    public class QuestDescription : VisualElement
    {
        public VisualElement localRoot;
        public VisualElement progessBarParent;
        public ProgressBarExt ProgressBarExt; // Это Instance типа
        public Label questDescription;
        public VisualElement revardArea;

        public QuestDescription(VisualTreeAsset questDescriptionAsset)
        {
            questDescriptionAsset.CloneTree(this);
            var asset = this.ElementAt(0);
            progessBarParent = asset.Q("progess_bar_parent");
            ProgressBarExt = progessBarParent.Q<ProgressBarExt>();
            questDescription = asset.Q<Label>("quest_description");
            revardArea = asset.Q("revard_area");
        }

        public void Init(QuestInfo questInfo)
        {
            questDescription.text = questInfo.QuestDescription;
            revardArea.Clear();

            foreach (ItemReward itemReward in questInfo.RewardItems)
            {
                var itemView = new VisualElement();
                itemView.SetWidthAndHeight(128, 128);
                itemView.SetBorderRadius(20);
                var color = new Color(0f, 0f, 0f, 0.4f);
                itemView.SetBackgroundColor(color);
                itemView.SetBackgroundImage(itemReward.Item.Icon);
                revardArea.Add(itemView);
            }
        }
    }
}
