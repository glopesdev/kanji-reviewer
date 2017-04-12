using System;
using System.IO;

namespace KanjiReviewer
{
    static class KanjiEntrySerializer
    {
        public static void Read(BinaryReader reader, KanjiEntry entry)
        {
            entry.Compartment = reader.ReadInt32();
            entry.PassedCount = reader.ReadInt32();
            entry.FailedCount = reader.ReadInt32();
            entry.LastReview = DateTime.FromBinary(reader.ReadInt64());
            entry.NextReview = DateTime.FromBinary(reader.ReadInt64());
        }

        public static void Write(BinaryWriter writer, KanjiEntry entry)
        {
            writer.Write(entry.Compartment);
            writer.Write(entry.PassedCount);
            writer.Write(entry.FailedCount);
            writer.Write(entry.LastReview.ToBinary());
            writer.Write(entry.NextReview.ToBinary());
        }
    }
}
