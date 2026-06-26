using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MLN122.VisualNovel
{
    public sealed class VisualNovelUI : MonoBehaviour
    {
        private const int ChoiceCardCount = 3;

        [Serializable]
        public sealed class StatValueRow
        {
            [SerializeField] private Image icon;
            [SerializeField] private TMP_Text labelText;
            [SerializeField] private TMP_Text valueText;

            public StatValueRow(Image icon, TMP_Text labelText, TMP_Text valueText)
            {
                this.icon = icon;
                this.labelText = labelText;
                this.valueText = valueText;
            }

            public void SetValue(string value)
            {
                if (valueText != null)
                {
                    valueText.text = value;
                }
            }
        }

        [Header("Runtime")]
        [SerializeField] private GameManager gameManager;

        [Header("Scene")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject darkOverlay;

        [Header("Header")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text yearText;
        [SerializeField] private TMP_Text titleText;

        [Header("Stats Panel")]
        [SerializeField] private StatValueRow capitalRow;
        [SerializeField] private StatValueRow marketShareRow;
        [SerializeField] private StatValueRow technologyRow;
        [SerializeField] private StatValueRow reputationRow;
        [SerializeField] private StatValueRow investigationRiskRow;

        [Header("Story Panel")]
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image portraitImage;

        [Header("Choices")]
        [SerializeField] private ChoiceCardUI[] choiceCards = new ChoiceCardUI[ChoiceCardCount];

        [Header("Result Popup")]
        [SerializeField] private GameObject resultPopup;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultBodyText;

        [Header("Ending Screen")]
        [SerializeField] private GameObject endingScreen;
        [SerializeField] private TMP_Text endingTitleText;
        [SerializeField] private TMP_Text endingBodyText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            EnsureChoiceCards();
            BindButtons();
        }

        private void OnEnable()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            EnsureChoiceCards();

            if (gameManager != null)
            {
                gameManager.RoundChanged += ShowRound;
                gameManager.EndingReached += ShowEnding;
                gameManager.StatsChanged += ShowStats;

                if (gameManager.CurrentEnding != null)
                {
                    ShowEnding(gameManager.CurrentEnding);
                }
                else if (gameManager.CurrentRound != null)
                {
                    ShowRound(gameManager.CurrentRound);
                }
                else
                {
                    ShowEmptyState();
                }

                ShowStats(gameManager.CurrentStats);
            }
            else
            {
                ShowEmptyState();
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.RoundChanged -= ShowRound;
                gameManager.EndingReached -= ShowEnding;
                gameManager.StatsChanged -= ShowStats;
            }
        }

        public void Configure(
            GameManager manager,
            Image background,
            GameObject overlay,
            TMP_Text progress,
            TMP_Text year,
            TMP_Text title,
            StatValueRow capital,
            StatValueRow marketShare,
            StatValueRow technology,
            StatValueRow reputation,
            StatValueRow investigationRisk,
            TMP_Text speaker,
            TMP_Text dialogue,
            Image portrait,
            ChoiceCardUI[] cards,
            GameObject popup,
            TMP_Text popupTitle,
            TMP_Text popupBody,
            GameObject ending,
            TMP_Text endingTitle,
            TMP_Text endingBody,
            Button restart)
        {
            gameManager = manager;
            backgroundImage = background;
            darkOverlay = overlay;
            progressText = progress;
            yearText = year;
            titleText = title;
            capitalRow = capital;
            marketShareRow = marketShare;
            technologyRow = technology;
            reputationRow = reputation;
            investigationRiskRow = investigationRisk;
            speakerText = speaker;
            dialogueText = dialogue;
            portraitImage = portrait;
            choiceCards = NormalizeChoiceCards(cards);
            resultPopup = popup;
            resultTitleText = popupTitle;
            resultBodyText = popupBody;
            endingScreen = ending;
            endingTitleText = endingTitle;
            endingBodyText = endingBody;
            restartButton = restart;

            BindButtons();
        }

        private void BindButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }
        }

        private void ShowRound(GameRoundData round)
        {
            EnsureChoiceCards();

            SetActive(resultPopup, false);
            SetActive(endingScreen, false);
            SetActive(darkOverlay, true);

            Debug.Log($"[VisualNovelUI] Showing round '{GetRoundTitle(round)}' with {round.Choices.Count} choices.");

            int currentRound = gameManager == null ? 0 : gameManager.CurrentRoundNumber;
            int totalRounds = gameManager == null ? 0 : gameManager.LoadedRoundCount;
            SetText(progressText, totalRounds > 0 ? $"V\u00d2NG {currentRound}/{totalRounds}" : "V\u00d2NG");
            SetText(yearText, round.Year > 0 ? round.Year.ToString() : "N\u0103m kh\u00f4ng r\u00f5");
            SetText(titleText, string.IsNullOrWhiteSpace(round.Title) ? "Untitled Round" : round.Title);
            SetText(speakerText, string.IsNullOrWhiteSpace(round.SpeakerName) ? "John D. Rockefeller" : round.SpeakerName);
            SetText(dialogueText, string.IsNullOrWhiteSpace(round.Story) ? "No story has been written for this round yet." : round.Story);

            if (backgroundImage != null)
            {
                backgroundImage.sprite = round.Background;
                backgroundImage.color = round.Background == null ? new Color(0.025f, 0.035f, 0.047f, 1f) : Color.white;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = round.CharacterPortrait;
                portraitImage.color = round.CharacterPortrait == null ? new Color(0.27f, 0.22f, 0.16f, 1f) : Color.white;
            }

            for (int i = 0; i < ChoiceCardCount; i++)
            {
                if (choiceCards[i] == null)
                {
                    Debug.LogWarning($"[VisualNovelUI] Card {i + 1} has no ChoiceCardUI reference.");
                    continue;
                }

                if (i < round.Choices.Count)
                {
                    choiceCards[i].Bind(i, round.Choices[i], SelectChoice);
                    Debug.Log($"[VisualNovelUI] Card {i + 1} assigned choice[{i}] '{GetChoiceTitle(round.Choices[i], i)}'.");
                }
                else
                {
                    choiceCards[i].Clear();
                    Debug.Log($"[VisualNovelUI] Card {i + 1} cleared because no choice[{i}] exists.");
                }
            }
        }

        private void ShowEnding(EndingData ending)
        {
            SetActive(resultPopup, false);
            SetActive(endingScreen, true);
            SetActive(darkOverlay, true);

            SetText(progressText, "K\u1ebeT TH\u00daC");
            SetText(yearText, string.Empty);
            SetText(titleText, ending == null || string.IsNullOrWhiteSpace(ending.Title) ? "Ending" : ending.Title);
            SetText(endingTitleText, ending == null || string.IsNullOrWhiteSpace(ending.Title) ? "Ending" : ending.Title);
            SetText(endingBodyText, ending == null || string.IsNullOrWhiteSpace(ending.Description) ? "The Standard Oil timeline has ended." : ending.Description);
        }

        private void ShowEmptyState()
        {
            SetActive(resultPopup, false);
            SetActive(endingScreen, false);
            SetActive(darkOverlay, true);

            SetText(progressText, "V\u00d2NG 0/0");
            SetText(yearText, string.Empty);
            SetText(titleText, "Visual Novel");
            SetText(speakerText, "John D. Rockefeller");
            SetText(dialogueText, "Assign a starting round to the GameManager to begin.");
            SetStatsValues(0, 0, 0, 0, 0);

            foreach (ChoiceCardUI choiceCard in choiceCards)
            {
                choiceCard?.Clear();
            }
        }

        private void SelectChoice(int choiceIndex)
        {
            if (gameManager == null || gameManager.CurrentRound == null || choiceIndex >= gameManager.CurrentRound.Choices.Count)
            {
                return;
            }

            ChoiceData choice = gameManager.CurrentRound.Choices[choiceIndex];
            gameManager.SelectChoice(choiceIndex);

            if (gameManager.CurrentEnding == null)
            {
                SetActive(resultPopup, true);
                SetText(resultTitleText, "Quy\u1ebft \u0111\u1ecbnh \u0111\u00e3 ch\u1ecdn");
                string resultText = string.IsNullOrWhiteSpace(choice.ResultText) ? choice.Text : choice.ResultText;
                SetText(resultBodyText, $"{resultText}\n\n{choice.StatChanges.ToChangeDisplayString()}");
            }
        }

        private void RestartGame()
        {
            SetActive(endingScreen, false);
            gameManager?.Restart();
        }

        private void ShowStats(StatBlockData stats)
        {
            if (stats == null)
            {
                return;
            }

            SetStatsValues(
                stats.Capital,
                stats.MarketShare,
                stats.Technology,
                stats.Reputation,
                stats.InvestigationRisk);
        }

        private void SetStatsValues(int capital, int marketShare, int technology, int reputation, int investigationRisk)
        {
            capitalRow?.SetValue(capital.ToString());
            marketShareRow?.SetValue($"{marketShare}%");
            technologyRow?.SetValue(technology.ToString());
            reputationRow?.SetValue(reputation.ToString());
            investigationRiskRow?.SetValue(investigationRisk.ToString());
        }

        private void EnsureChoiceCards()
        {
            choiceCards = NormalizeChoiceCards(choiceCards);

            bool hasMissingReference = false;
            for (int i = 0; i < choiceCards.Length; i++)
            {
                if (choiceCards[i] == null)
                {
                    hasMissingReference = true;
                    break;
                }
            }

            if (!hasMissingReference)
            {
                return;
            }

            ChoiceCardUI[] discoveredCards = GetComponentsInChildren<ChoiceCardUI>(true);
            choiceCards = NormalizeChoiceCards(discoveredCards);
        }

        private static ChoiceCardUI[] NormalizeChoiceCards(ChoiceCardUI[] cards)
        {
            ChoiceCardUI[] normalizedCards = new ChoiceCardUI[ChoiceCardCount];
            if (cards == null)
            {
                return normalizedCards;
            }

            int assignedCount = 0;
            for (int i = 0; i < cards.Length && assignedCount < ChoiceCardCount; i++)
            {
                if (cards[i] == null || Contains(normalizedCards, cards[i], assignedCount))
                {
                    continue;
                }

                normalizedCards[assignedCount] = cards[i];
                assignedCount++;
            }

            return normalizedCards;
        }

        private static bool Contains(ChoiceCardUI[] cards, ChoiceCardUI card, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (cards[i] == card)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetRoundTitle(GameRoundData round)
        {
            if (round == null)
            {
                return "Null Round";
            }

            return string.IsNullOrWhiteSpace(round.Title) ? round.name : round.Title;
        }

        private static string GetChoiceTitle(ChoiceData choice, int index)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.Text))
            {
                return $"Choice {index + 1}";
            }

            return choice.Text;
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
