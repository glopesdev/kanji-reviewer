using System;

namespace KanjiReviewer
{
    class KanjiEntry
    {
        public KanjiEntry()
        {
        }

        public KanjiEntry(string meaning, string strokeSource, DateTime nextReview)
        {
            Meaning = meaning;
            StrokeSource = strokeSource;
            LastReview = nextReview;
            NextReview = nextReview;
        }

        public string Meaning { get; set; }

        public string StrokeSource { get; set; }

        public int Compartment { get; set; }

        public int PassedCount { get; set; }

        public int FailedCount { get; set; }

        public DateTime LastReview { get; set; }

        public DateTime NextReview { get; set; }
    }
}
