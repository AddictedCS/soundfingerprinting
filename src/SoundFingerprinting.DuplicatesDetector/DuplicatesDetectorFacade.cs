namespace SoundFingerprinting.DuplicatesDetector
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Ninject;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DuplicatesDetector.Infrastructure;
    using SoundFingerprinting.DuplicatesDetector.Model;
    using SoundFingerprinting.DuplicatesDetector.ViewModel;

    /// <summary>
    ///   Facade which prepares the data for analysis of the tracks (does all the "dirty job")
    /// </summary>
    public class DuplicatesDetectorFacade : IDisposable
    {
        /// <summary>
        ///   Maximum track length (track's bigger than this value will be discarded)
        /// </summary>
        private const int MaxTrackLength = 60 * 10; /*10 min - maximal track length*/

        /// <summary>
        ///   Number of seconds to process from each song
        /// </summary>
        private const int SecondsToProcess = 10;

        /// <summary>
        ///   Starting processing point
        /// </summary>
        private const int StartProcessingAtSecond = 20;

        /// <summary>
        ///   Buffer size of the application reading songs
        /// </summary>
        /// <remarks>
        ///   Represented in MB.
        ///   Max 100MB will be reserved for the samples read from songs
        /// </remarks>
        private const int BufferSize = 100;

        /// <summary>
        ///   Minimum track length (track's less than this value will be discarded)
        /// </summary>
        private const int MinTrackLength = SecondsToProcess + StartProcessingAtSecond + 1;
       
        /// <summary>
        ///   Down sampling rate
        /// </summary>
        /// <remarks>
        ///   If you want to change this, contact ciumac.sergiu@gmail.com
        /// </remarks>
        private const int SampleRate = 5512;
       
        private readonly DuplicatesDetectorService duplicatesDetectorService;

        private readonly TrackHelper trackHelper;

        private CancellationTokenSource cts;

        public DuplicatesDetectorFacade()
        {
            cts = new CancellationTokenSource();
            duplicatesDetectorService = ServiceContainer.Kernel.Get<DuplicatesDetectorService>();
            trackHelper = ServiceContainer.Kernel.Get<TrackHelper>();
        }

        ~DuplicatesDetectorFacade()
        {
            Dispose(false);
        }

        /// <summary>
        ///   Process the tracks asynchronously (get their path location, fingerprint content, hash fingerprint into storage)
        /// </summary>
        /// <param name = "paths">Paths to be processed</param>
        /// <param name = "fileFilters">File filters used</param>
        /// <param name = "callback">Callback invoked once processing ends</param>
        /// <param name = "trackProcessed">Callback invoked once 1 track is processed</param>
        public void ProcessTracksAsync(
            IEnumerable<Item> paths,
            string[] fileFilters,
            Action<List<TrackData>, Exception> callback,
            Action<TrackData> trackProcessed)
        {
            var files = new List<string>();
            foreach (var path in paths)
            {
                if (path.IsFolder)
                {
                    files.AddRange(Helper.GetMusicFiles(path.Path, fileFilters)); // get music file names
                }
                else
                {
                    files.Add(path.Path);
                }
            }

            Task.Factory.StartNew(
                () =>
                    {
                        try
                        {
                            var tracks = ProcessFiles(files, trackProcessed);
                            callback.Invoke(tracks, null);
                        }
                        catch (AggregateException) /*here we are sure all consumers are done processing*/
                        {
                            callback.Invoke(null, null);
                            duplicatesDetectorService.ClearStorage(); /*its safe to clear the storage, no more thread is executing*/
                        }
                        catch (Exception ex)
                        {
                            callback.Invoke(null, ex);
                        }
                    },
                cts.Token);
        }

        /// <summary>
        ///   Find all duplicate files from the storage
        /// </summary>
        /// <param name = "callback">Callback invoked at each processed track</param>
        /// <returns>Set of tracks that are duplicate</returns>
        public HashSet<TrackData>[] FindAllDuplicates(Action<TrackData, int, int> callback)
        {
            return duplicatesDetectorService.FindDuplicates(callback);
        }

        /// <summary>
        ///   Abort processing the files (at any stage)
        /// </summary>
        public void AbortProcessing()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                cts.Dispose();
            }
        }

        /// <summary>
        ///   Process files (get fingerprint signatures, hash them into storage)
        /// </summary>
        /// <param name = "files">List of files to be hashed</param>
        /// <param name = "processed">Callback invoked once 1 track is processed</param>
        /// <returns>List of processed tracks</returns>
        private List<TrackData> ProcessFiles(IEnumerable<string> files, Action<TrackData> processed)
        {
            /*preprocessing stage ended, now make sure to do the actual job*/

            int numProcs = Environment.ProcessorCount;

            // 1024 (Kb) * BufferSize / SampleRate * SecondsRead * 4 (1 float = 4 bytes) / 1024 (Kb)
            const int Buffersize =
                (int)((1024.0 * BufferSize) / ((double)SampleRate * SecondsToProcess / 1000 * 4 / 1024));

            // ~317 songs are allowed for 15 seconds snippet at 5512 Hz sample rate
            var buffer = new BlockingCollection<Tuple<TrackData, float[]>>(Buffersize);
            var processedtracks = new List<TrackData>();
            var consumers = new List<Task>();
            var producers = new List<Task>();
            CancellationToken token = cts.Token;
            var bag = new ConcurrentBag<string>(files);

            int maxprod = numProcs > 2 ? 2 : numProcs;
            for (var i = 0; i < maxprod; i++)
            {
                /*producers*/
                producers.Add(Task.Factory.StartNew(
                    () =>
                    {
                        while (!bag.IsEmpty)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }

                                string file;
                                if (!bag.TryTake(out file))
                                {
                                    return;
                                }

                                TrackData track;
                                float[] samples;
                                try
                                {
                                    track = trackHelper.GetTrack(MinTrackLength, MaxTrackLength, file); // lame casting I know
                                    samples = trackHelper.GetTrackSamples(track, SampleRate, SecondsToProcess, StartProcessingAtSecond);
                                }
                                catch
                                {
                                    continue;
                                    /*Continue processing even if getting samples failed*/
                                    /*the failing might be caused by a bunch of File I/O factors, that cannot be considered critical*/
                                }

                                try
                                {
                                    buffer.TryAdd(new Tuple<TrackData, float[]>(track, samples), 1, token); /*producer*/
                                }
                                catch (OperationCanceledException)
                                {
                                    /*it is safe to break here, operation was canceled*/
                                    break;
                                }
                            }
                    },
                    token));
            }

            /*When all producers ended with their operations, call the CompleteAdding() to tell Consumers no more items are available*/
            Task.Factory.ContinueWhenAll(producers.ToArray(), p => buffer.CompleteAdding());

            for (int i = 0; i < numProcs * 4; i++) 
            {
                /*consumer*/
                consumers.Add(Task.Factory.StartNew(
                    () =>
                    {
                        foreach (Tuple<TrackData, float[]> tuple in buffer.GetConsumingEnumerable()) /*If OCE is thrown it will be caught in the caller's AggregateException*/
                        {
                            if (tuple != null)
                            {
                                /*Long running procedure*/
                                duplicatesDetectorService.CreateInsertFingerprints(tuple.Item2, tuple.Item1);

                                processedtracks.Add(tuple.Item1);
                                if (processed != null)
                                {
                                    processed.Invoke(tuple.Item1);
                                }
                            }
                        }
                    },
                    token));
            }

            Task.WaitAll(consumers.ToArray()); /*wait for all consumers to end*/
            return processedtracks;
        }
    }
}