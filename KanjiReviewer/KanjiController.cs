using System;

namespace KanjiReviewer
{
    static class KanjiController
    {
        static Random random = new Random();

        public static Random Random
        {
            get { return random; }
        }

        static double NextDouble(this Random generator, double minValue, double maxValue)
        {
            return minValue + generator.NextDouble() * (maxValue - minValue);
        }

        public static void Review(this KanjiEntry entry, ReviewResult result)
        {
            var reviewDate = DateTime.UtcNow;
            switch (result)
            {
                case ReviewResult.Yes:
                case ReviewResult.Easy:
                    entry.Compartment++;
                    break;
                default:
                case ReviewResult.No:
                    entry.Compartment = 0;
                    break;
            }

            double interval;
            switch (entry.Compartment)
            {
                case 0: interval = 0; break;
                case 1: interval = 3; break;
                case 2: interval = 7; break;
                case 3: interval = 14; break;
                case 4: interval = 30; break;
                case 5: interval = 60; break;
                case 6: interval = 120; break;
                default: interval = 240; break;
            }

            var variance = interval / 6;
            interval += random.NextDouble(-variance, variance);
            if (result == ReviewResult.Easy) interval += interval * 0.5;
            entry.LastReview = reviewDate;
            entry.NextReview = reviewDate + TimeSpan.FromDays(interval);
        }
    }

    public enum ReviewResult
    {
        No,
        Yes,
        Easy
    }
}
