using UnityEngine;

namespace MLN122.VisualNovel
{
    [CreateAssetMenu(
        fileName = "New Ending",
        menuName = "MLN122/Visual Novel/Ending",
        order = 2)]
    public sealed class EndingData : ScriptableObject
    {
        [SerializeField] private string endingId;
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 8)] private string description;
        [SerializeField] private Sprite illustration;

        public string EndingId => endingId;
        public string Title => title;
        public string Description => description;
        public Sprite Illustration => illustration;
    }
}
