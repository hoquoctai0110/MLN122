using System;
using MLN122.VisualNovel;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MLN122.VisualNovelEditor
{
    public static class VisualNovelUIBuilder
    {
        private const string RootName = "Visual Novel UI";

        private static readonly Color Ink = new(0.1f, 0.075f, 0.045f, 1f);
        private static readonly Color Paper = new(0.78f, 0.67f, 0.49f, 0.97f);
        private static readonly Color PaperDark = new(0.55f, 0.43f, 0.28f, 0.98f);
        private static readonly Color Gold = new(0.84f, 0.62f, 0.29f, 1f);
        private static readonly Color Bronze = new(0.45f, 0.26f, 0.11f, 1f);
        private static readonly Color Navy = new(0.018f, 0.028f, 0.04f, 1f);

        [MenuItem("MLN122/Visual Novel/Build Complete UI")]
        public static void BuildCompleteUI()
        {
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
            }

            GameManager gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                GameObject managerObject = new GameObject("GameManager");
                Undo.RegisterCreatedObjectUndo(managerObject, "Create GameManager");
                gameManager = managerObject.AddComponent<GameManager>();
            }

            EnsureEventSystem();

            GameObject root = CreateObject(RootName, null);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            Image background = CreateImage("Industrial Background", root.transform, Navy);
            Stretch(background.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            background.preserveAspect = true;

            CreateIndustrialBands(root.transform);

            Image darkOverlay = CreateImage("Dark Overlay", root.transform, new Color(0f, 0f, 0f, 0.26f));
            Stretch(darkOverlay.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject header = CreateObject("Header", root.transform);
            Stretch(header.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(520f, -152f), new Vector2(-520f, -28f));
            VerticalLayoutGroup headerLayout = header.AddComponent<VerticalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.UpperCenter;
            headerLayout.spacing = 2f;
            headerLayout.childControlHeight = false;
            headerLayout.childControlWidth = true;

            TMP_Text progressText = CreateText("Progress Text", header.transform, "VÒNG 1/10", 30, FontStyles.Bold, TextAlignmentOptions.Center, Gold);
            AddLayout(progressText.gameObject, 0f, 34f, 1f);

            TMP_Text yearText = CreateText("Year Text", header.transform, "1870", 56, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.96f, 0.88f, 0.68f, 1f));
            AddLayout(yearText.gameObject, 0f, 58f, 1f);

            TMP_Text titleText = CreateText("Round Title Text", header.transform, "Round Title", 42, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.92f, 0.82f, 0.58f, 1f));
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 34f;
            titleText.fontSizeMax = 42f;
            AddLayout(titleText.gameObject, 0f, 48f, 1f);

            Image statsPanel = CreateBorderedPanel("Stats Panel", root.transform, new Color(0.045f, 0.055f, 0.058f, 0.94f));
            Stretch(statsPanel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(36f, 392f), new Vector2(376f, -182f));

            VerticalLayoutGroup statsLayout = statsPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            statsLayout.padding = new RectOffset(24, 24, 24, 24);
            statsLayout.spacing = 16f;
            statsLayout.childControlWidth = true;
            statsLayout.childControlHeight = false;
            statsLayout.childAlignment = TextAnchor.UpperLeft;

            TMP_Text statsHeader = CreateText("Stats Header", statsPanel.transform, "STANDARD OIL", 28, FontStyles.Bold, TextAlignmentOptions.Left, Gold);
            AddLayout(statsHeader.gameObject, 0f, 40f, 1f);

            GameObject statsRows = CreateObject("Stats Rows", statsPanel.transform);
            AddLayout(statsRows, 0f, 250f, 1f);
            VerticalLayoutGroup statsRowsLayout = statsRows.AddComponent<VerticalLayoutGroup>();
            statsRowsLayout.spacing = 12f;
            statsRowsLayout.childControlWidth = true;
            statsRowsLayout.childControlHeight = false;

            VisualNovelUI.StatValueRow capitalRow = CreateInfoStatRow("CapitalRow", statsRows.transform, "V\u1ed1n", "100", new Color(0.9f, 0.68f, 0.26f, 1f));
            VisualNovelUI.StatValueRow marketShareRow = CreateInfoStatRow("MarketShareRow", statsRows.transform, "Th\u1ecb ph\u1ea7n", "4%", new Color(0.52f, 0.73f, 0.48f, 1f));
            VisualNovelUI.StatValueRow technologyRow = CreateInfoStatRow("TechnologyRow", statsRows.transform, "C\u00f4ng ngh\u1ec7", "1", new Color(0.44f, 0.65f, 0.82f, 1f));
            VisualNovelUI.StatValueRow reputationRow = CreateInfoStatRow("ReputationRow", statsRows.transform, "Uy t\u00edn", "1", new Color(0.86f, 0.76f, 0.3f, 1f));
            VisualNovelUI.StatValueRow investigationRiskRow = CreateInfoStatRow("InvestigationRiskRow", statsRows.transform, "Nguy c\u01a1 \u0111i\u1ec1u tra", "0", new Color(0.76f, 0.36f, 0.28f, 1f));

            Image portraitFrame = CreateBorderedPanel("Rockefeller Portrait Frame", root.transform, PaperDark);
            Stretch(portraitFrame.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(424f, -570f), new Vector2(704f, -182f));

            Image portraitImage = CreateImage("Portrait Placeholder", portraitFrame.transform, new Color(0.22f, 0.17f, 0.11f, 1f));
            Stretch(portraitImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 60f), new Vector2(-16f, -16f));
            CreatePortraitMonogram(portraitImage.transform);

            TMP_Text portraitLabel = CreateText("Portrait Label", portraitFrame.transform, "J. D. ROCKEFELLER", 22, FontStyles.Bold, TextAlignmentOptions.Center, Ink);
            Stretch(portraitLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 14f), new Vector2(-14f, -318f));

            Image storyPanel = CreateBorderedPanel("Story Paper Panel", root.transform, Paper);
            Stretch(storyPanel.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(736f, -570f), new Vector2(-54f, -182f));

            VerticalLayoutGroup storyLayout = storyPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            storyLayout.padding = new RectOffset(34, 34, 30, 28);
            storyLayout.spacing = 12f;
            storyLayout.childControlWidth = true;
            storyLayout.childControlHeight = false;

            TMP_Text speakerText = CreateText("Speaker Text", storyPanel.transform, "John D. Rockefeller", 30, FontStyles.Bold, TextAlignmentOptions.Left, Bronze);
            AddLayout(speakerText.gameObject, 0f, 42f, 1f);

            TMP_Text dialogueText = CreateText("Story Text", storyPanel.transform, "Story appears here.", 31, FontStyles.Normal, TextAlignmentOptions.TopLeft, Ink);
            dialogueText.textWrappingMode = TextWrappingModes.Normal;
            dialogueText.overflowMode = TextOverflowModes.Ellipsis;
            dialogueText.lineSpacing = 5f;
            AddLayout(dialogueText.gameObject, 0f, 280f, 1f);

            GameObject cardRow = CreateObject("Choice Card Row", root.transform);
            Stretch(cardRow.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(54f, 34f), new Vector2(-54f, 394f));
            HorizontalLayoutGroup cardLayout = cardRow.AddComponent<HorizontalLayoutGroup>();
            cardLayout.padding = new RectOffset(0, 0, 0, 0);
            cardLayout.spacing = 26f;
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = true;

            ChoiceCardUI[] choiceCards = new ChoiceCardUI[3];
            for (int i = 0; i < choiceCards.Length; i++)
            {
                choiceCards[i] = CreateChoiceCard(cardRow.transform, i + 1);
            }

            Image resultPopup = CreateBorderedPanel("Result Popup", root.transform, new Color(0.08f, 0.075f, 0.06f, 0.97f));
            Stretch(resultPopup.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-360f, -138f), new Vector2(360f, 138f));

            TMP_Text resultTitle = CreateText("Result Title Text", resultPopup.transform, "Quyết định đã chọn", 34, FontStyles.Bold, TextAlignmentOptions.Center, Gold);
            Stretch(resultTitle.rectTransform, Vector2.zero, Vector2.one, new Vector2(30f, -76f), new Vector2(-30f, -24f));

            TMP_Text resultBody = CreateText("Result Body Text", resultPopup.transform, "Result text appears here.", 25, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.94f, 0.88f, 0.72f, 1f));
            resultBody.textWrappingMode = TextWrappingModes.Normal;
            Stretch(resultBody.rectTransform, Vector2.zero, Vector2.one, new Vector2(34f, 28f), new Vector2(-34f, -92f));
            resultPopup.gameObject.SetActive(false);

            Image endingScreen = CreateBorderedPanel("Ending Screen", root.transform, new Color(0.015f, 0.018f, 0.024f, 0.98f));
            Stretch(endingScreen.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            TMP_Text endingTitle = CreateText("Ending Title Text", endingScreen.transform, "Ending", 56, FontStyles.Bold, TextAlignmentOptions.Center, Gold);
            Stretch(endingTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(180f, -260f), new Vector2(-180f, -140f));

            TMP_Text endingBody = CreateText("Ending Body Text", endingScreen.transform, "Ending description appears here.", 32, FontStyles.Normal, TextAlignmentOptions.Top, new Color(0.94f, 0.88f, 0.72f, 1f));
            endingBody.textWrappingMode = TextWrappingModes.Normal;
            Stretch(endingBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-560f, -128f), new Vector2(560f, 128f));

            Button restartButton = CreateButton("Restart Button", endingScreen.transform, "Restart");
            Stretch(restartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150f, 96f), new Vector2(150f, 164f));
            endingScreen.gameObject.SetActive(false);

            VisualNovelUI visualNovelUI = root.AddComponent<VisualNovelUI>();
            visualNovelUI.Configure(
                gameManager,
                background,
                darkOverlay.gameObject,
                progressText,
                yearText,
                titleText,
                capitalRow,
                marketShareRow,
                technologyRow,
                reputationRow,
                investigationRiskRow,
                speakerText,
                dialogueText,
                portraitImage,
                choiceCards,
                resultPopup.gameObject,
                resultTitle,
                resultBody,
                endingScreen.gameObject,
                endingTitle,
                endingBody,
                restartButton);

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static ChoiceCardUI CreateChoiceCard(Transform parent, int number)
        {
            Image cardImage = CreateBorderedPanel($"Choice Card {number}", parent, new Color(0.12f, 0.095f, 0.06f, 0.98f));
            Button button = cardImage.gameObject.AddComponent<Button>();
            button.targetGraphic = cardImage;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.9f, 0.62f, 1f);
            colors.pressedColor = new Color(0.78f, 0.55f, 0.26f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.colorMultiplier = 1f;
            button.colors = colors;

            AddLayout(cardImage.gameObject, 360f, 360f, 1f);

            VerticalLayoutGroup cardLayout = cardImage.gameObject.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(18, 18, 18, 18);
            cardLayout.spacing = 9f;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = false;

            GameObject topRow = CreateObject("Card Top Row", cardImage.transform);
            AddLayout(topRow, 0f, 82f, 1f);

            Image illustration = CreateImage("Illustration", topRow.transform, new Color(0.24f, 0.19f, 0.13f, 1f));
            Stretch(illustration.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(0f, 0f));
            CreateCardIcon(illustration.transform);

            Image badge = CreateImage("Number Badge", topRow.transform, Bronze);
            Stretch(badge.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -54f), new Vector2(58f, -8f));

            TMP_Text numberText = CreateText("Number Text", badge.transform, number.ToString(), 26, FontStyles.Bold, TextAlignmentOptions.Center, Gold);
            Stretch(numberText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            TMP_Text titleText = CreateText("Card Title Text", cardImage.transform, $"Choice {number}", 29, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.95f, 0.86f, 0.62f, 1f));
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 23f;
            titleText.fontSizeMax = 29f;
            titleText.textWrappingMode = TextWrappingModes.Normal;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            AddLayout(titleText.gameObject, 0f, 52f, 1f);

            TMP_Text descriptionText = CreateText("Card Description Text", cardImage.transform, string.Empty, 21, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.83f, 0.77f, 0.65f, 1f));
            descriptionText.textWrappingMode = TextWrappingModes.Normal;
            descriptionText.overflowMode = TextOverflowModes.Ellipsis;
            AddLayout(descriptionText.gameObject, 0f, 26f, 1f);

            Image statsBox = CreateImage("Stat Changes Box", cardImage.transform, new Color(0.055f, 0.06f, 0.058f, 0.92f));
            AddLayout(statsBox.gameObject, 0f, 126f, 1f);
            VerticalLayoutGroup statsLayout = statsBox.gameObject.AddComponent<VerticalLayoutGroup>();
            statsLayout.padding = new RectOffset(12, 12, 8, 8);
            statsLayout.spacing = 3f;
            statsLayout.childControlWidth = true;
            statsLayout.childControlHeight = false;

            ChoiceCardUI.StatChangeRow capital = CreateChoiceStatRow("CapitalChangeRow", statsBox.transform, "V\u1ed1n", "0", new Color(0.9f, 0.68f, 0.26f, 1f));
            ChoiceCardUI.StatChangeRow marketShare = CreateChoiceStatRow("MarketShareChangeRow", statsBox.transform, "Th\u1ecb ph\u1ea7n", "0%", new Color(0.52f, 0.73f, 0.48f, 1f));
            ChoiceCardUI.StatChangeRow technology = CreateChoiceStatRow("TechnologyChangeRow", statsBox.transform, "C\u00f4ng ngh\u1ec7", "0", new Color(0.44f, 0.65f, 0.82f, 1f));
            ChoiceCardUI.StatChangeRow reputation = CreateChoiceStatRow("ReputationChangeRow", statsBox.transform, "Uy t\u00edn", "0", new Color(0.86f, 0.76f, 0.3f, 1f));
            ChoiceCardUI.StatChangeRow investigationRisk = CreateChoiceStatRow("InvestigationRiskChangeRow", statsBox.transform, "Nguy c\u01a1 \u0111i\u1ec1u tra", "0", new Color(0.76f, 0.36f, 0.28f, 1f));

            ChoiceCardUI card = cardImage.gameObject.AddComponent<ChoiceCardUI>();
            card.Configure(
                button,
                numberText,
                titleText,
                descriptionText,
                illustration,
                statsBox.gameObject,
                capital,
                marketShare,
                technology,
                reputation,
                investigationRisk);

            return card;
        }

        private static VisualNovelUI.StatValueRow CreateInfoStatRow(string name, Transform parent, string label, string value, Color iconColor)
        {
            CreateStatRowObjects(name, parent, label, value, iconColor, 26, 24f, 150f, 0f, 78f, 24f, out Image icon, out TMP_Text labelText, out TMP_Text valueText);
            return new VisualNovelUI.StatValueRow(icon, labelText, valueText);
        }

        private static ChoiceCardUI.StatChangeRow CreateChoiceStatRow(string name, Transform parent, string label, string value, Color iconColor)
        {
            CreateStatRowObjects(name, parent, label, value, iconColor, 20, 36f, 72f, 1f, 104f, 18f, out Image icon, out TMP_Text labelText, out TMP_Text valueText);
            return new ChoiceCardUI.StatChangeRow(icon, labelText, valueText);
        }

        private static void CreateStatRowObjects(
            string name,
            Transform parent,
            string label,
            string value,
            Color iconColor,
            int fontSize,
            float iconWidth,
            float labelPreferredWidth,
            float labelFlexibleWidth,
            float valueWidth,
            float rowHeight,
            out Image icon,
            out TMP_Text labelText,
            out TMP_Text valueText)
        {
            GameObject row = CreateObject(name, parent);
            AddLayout(row, 0f, rowHeight, 1f);

            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;

            icon = CreateImage("Icon", row.transform, iconColor);
            AddFixedLayout(icon.gameObject, iconWidth, rowHeight);
            Outline iconOutline = icon.gameObject.AddComponent<Outline>();
            iconOutline.effectColor = Gold;
            iconOutline.effectDistance = new Vector2(1f, -1f);

            labelText = CreateText("LabelText", row.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, new Color(0.93f, 0.86f, 0.68f, 1f));
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            labelText.overflowMode = TextOverflowModes.Ellipsis;
            AddLayout(labelText.gameObject, labelPreferredWidth, rowHeight, labelFlexibleWidth);

            valueText = CreateText("ValueText", row.transform, value, fontSize, FontStyles.Bold, TextAlignmentOptions.MidlineRight, new Color(0.96f, 0.91f, 0.76f, 1f));
            valueText.textWrappingMode = TextWrappingModes.NoWrap;
            valueText.overflowMode = TextOverflowModes.Ellipsis;
            AddFixedLayout(valueText.gameObject, valueWidth, rowHeight);
        }

        private static void CreateIndustrialBands(Transform parent)
        {
            for (int i = 0; i < 6; i++)
            {
                Image band = CreateImage($"Smoky Band {i + 1}", parent, new Color(0.08f + i * 0.01f, 0.075f, 0.065f, 0.17f));
                RectTransform rect = band.rectTransform;
                rect.anchorMin = new Vector2(0f, 0.08f + i * 0.13f);
                rect.anchorMax = new Vector2(1f, 0.14f + i * 0.13f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        private static void CreatePortraitMonogram(Transform parent)
        {
            TMP_Text monogram = CreateText("Portrait Monogram", parent, "JD", 74, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.79f, 0.65f, 0.42f, 0.7f));
            Stretch(monogram.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image line = CreateImage("Portrait Separator", parent, new Color(0.79f, 0.65f, 0.42f, 0.45f));
            Stretch(line.rectTransform, new Vector2(0.18f, 0.35f), new Vector2(0.82f, 0.35f), new Vector2(0f, -2f), new Vector2(0f, 2f));
        }

        private static void CreateCardIcon(Transform parent)
        {
            Image baseLine = CreateImage("Factory Base", parent, new Color(0.72f, 0.55f, 0.32f, 0.72f));
            Stretch(baseLine.rectTransform, new Vector2(0.16f, 0.22f), new Vector2(0.84f, 0.22f), new Vector2(0f, -3f), new Vector2(0f, 3f));

            for (int i = 0; i < 3; i++)
            {
                Image stack = CreateImage($"Smokestack {i + 1}", parent, new Color(0.72f, 0.55f, 0.32f, 0.65f));
                float x = 0.24f + i * 0.18f;
                Stretch(stack.rectTransform, new Vector2(x, 0.22f), new Vector2(x, 0.74f - i * 0.08f), new Vector2(-8f, 0f), new Vector2(8f, 0f));
            }
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static GameObject CreateObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            return gameObject;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject gameObject = CreateObject(name, parent);
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Image CreateBorderedPanel(string name, Transform parent, Color color)
        {
            Image panel = CreateImage(name, parent, color);
            panel.raycastTarget = true;

            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(2f, -2f);

            return panel;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, int size, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
        {
            GameObject gameObject = CreateObject(name, parent);
            TextMeshProUGUI textComponent = gameObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = size;
            textComponent.fontStyle = fontStyle;
            textComponent.alignment = alignment;
            textComponent.color = color;
            textComponent.raycastTarget = false;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            return textComponent;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            Image image = CreateImage(name, parent, Bronze);
            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.98f, 0.73f, 0.34f, 1f);
            colors.pressedColor = new Color(0.58f, 0.34f, 0.14f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text labelText = CreateText("Label", image.transform, label, 28, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.95f, 0.86f, 0.62f, 1f));
            Stretch(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 8f), new Vector2(-18f, -8f));
            return button;
        }

        private static void AddLayout(GameObject gameObject, float preferredWidth, float preferredHeight, float flexibleWidth = 0f)
        {
            LayoutElement layout = gameObject.AddComponent<LayoutElement>();
            if (preferredWidth > 0f)
            {
                layout.preferredWidth = preferredWidth;
            }

            if (preferredHeight > 0f)
            {
                layout.preferredHeight = preferredHeight;
            }

            layout.flexibleWidth = flexibleWidth;
        }

        private static void AddFixedLayout(GameObject gameObject, float width, float height)
        {
            LayoutElement layout = gameObject.AddComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            layout.flexibleWidth = 0f;
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleHeight = 0f;
        }

        private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
