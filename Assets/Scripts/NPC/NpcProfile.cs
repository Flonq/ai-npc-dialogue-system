using UnityEngine;

namespace AINPC.AI
{
    [CreateAssetMenu(fileName = "NewNpcProfile", menuName = "AI NPC/Npc Profile")]
    public class NpcProfile : ScriptableObject
    {
        [Header("Kimlik")]
        public string npcId = "elias";
        public string displayName = "Elias";
        public string role = "yaşlı köy demircisi";
        public string mood = "sakin, biraz yorgun ama nazik";

        [Header("Kişilik")]
        [TextArea(3, 10)]
        public string personality =
            "Yıllarını ocak başında geçirmiş, az konuşan ama lafı geçen biri. " +
            "Yabancılara mesafeli başlar, hak edene yumuşar.";

        [Header("Diyalog")]
        [TextArea(2, 4)]
        public string firstUserMessage =
            "Oyuncu sana yaklaştı ve seninle konuşmaya hazır. " +
            "Karakterinden çıkmadan onu selamla.";
    }
}