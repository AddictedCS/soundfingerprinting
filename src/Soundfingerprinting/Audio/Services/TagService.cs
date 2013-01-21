namespace Soundfingerprinting.Audio.Services
{
    using System;

    using Soundfingerprinting.Audio.Models;

    using Un4seen.Bass;
    using Un4seen.Bass.AddOn.Fx;
    using Un4seen.Bass.AddOn.Mix;
    using Un4seen.Bass.AddOn.Tags;

    public class TagService : ITagService
    {
        public TagService()
        {
            // Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
            BassNet.Registration("ciumac.sergiu@gmail.com", "2X155323152222");

            if (!Bass.BASS_Init(0, 5512, BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero))
            {
                throw new Exception(Bass.BASS_ErrorGetCode().ToString());
            }
        }

        public TagInfo GetTagInfo(string pathToAudioFile)
        {
            TAG_INFO tags = BassTags.BASS_TAG_GetFromFile(pathToAudioFile);
            TagInfo tag = new TagInfo
                {
                    Duration = tags.duration,
                    Album = tags.album,
                    Artist = tags.artist,
                    Title = tags.title,
                    AlbumArtist = tags.albumartist,
                    Genre = tags.genre,
                    Year = tags.year,
                    Composer = tags.composer
                };

            return tag;
        }
    }
}