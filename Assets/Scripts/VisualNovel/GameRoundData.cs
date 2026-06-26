using System.Collections.Generic;
using UnityEngine;

namespace MLN122.VisualNovel
{
    [CreateAssetMenu(
        fileName = "New Round",
        menuName = "MLN122/Visual Novel/Game Round",
        order = 1)]
    public sealed class GameRoundData : ScriptableObject
    {
        [SerializeField] private string roundId;
        [SerializeField] private int year;
        [SerializeField] private string title;
        [SerializeField] private string speakerName;
        [SerializeField, TextArea(3, 8)] private string story;
        [SerializeField] private Sprite background;
        [SerializeField] private Sprite characterPortrait;
        [SerializeField] private List<ChoiceData> choices = new();

        public string RoundId => roundId;
        public int Year => year;
        public string Title => title;
        public string SpeakerName => speakerName;
        public string Story => story;
        public string DialogueText => story;
        public Sprite Background => background;
        public Sprite CharacterPortrait => characterPortrait;
        public IReadOnlyList<ChoiceData> Choices => choices;
        public bool HasChoices => choices.Count > 0;
    }
}
