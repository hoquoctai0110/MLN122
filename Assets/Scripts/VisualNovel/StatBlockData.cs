using System;
using UnityEngine;

namespace MLN122.VisualNovel
{
    [Serializable]
    public sealed class StatBlockData
    {
        [SerializeField] private int capital;
        [SerializeField] private int marketShare;
        [SerializeField] private int technology;
        [SerializeField] private int reputation;
        [SerializeField] private int investigationRisk;

        public int Capital => capital;
        public int MarketShare => marketShare;
        public int Technology => technology;
        public int Reputation => reputation;
        public int InvestigationRisk => investigationRisk;

        public StatBlockData()
        {
        }

        public StatBlockData(int capital, int marketShare, int technology, int reputation, int investigationRisk)
        {
            this.capital = capital;
            this.marketShare = marketShare;
            this.technology = technology;
            this.reputation = reputation;
            this.investigationRisk = investigationRisk;
        }

        public void Add(StatBlockData other)
        {
            if (other == null)
            {
                return;
            }

            capital += other.capital;
            marketShare += other.marketShare;
            technology += other.technology;
            reputation += other.reputation;
            investigationRisk += other.investigationRisk;

            ClampValues();
        }

        public string ToDisplayString()
        {
            return $"Capital: {capital}\nMarket Share: {marketShare}%\nTechnology: {technology}\nReputation: {reputation}\nInvestigation Risk: {investigationRisk}";
        }

        public string ToChangeDisplayString()
        {
            return $"Capital {FormatChange(capital)}\nMarket Share {FormatChange(marketShare)}%\nTechnology {FormatChange(technology)}\nReputation {FormatChange(reputation)}\nInvestigation Risk {FormatChange(investigationRisk)}";
        }

        private static string FormatChange(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }

        private void ClampValues()
        {
            capital = Mathf.Max(0, capital);
            marketShare = Mathf.Clamp(marketShare, 0, 100);
            technology = Mathf.Max(0, technology);
            reputation = Mathf.Max(0, reputation);
            investigationRisk = Mathf.Max(0, investigationRisk);
        }
    }
}
