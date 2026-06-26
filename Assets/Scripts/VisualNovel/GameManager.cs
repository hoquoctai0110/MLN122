using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLN122.VisualNovel
{
    public sealed class GameManager : MonoBehaviour
    {
        private const int StartingCapital = 100;
        private const int StartingMarketShare = 4;
        private const int StartingTechnology = 1;
        private const int StartingReputation = 1;
        private const int StartingInvestigationRisk = 0;
        private const string FinalJudgementRoundId = "standard_oil_1911_breakup";

        [SerializeField] private GameRoundData startingRound;
        [SerializeField] private bool autoLoadRounds = true;
        [SerializeField] private string roundsResourcePath = "StandardOil/Rounds";
        [SerializeField] private EndingData rockefellerNewEnding;
        [SerializeField] private EndingData empireTooBigEnding;
        [SerializeField] private EndingData victimOfCompetitionEnding;
        public event Action<GameRoundData> RoundChanged;
        public event Action<EndingData> EndingReached;
        public event Action<StatBlockData> StatsChanged;

        public GameRoundData CurrentRound { get; private set; }
        public EndingData CurrentEnding { get; private set; }
        public StatBlockData CurrentStats { get; private set; } = CreateStartingStats();
        public bool IsRunning => CurrentRound != null && CurrentEnding == null;
        public int LoadedRoundCount => loadedRounds.Count;
        public int CurrentRoundNumber => currentRoundIndex >= 0 ? currentRoundIndex + 1 : 0;

        private readonly List<GameRoundData> loadedRounds = new();
        private int currentRoundIndex = -1;

        private void Awake()
        {
            LoadConfiguredData();
        }

        public void StartGame()
        {
            CurrentStats = CreateStartingStats();
            CurrentEnding = null;
            StatsChanged?.Invoke(CurrentStats);

            GameRoundData firstRound = startingRound != null ? startingRound : loadedRounds.FirstOrDefault();
            SetRound(firstRound);
        }

        public void Restart()
        {
            StartGame();
        }

        public void ResetToHome()
        {
            CurrentStats = CreateStartingStats();
            CurrentRound = null;
            CurrentEnding = null;
            currentRoundIndex = -1;
            StatsChanged?.Invoke(CurrentStats);
            Debug.Log("[GameManager] Game reset.");
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!IsRunning)
            {
                Debug.LogWarning("Cannot select a choice because the visual novel game is not running.");
                return;
            }

            if (choiceIndex < 0 || choiceIndex >= CurrentRound.Choices.Count)
            {
                Debug.LogWarning($"Choice index {choiceIndex} is out of range for round '{CurrentRound.RoundId}'.");
                return;
            }

            SelectChoice(CurrentRound.Choices[choiceIndex]);
        }

        public void SelectChoice(ChoiceData choice)
        {
            if (choice == null)
            {
                Debug.LogWarning("Cannot select a null visual novel choice.");
                return;
            }

            if (choice.HasEnding)
            {
                ApplyStatChanges(choice);
                ReachEnding(choice.Ending);
                return;
            }

            if (choice.HasNextRound)
            {
                ApplyStatChanges(choice);
                SetRound(choice.NextRound);
                return;
            }

            ApplyStatChanges(choice);
            AdvanceToNextLoadedRound();
        }

        public void SetRound(GameRoundData round)
        {
            if (round == null)
            {
                Debug.LogWarning("Cannot set a null visual novel round.");
                CurrentRound = null;
                return;
            }

            CurrentRound = round;
            CurrentEnding = null;
            currentRoundIndex = loadedRounds.IndexOf(round);
            RoundChanged?.Invoke(CurrentRound);
        }

        public void ReachEnding(EndingData ending)
        {
            if (ending == null)
            {
                Debug.LogWarning("Cannot reach a null visual novel ending.");
                return;
            }

            CurrentRound = null;
            CurrentEnding = ending;
            EndingReached?.Invoke(CurrentEnding);
        }

        private void LoadConfiguredData()
        {
            if (autoLoadRounds)
            {
                loadedRounds.Clear();
                loadedRounds.AddRange(Resources.LoadAll<GameRoundData>(roundsResourcePath)
                    .OrderBy(round => round.Year)
                    .ThenBy(round => round.name));
            }

            rockefellerNewEnding ??= Resources.Load<EndingData>("StandardOil/Endings/Rockefeller_New");
            empireTooBigEnding ??= Resources.Load<EndingData>("StandardOil/Endings/Empire_Too_Big");
            victimOfCompetitionEnding ??= Resources.Load<EndingData>("StandardOil/Endings/Victim_Of_Competition");
        }

        private void ApplyStatChanges(ChoiceData choice)
        {
            CurrentStats.Add(choice.StatChanges);
            StatsChanged?.Invoke(CurrentStats);
        }

        private static StatBlockData CreateStartingStats()
        {
            return new StatBlockData(
                StartingCapital,
                StartingMarketShare,
                StartingTechnology,
                StartingReputation,
                StartingInvestigationRisk);
        }

        private void AdvanceToNextLoadedRound()
        {
            if (loadedRounds.Count == 0)
            {
                Debug.LogWarning($"Choice '{CurrentRound?.RoundId}' does not point to another round or an ending, and no rounds are loaded.");
                return;
            }

            int nextIndex = currentRoundIndex + 1;
            if (nextIndex >= 0 && nextIndex < loadedRounds.Count)
            {
                GameRoundData nextRound = loadedRounds[nextIndex];
                if (IsFinalJudgementRound(nextRound))
                {
                    ReachEnding(SelectFinalJudgementEnding());
                    return;
                }

                SetRound(nextRound);
                return;
            }

            ReachEnding(SelectFinalJudgementEnding());
        }

        private static bool IsFinalJudgementRound(GameRoundData round)
        {
            return round != null
                && round.Year == 1911
                && round.RoundId == FinalJudgementRoundId
                && !round.HasChoices;
        }

        private EndingData SelectFinalJudgementEnding()
        {
            if (CurrentStats.Capital <= 0 || CurrentStats.MarketShare < 50)
            {
                return victimOfCompetitionEnding;
            }

            if (CurrentStats.MarketShare >= 80)
            {
                return empireTooBigEnding;
            }

            return rockefellerNewEnding;
        }
    }
}
