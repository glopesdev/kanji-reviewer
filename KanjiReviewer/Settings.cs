using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;

namespace KanjiReviewer
{
    class Settings
    {
        const string DatabaseFile = "database.dat";

        public int FrameNumber { get; set; }

        public KanjiEntry[] Database { get; private set; }

        public static async Task<Settings> Create()
        {
            var nextReview = DateTime.UtcNow;
            var kanjiFile = await Package.Current.InstalledLocation.GetFileAsync("Assets\\KanjiMap.csv");
            var lines = await FileIO.ReadLinesAsync(kanjiFile);
            var database = new KanjiEntry[lines.Count];
            for (int i = 0; i < database.Length; i++)
            {
                var columns = lines[i].Split('\t');
                database[i] = new KanjiEntry(columns[1], columns[0] + ".xaml", nextReview);
            }

            const int DefaultFrameNumber = 15;
            return new Settings
            {
                FrameNumber = DefaultFrameNumber,
                Database = database
            };
        }

        static async Task<byte[]> ReadBytesAsync(IStorageFile file)
        {
            using (var stream = await file.OpenReadAsync())
            using (var reader = new DataReader(stream))
            {
                var buffer = new byte[stream.Size];
                await reader.LoadAsync((uint)buffer.Length);
                reader.ReadBytes(buffer);
                return buffer;
            }
        }

        public async void Read(StorageFolder storage)
        {
            var settingsFile = await storage.TryGetItemAsync(DatabaseFile);
            if (settingsFile != null && settingsFile.IsOfType(StorageItemTypes.File))
            {
                var buffer = await ReadBytesAsync((IStorageFile)settingsFile);
                using (var stream = new MemoryStream(buffer))
                using (var reader = new BinaryReader(stream))
                {
                    FrameNumber = reader.ReadInt32();
                    for (int i = 0; i < Database.Length; i++)
                    {
                        KanjiEntrySerializer.Read(reader, Database[i]);
                    }
                }
            }
        }

        public async void Write(StorageFolder storage)
        {
            const int SizeOfEntry = 28;
            var buffer = new byte[Database.Length * SizeOfEntry + sizeof(int)];
            using (var stream = new MemoryStream(buffer))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(FrameNumber);
                for (int i = 0; i < Database.Length; i++)
                {
                    KanjiEntrySerializer.Write(writer, Database[i]);
                }
            }

            var settingsFile = await storage.CreateFileAsync(
                DatabaseFile,
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(settingsFile, buffer);
        }
    }
}
