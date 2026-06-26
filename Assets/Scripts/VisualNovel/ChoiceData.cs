using System;
using UnityEngine;

namespace MLN122.VisualNovel
{
    [Serializable]
    public sealed class ChoiceData
    {
        [SerializeField] private string choiceId;
        [SerializeField, TextArea(1, 3)] private string text;
        [SerializeField] private Sprite illustration;
        [SerializeField, TextArea(1, 3)] private string resultText;
        [SerializeField] private StatBlockData statChanges = new();
        [SerializeField] private GameRoundData nextRound;
        [SerializeField] private EndingData ending;

        public string ChoiceId => choiceId;
        public string Text => text;
        public Sprite Illustration => illustration;
        public string ResultText => resultText;
        public StatBlockData StatChanges => statChanges;
        public GameRoundData NextRound => nextRound;
        public EndingData Ending => ending;
        public bool HasEnding => ending != null;
        public bool HasNextRound => nextRound != null;
    }
}
