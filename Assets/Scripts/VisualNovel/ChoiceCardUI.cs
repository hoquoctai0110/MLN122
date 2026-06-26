using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MLN122.VisualNovel
{
    public sealed class ChoiceCardUI : MonoBehaviour
    {
        [Serializable]
        public sealed class StatChangeRow
        {
            [SerializeField] private Image icon;
            [SerializeField] private TMP_Text labelText;
            [SerializeField] private TMP_Text valueText;

            public StatChangeRow(Image icon, TMP_Text labelText, TMP_Text valueText)
            {
                this.icon = icon;
                this.labelText = labelText;
                this.valueText = valueText;
            }

            public void SetValue(int value, string suffix, bool lowerIsGood, Color positiveColor, Color negativeColor, Color zeroColor)
            {
                if (valueText == null)
                {
                    return;
                }

                string sign = value > 0 ? "+" : string.Empty;
                valueText.text = $"{sign}{value}{suffix}";
                valueText.color = GetChangeColor(value, lowerIsGood, positiveColor, negativeColor, zeroColor);
            }

            private static Color GetChangeColor(int value, bool lowerIsGood, Color positiveColor, Color negativeColor, Color zeroColor)
            {
                if (value == 0)
                {
                    return zeroColor;
                }

                bool goodChange = lowerIsGood ? value < 0 : value > 0;
                return goodChange ? positiveColor : negativeColor;
            }
        }

        [Header("Card")]
        [SerializeField] private Button button;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Vector2 resultRevealSize = new(430f, 520f);

        [Header("Front Side")]
        [SerializeField] private GameObject frontPanel;
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image image;

        [Header("Result Side")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultChoiceTitleText;
        [SerializeField] private GameObject statChangesContainer;
        [SerializeField] private StatChangeRow capitalChangeRow;
        [SerializeField] private StatChangeRow marketShareChangeRow;
        [SerializeField] private StatChangeRow technologyChangeRow;
        [SerializeField] private StatChangeRow reputationChangeRow;
        [SerializeField] private StatChangeRow investigationRiskChangeRow;

        [Header("Change Colors")]
        [SerializeField] private Color positiveColor = new(0.44f, 0.71f, 0.42f, 1f);
        [SerializeField] private Color negativeColor = new(0.79f, 0.34f, 0.27f, 1f);
        [SerializeField] private Color zeroColor = new(0.56f, 0.54f, 0.5f, 1f);

        private int choiceIndex;
        private Action<int> onClicked;
        private ChoiceData boundChoice;

        public RectTransform RectTransform => transform as RectTransform;
        public LayoutElement LayoutElement => GetComponent<LayoutElement>();
        public Vector2 ResultRevealSize => resultRevealSize;

        public void Configure(
            Button cardButton,
            CanvasGroup group,
            GameObject front,
            TMP_Text badge,
            TMP_Text title,
            TMP_Text description,
            Image illustration,
            GameObject result,
            TMP_Text resultTitle,
            TMP_Text resultChoiceTitle,
            GameObject statsContainer,
            StatChangeRow capital,
            StatChangeRow marketShare,
            StatChangeRow technology,
            StatChangeRow reputation,
            StatChangeRow investigationRisk)
        {
            button = cardButton;
            canvasGroup = group;
            frontPanel = front;
            numberText = badge;
            titleText = title;
            descriptionText = description;
            image = illustration;
            resultPanel = result;
            resultTitleText = resultTitle;
            resultChoiceTitleText = resultChoiceTitle;
            statChangesContainer = statsContainer;
            capitalChangeRow = capital;
            marketShareChangeRow = marketShare;
            technologyChangeRow = technology;
            reputationChangeRow = reputation;
            investigationRiskChangeRow = investigationRisk;
        }

        public void Bind(int index, ChoiceData choice, Action<int> clickHandler)
        {
            choiceIndex = index;
            onClicked = clickHandler;
            boundChoice = choice;

            SetActive(gameObject, choice != null);
            if (choice == null)
            {
                return;
            }

            ShowFront();
            SetAlpha(1f);
            SetInteractable(true);

            SetText(numberText, (index + 1).ToString());

            string choiceText = GetChoiceTitle(choice, index);
            SetText(titleText, choiceText);
            SetText(descriptionText, choiceText);
            SetText(resultChoiceTitleText, choiceText);
            SetText(resultTitleText, "K\u1ebeT QU\u1ea2");

            if (choice.Illustration != null && image != null)
            {
                image.sprite = choice.Illustration;
            }
        }

        public void ShowResult()
        {
            ShowResult(boundChoice);
        }

        public void ShowResult(ChoiceData choice)
        {
            DisableClippingMasks();
            SetActive(frontPanel, false);
            SetActive(resultPanel, true);
            SetActive(statChangesContainer, choice?.StatChanges != null);
            SetText(resultTitleText, "K\u1ebeT QU\u1ea2");
            SetText(resultChoiceTitleText, GetChoiceTitle(choice, choiceIndex));

            StatBlockData changes = choice?.StatChanges;
            capitalChangeRow?.SetValue(changes?.Capital ?? 0, string.Empty, false, positiveColor, negativeColor, zeroColor);
            marketShareChangeRow?.SetValue(changes?.MarketShare ?? 0, "%", false, positiveColor, negativeColor, zeroColor);
            technologyChangeRow?.SetValue(changes?.Technology ?? 0, string.Empty, false, positiveColor, negativeColor, zeroColor);
            reputationChangeRow?.SetValue(changes?.Reputation ?? 0, string.Empty, false, positiveColor, negativeColor, zeroColor);
            investigationRiskChangeRow?.SetValue(changes?.InvestigationRisk ?? 0, string.Empty, true, positiveColor, negativeColor, zeroColor);
        }

        public void ShowFront()
        {
            SetActive(frontPanel, true);
            SetActive(resultPanel, false);
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
                if (interactable)
                {
                    button.onClick.AddListener(HandleClicked);
                }

                button.interactable = interactable;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = interactable;
                canvasGroup.interactable = interactable;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }

        public void Clear()
        {
            SetActive(gameObject, false);
            choiceIndex = -1;
            onClicked = null;
            boundChoice = null;
            SetInteractable(false);
            SetAlpha(1f);
            ShowFront();
        }

        private void HandleClicked()
        {
            onClicked?.Invoke(choiceIndex);
        }

        private void DisableClippingMasks()
        {
            Mask[] masks = GetComponentsInChildren<Mask>(true);
            for (int i = 0; i < masks.Length; i++)
            {
                masks[i].enabled = false;
            }

            RectMask2D[] rectMasks = GetComponentsInChildren<RectMask2D>(true);
            for (int i = 0; i < rectMasks.Length; i++)
            {
                rectMasks[i].enabled = false;
            }
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
