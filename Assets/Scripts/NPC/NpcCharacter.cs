using UnityEngine;

namespace AINPC.AI
{
    public class NpcCharacter : MonoBehaviour
    {
        [SerializeField] private NpcProfile profile;

        public NpcProfile Profile => profile;
    }
}