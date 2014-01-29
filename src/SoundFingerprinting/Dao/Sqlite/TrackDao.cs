namespace SoundFingerprinting.Dao.Sqlite
{
    ﻿using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using LinqToDB;
    using LinqToDB.Data;
    using LinqToDB.Mapping;
    using SoundFingerprinting.Data;

    internal class Track
    {
        [Column(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Artist { get; set; }
        public string Title { get; set; }
        public string ISRC { get; set; }
        public string Album { get; set; }
        public int ReleaseYear { get; set; }
        public int TrackLengthSec { get; set; }
    }

    internal class TrackDao : ITrackDao
    {
        private readonly string _configurationString;

        public TrackDao(string configurationString)
        {
            _configurationString = configurationString;

            Mapper.CreateMap<TrackData, Track>();
            Mapper.CreateMap<Track, TrackData>()
                .ForMember(td => td.TrackReference, opt => opt.MapFrom(track => new ModelReference<int>(track.Id)));
        }

        public int Insert(TrackData track)
        {
            using (var db = new DataConnection(_configurationString))
            {
                var res = db.InsertWithIdentity(Mapper.Map<Track>(track));
                var id = Convert.ToInt32(res);
                track.TrackReference = new ModelReference<int>(id);
                return id;
            }
        }

        public IList<TrackData> ReadAll()
        {
            using (var db = new DataConnection(_configurationString))
            {
                return (from t in db.GetTable<Track>() select t).ToList().Select(Mapper.Map<TrackData>).ToList();
            }
        }

        public TrackData ReadById(int id)
        {
            using (var db = new DataConnection(_configurationString))
            {
                var track = (from t in db.GetTable<Track>() where t.Id == id select t).FirstOrDefault();
                return Mapper.Map<TrackData>(track);
            }
        }

        public IList<TrackData> ReadTrackByArtistAndTitleName(string artist, string title)
        {
            using (var db = new DataConnection(_configurationString))
            {
                var res = (from t in db.GetTable<Track>() where t.Artist == artist && t.Title == title select t).ToList();

                return res.Select(Mapper.Map<TrackData>).ToList();
            }
        }

        public TrackData ReadTrackByISRC(string isrc)
        {
            using (var db = new DataConnection(_configurationString))
            {
                var track = (from t in db.GetTable<Track>() where t.ISRC == isrc select t).First();
                return Mapper.Map<TrackData>(track);
            }
        }

        public int DeleteTrack(int trackId)
        {
            using (var db = new DataConnection(_configurationString))
            {
                return (from t in db.GetTable<Track>() where t.Id == trackId select t).Delete();
            }
        }
    }
}