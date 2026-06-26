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
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image image;

        [Header("Stat Changes")]
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

        public void Configure(
            Button cardButton,
            TMP_Text badge,
            TMP_Text title,
            TMP_Text description,
            Image illustration,
            GameObject statsContainer,
            StatChangeRow capital,
            StatChangeRow marketShare,
            StatChangeRow technology,
            StatChangeRow reputation,
            StatChangeRow investigationRisk)
        {
            button = cardButton;
            numberText = badge;
            titleText = title;
            descriptionText = description;
            image = illustration;
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

            SetActive(gameObject, choice != null);
            if (choice == null)
            {
                return;
            }

            SetText(numberText, (index + 1).ToString());

            string choiceText = string.IsNullOrWhiteSpace(choice.Text) ? $"Choice {index + 1}" : choice.Text;
            SetText(titleText, choiceText);
            SetText(descriptionText, choiceText);

            if (statChangesContainer != null)
            {
                statChangesContainer.SetActive(choice.StatChanges != null);
            }

            StatBlockData changes = choice.StatChanges;
            capitalChangeRow?.SetValue(changes?.Capital ?? 0, string.Empty, false, positiveColor, negativeColor, zeroColor);
            marketShareChangeRow?.SetValue(changes?.MarketShare ?? 0, "%", false, positiveColor, negativeColor, zeroColor);
            technologyChangeRow?.SetValue(changes?.Technology ?? 0, string.Empty, false, positiveColor, negativeColor, zeroColor);
            reputationChangeRow?.SetValue(changes?.Reputation ?? 0, string.Empty, false, positiveColor, negativeColor, zeroColor);
            investigationRiskChangeRow?.SetValue(changes?.InvestigationRisk ?? 0, string.Empty, true, positiveColor, negativeColor, zeroColor);

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
                button.onClick.AddListener(HandleClicked);
                button.interactable = true;
            }
        }

        public void Clear()
        {
            SetActive(gameObject, false);
            choiceIndex = -1;
            onClicked = null;

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            onClicked?.Invoke(choiceIndex);
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
