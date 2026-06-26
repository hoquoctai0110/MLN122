using System;
using System.Collections;
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
        [SerializeField] private GameObject choiceRevealOverlay;
        [SerializeField] private Button continueButton;

        [Header("Result Popup")]
        [SerializeField] private GameObject resultPopup;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultBodyText;

        [Header("Ending Screen")]
        [SerializeField] private GameObject endingScreen;
        [SerializeField] private TMP_Text endingTitleText;
        [SerializeField] private TMP_Text endingBodyText;
        [SerializeField] private Button restartButton;

        private readonly CardState[] cardStates = new CardState[ChoiceCardCount];
        private Coroutine revealCoroutine;
        private int pendingChoiceIndex = -1;
        private bool isChoiceRevealActive;

        private sealed class CardState
        {
            public Transform Parent;
            public int SiblingIndex;
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
            public Vector3 LocalScale;
            public Quaternion LocalRotation;
            public bool IgnoreLayout;
        }

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
            GameObject revealOverlay,
            Button revealContinueButton,
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
            choiceRevealOverlay = revealOverlay;
            continueButton = revealContinueButton;
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
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(ContinueAfterChoiceReveal);
                continueButton.interactable = false;
                SetActive(continueButton.gameObject, false);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }
        }

        private void ShowRound(GameRoundData round)
        {
            EnsureChoiceCards();
            ResetChoiceRevealState();

            SetActive(resultPopup, false);
            SetActive(endingScreen, false);
            SetActive(darkOverlay, true);

            Debug.Log($"[VisualNovelUI] Round displayed: '{GetRoundTitle(round)}' with {round.Choices.Count} choices.");
            Debug.Log($"[VisualNovelUI] Next round started: '{GetRoundTitle(round)}'.");

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
            ResetChoiceRevealState();
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
            ResetChoiceRevealState();
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
            if (isChoiceRevealActive || gameManager == null || gameManager.CurrentRound == null || choiceIndex >= gameManager.CurrentRound.Choices.Count)
            {
                return;
            }

            ChoiceData choice = gameManager.CurrentRound.Choices[choiceIndex];
            pendingChoiceIndex = choiceIndex;
            isChoiceRevealActive = true;

            Debug.Log($"[VisualNovelUI] Choice clicked: '{GetChoiceTitle(choice, choiceIndex)}'.");
            Debug.Log($"[VisualNovelUI] Selected card index: {choiceIndex}.");

            for (int i = 0; i < choiceCards.Length; i++)
            {
                choiceCards[i]?.SetInteractable(false);
            }

            SetActive(resultPopup, false);

            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
            }

            revealCoroutine = StartCoroutine(PlayChoiceReveal(choiceIndex, choice));
        }

        private void RestartGame()
        {
            ResetChoiceRevealState();
            SetActive(endingScreen, false);
            gameManager?.Restart();
        }

        private IEnumerator PlayChoiceReveal(int selectedIndex, ChoiceData selectedChoice)
        {
            CacheCardStates();
            SetActive(choiceRevealOverlay, true);
            SetActive(continueButton == null ? null : continueButton.gameObject, false);

            ChoiceCardUI selectedCard = choiceCards[selectedIndex];
            RectTransform selectedRect = selectedCard.RectTransform;
            LayoutElement selectedLayout = selectedCard.LayoutElement;
            if (selectedLayout != null)
            {
                selectedLayout.ignoreLayout = true;
            }

            Vector3 selectedWorldPosition = selectedRect.position;
            Vector2 selectedSize = selectedRect.rect.size;
            selectedRect.SetParent(transform, true);
            selectedRect.position = selectedWorldPosition;
            selectedRect.anchorMin = new Vector2(0.5f, 0.5f);
            selectedRect.anchorMax = new Vector2(0.5f, 0.5f);
            selectedRect.pivot = new Vector2(0.5f, 0.5f);
            selectedRect.sizeDelta = selectedSize;
            selectedRect.SetAsLastSibling();

            BringContinueButtonToFront();

            Vector2 startPosition = selectedRect.anchoredPosition;
            Vector2 endPosition = Vector2.zero;
            Vector2 startSize = selectedRect.sizeDelta;
            Vector2 endSize = selectedCard.ResultRevealSize;
            Vector3 startScale = selectedRect.localScale;
            Vector3 endScale = Vector3.one * 1.1f;

            float elapsed = 0f;
            const float fadeDuration = 0.25f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                for (int i = 0; i < choiceCards.Length; i++)
                {
                    if (i != selectedIndex)
                    {
                        choiceCards[i]?.SetAlpha(1f - t);
                    }
                }

                yield return null;
            }

            for (int i = 0; i < choiceCards.Length; i++)
            {
                if (i != selectedIndex)
                {
                    SetActive(choiceCards[i] == null ? null : choiceCards[i].gameObject, false);
                }
            }

            elapsed = 0f;
            const float moveDuration = 0.35f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = SmoothStep(Mathf.Clamp01(elapsed / moveDuration));
                selectedRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
                selectedRect.sizeDelta = Vector2.Lerp(startSize, endSize, t);
                selectedRect.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            selectedRect.anchoredPosition = endPosition;
            selectedRect.sizeDelta = endSize;
            selectedRect.localScale = endScale;

            elapsed = 0f;
            const float halfFlipDuration = 0.18f;
            while (elapsed < halfFlipDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfFlipDuration);
                selectedRect.localScale = new Vector3(Mathf.Lerp(endScale.x, 0f, t), endScale.y, endScale.z);
                yield return null;
            }

            selectedCard.ShowResult(selectedChoice);

            elapsed = 0f;
            while (elapsed < halfFlipDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / halfFlipDuration);
                selectedRect.localScale = new Vector3(Mathf.Lerp(0f, endScale.x, t), endScale.y, endScale.z);
                yield return null;
            }

            selectedRect.localScale = endScale;
            Debug.Log("[VisualNovelUI] Reveal animation finished.");
            ShowContinueButton();
            revealCoroutine = null;
        }

        private void ContinueAfterChoiceReveal()
        {
            Debug.Log("[VisualNovelUI] ContinueButton clicked.");

            if (!isChoiceRevealActive || gameManager == null || gameManager.CurrentRound == null || pendingChoiceIndex < 0)
            {
                return;
            }

            int choiceIndex = pendingChoiceIndex;
            pendingChoiceIndex = -1;
            isChoiceRevealActive = false;
            HideContinueButton();

            Debug.Log($"[VisualNovelUI] Stat changes applied for choice index {choiceIndex}.");
            gameManager.SelectChoice(choiceIndex);
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

        private void CacheCardStates()
        {
            for (int i = 0; i < ChoiceCardCount; i++)
            {
                ChoiceCardUI card = choiceCards[i];
                RectTransform rect = card == null ? null : card.RectTransform;
                if (rect == null)
                {
                    cardStates[i] = null;
                    continue;
                }

                LayoutElement layout = card.LayoutElement;
                cardStates[i] = new CardState
                {
                    Parent = rect.parent,
                    SiblingIndex = rect.GetSiblingIndex(),
                    AnchorMin = rect.anchorMin,
                    AnchorMax = rect.anchorMax,
                    Pivot = rect.pivot,
                    AnchoredPosition = rect.anchoredPosition,
                    SizeDelta = rect.sizeDelta,
                    LocalScale = rect.localScale,
                    LocalRotation = rect.localRotation,
                    IgnoreLayout = layout != null && layout.ignoreLayout
                };
            }
        }

        private void ResetChoiceRevealState()
        {
            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
                revealCoroutine = null;
            }

            SetActive(choiceRevealOverlay, false);
            HideContinueButton();
            pendingChoiceIndex = -1;
            isChoiceRevealActive = false;

            for (int i = 0; i < ChoiceCardCount; i++)
            {
                ChoiceCardUI card = choiceCards[i];
                RectTransform rect = card == null ? null : card.RectTransform;
                CardState state = cardStates[i];
                if (rect != null && state != null)
                {
                    rect.SetParent(state.Parent, false);
                    rect.SetSiblingIndex(state.SiblingIndex);
                    rect.anchorMin = state.AnchorMin;
                    rect.anchorMax = state.AnchorMax;
                    rect.pivot = state.Pivot;
                    rect.anchoredPosition = state.AnchoredPosition;
                    rect.sizeDelta = state.SizeDelta;
                    rect.localScale = state.LocalScale;
                    rect.localRotation = state.LocalRotation;

                    LayoutElement layout = card.LayoutElement;
                    if (layout != null)
                    {
                        layout.ignoreLayout = state.IgnoreLayout;
                    }
                }

                if (card != null)
                {
                    card.SetAlpha(1f);
                    card.ShowFront();
                }

                cardStates[i] = null;
            }
        }

        private static float SmoothStep(float value)
        {
            return value * value * (3f - 2f * value);
        }

        private void ShowContinueButton()
        {
            if (continueButton == null)
            {
                Debug.LogWarning("[VisualNovelUI] Cannot show ContinueButton because no button is assigned.");
                return;
            }

            BringContinueButtonToFront();
            continueButton.interactable = true;
            SetActive(continueButton.gameObject, true);
            Debug.Log("[VisualNovelUI] ContinueButton shown.");
        }

        private void HideContinueButton()
        {
            if (continueButton == null)
            {
                return;
            }

            continueButton.interactable = false;
            SetActive(continueButton.gameObject, false);
        }

        private void BringContinueButtonToFront()
        {
            if (continueButton != null)
            {
                continueButton.transform.SetAsLastSibling();
            }
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
