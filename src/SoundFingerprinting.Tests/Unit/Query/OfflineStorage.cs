namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;

    public class OfflineStorage : IOfflineStorage
    {
        private const string Format = ".fp";
        private const string DateFormat = "yyyy-MM-ddTHH-mm-ss";
        private readonly string folder;

        public OfflineStorage(string folder)
        {
            this.folder = folder;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (string previousFiles in GetPreviousFiles(folder))
            {
                File.Delete(previousFiles);
            }
        }
        
        public IEnumerator<AVHashes> GetEnumerator()
        {
            foreach (var file in GetPreviousFiles(folder))
            {
                yield return Deserialize(file);
            }
        }

        private static AVHashes Deserialize(string file)
        {
            using var stream = new FileStream(file, FileMode.Open);
            return Serializer.DeserializeWithLengthPrefix<AVHashes>(stream, PrefixStyle.Fixed32);
        }

        private static IEnumerable<string> GetPreviousFiles(string path)
        {
            return Directory.GetFiles(path, $"*{Format}").OrderBy(filename => DateTime.ParseExact(Path.GetFileNameWithoutExtension(filename), DateFormat, CultureInfo.InvariantCulture));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(AVHashes? avHashes)
        {
            if (avHashes == null || avHashes.IsEmpty || Contains(avHashes))
            {
                return;
            }
            
            using var fileStream = new FileStream(GetPath(avHashes.CaptureTime), FileMode.CreateNew);
            Serializer.SerializeWithLengthPrefix(fileStream, avHashes, PrefixStyle.Fixed32);
        }
        
        public bool Contains(DateTime captureTime)
        {
            var path = GetPath(captureTime);
            return File.Exists(path);
        }

        public bool Contains(AVHashes avHashes)
        {
            return Contains(avHashes.CaptureTime);
        }

        public AVHashes? Get(DateTime captureTime)
        {
            if (!Contains(captureTime))
            {
                return null;
            }
            
            return Deserialize(GetPath(captureTime));
        }

        public void Remove(AVHashes avHashes)
        {
            if (Contains(avHashes))
            {
                File.Delete(GetPath(avHashes.CaptureTime));
            }
        }
        
        private string GetPath(DateTime captureTime)
        {
            return Path.Combine(folder, captureTime.ToString(DateFormat) + Format);
        }
    }
}