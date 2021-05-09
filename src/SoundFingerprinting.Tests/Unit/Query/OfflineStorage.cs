namespace SoundFingerprinting.Tests.Unit.Query
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using ProtoBuf;
    using SoundFingerprinting.Data;

    public class OfflineStorage : IEnumerable<Hashes>
    {
        private const string DateFormat = "yyyy-MM-ddTHH-mm-ss";
        private readonly string folder;

        public OfflineStorage(string folder)
        {
            this.folder = folder;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (var previousFiles in Directory.GetFiles(folder, "*.hash"))
            {
                File.Delete(previousFiles);
            }
        }
        
        public void Save(Hashes timedHashes)
        {
            if (timedHashes.IsEmpty)
            {
                return;
            }

            using var fileStream = new FileStream(Path.Combine(folder, timedHashes.RelativeTo.ToString(DateFormat) + ".hash"), FileMode.CreateNew);
            Serializer.SerializeWithLengthPrefix(fileStream, timedHashes, PrefixStyle.Fixed32);
        }
        
        public IEnumerator<Hashes> GetEnumerator()
        {
            foreach (var file in Directory.GetFiles(folder, "*.hash").OrderBy(filename => DateTime.ParseExact(Path.GetFileNameWithoutExtension(filename), DateFormat, CultureInfo.InvariantCulture)))
            {
                using (var stream = new FileStream(file, FileMode.Open))
                {
                    yield return Serializer.DeserializeWithLengthPrefix<Hashes>(stream, PrefixStyle.Fixed32);
                }
                
                File.Delete(file); // or archive for
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}