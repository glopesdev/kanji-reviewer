using System;
using System.IO;

namespace KanjiReviewer
{
    static class KanjiEntrySerializer
    {
        public static KanjiEntry Read(BinaryReader reader)
        {
            var entry = new KanjiEntry();
            entry.Meaning = reader.ReadString();
            entry.StrokeSource = reader.ReadString();
            entry.Compartment = reader.ReadInt32();
            entry.PassedCount = reader.ReadInt32();
            entry.FailedCount = reader.ReadInt32();
            entry.LastReview = DateTimeOffset.FromFileTime(reader.ReadInt64());
            entry.NextReview = DateTimeOffset.FromFileTime(reader.ReadInt64());
            return entry;
        }

        public static void Write(BinaryWriter writer, KanjiEntry entry)
        {
            writer.Write(entry.Meaning);
            writer.Write(entry.StrokeSource);
            writer.Write(entry.Compartment);
            writer.Write(entry.PassedCount);
            writer.Write(entry.FailedCount);
            writer.Write(entry.LastReview.ToFileTime());
            writer.Write(entry.NextReview.ToFileTime());
        }
    }
}
