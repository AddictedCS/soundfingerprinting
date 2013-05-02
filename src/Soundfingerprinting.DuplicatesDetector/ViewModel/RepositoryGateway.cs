namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Ninject;
    using Ninject.Parameters;

    using Soundfingerprinting.Audio.Services;
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.DuplicatesDetector.DataAccess;
    using Soundfingerprinting.DuplicatesDetector.Infrastructure;
    using Soundfingerprinting.DuplicatesDetector.Model;
    using Soundfingerprinting.DuplicatesDetector.Services;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.LSH;
    using Soundfingerprinting.Hashing.MinHash;

    /// <summary>
    ///   Class which prepares the data for Repository analysis of the tracks (does all the "dirty job")
    /// </summary>
    public class RepositoryGateway
    {
        #region Constants

        /// <summary>
        ///   Maximum track length (track's bigger than this value will be discarded)
        /// </summary>
        private const int MaxTrackLength = 60 * 10; /*10 min - maximal track length*/

        /// <summary>
        ///   Number of milliseconds to process from each song
        /// </summary>
        private const int MillisecondsToProcess = 10 * 1000;

        /// <summary>
        ///   Starting processing point
        /// </summary>
        private const int MillisecondsStart = 20 * 1000;

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
        private const int MinTrackLength = ((MillisecondsToProcess + MillisecondsStart) / 1000) + 1;

        /// <summary>
        ///   Number of LSH tables
        /// </summary>
        private const int NumberOfHashTables = 25;

        /// <summary>
        ///   Number of Min Hash keys per 1 hash function (1 LSH table)
        /// </summary>
        private const int NumberOfKeys = 4;

        /// <summary>
        ///   Path to permutations (generated using greedy algorithm)
        /// </summary>
        private const string PathToPermutations = "perms.csv";

        /// <summary>
        ///   Number of threshold votes for a file to be considerate a duplicate
        /// </summary>
        private const int ThresholdVotes = 5;

        /// <summary>
        ///   Value of threshold percentage of fingerprints that needs to be gathered
        ///   in order to be considered a possible result
        /// </summary>
        private const double ThresholdPercentage = 0;

        /// <summary>
        ///   Separator in the .csv files
        /// </summary>
        private const string Separator = ",";

        /// <summary>
        ///   Down sampling rate
        /// </summary>
        /// <remarks>
        ///   If you want to change this, contact ciumac.sergiu@gmail.com
        /// </remarks>
        private const int SampleRate = 5512;

        #endregion

        #region Read-only components

        /// <summary>
        ///   Creational stride (used in hashing audio objects)
        /// </summary>
        private readonly IStride createStride;

        /// <summary>
        ///   Permutations provider
        /// </summary>
        private readonly IPermutations permutations;

        /// <summary>
        ///   Repository for storage, permutations, algorithm
        /// </summary>
        private readonly Repository repository;

        /// <summary>
        ///   Storage for hash signatures and tracks
        /// </summary>
        private readonly IStorage storage;

        /// <summary>
        ///   Cancelation token used to abort all the processing
        /// </summary>
        private CancellationTokenSource cts;

        private readonly ITagService tagService;

        private readonly IExtendedAudioService audioService;

        #endregion

        public RepositoryGateway()
        {
            storage =
                ServiceContainer.Kernel.Get<IStorage>(
                    new ConstructorArgument("numberOfHashTables", NumberOfHashTables));
                /*Number of LSH Tables, used for storage purposes*/

            permutations =
                ServiceContainer.Kernel.Get<IPermutations>(
                    new ConstructorArgument("pathToPermutations", PathToPermutations),
                    new ConstructorArgument("separator", Separator)); /*Permutations*/

            audioService = ServiceContainer.Kernel.Get<IExtendedAudioService>();

            tagService = ServiceContainer.Kernel.Get<ITagService>();

            cts = new CancellationTokenSource();

            repository = new Repository(
                ServiceContainer.Kernel.Get<IFingerprintService>(),
                ServiceContainer.Kernel.Get<IWorkUnitBuilder>(),
                storage,
                new CombinedHashingAlgorithm(new MinHashService(permutations), new LSHService()));
                
            createStride = new IncrementalRandomStride(1, 5115, 128 * 64);
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
            Action<List<Track>, Exception> callback,
            Action<Track> trackProcessed)
        {
            List<string> files = new List<string>();
            foreach (Item path in paths)
            {
                if (path.IsFolder)
                {
                    files.AddRange(Helper.GetMusicFiles(path.Path, fileFilters, true)); //get music file names
                }
                else
                {
                    files.Add(path.Path);
                }
            }

            List<Track> tracks = null;
            Task.Factory.StartNew(
                () =>
                    {
                        try
                        {
                            tracks = ProcessFiles(files, trackProcessed);
                            callback.Invoke(tracks, null);
                        }
                        catch (AggregateException ex) /*here we are sure all consumers are done processing*/
                        {
                            callback.Invoke(null, null);
                            repository.ClearStorage(); /*its safe to clear the storage, no more thread is executing*/
                        }
                        catch (Exception ex)
                        {
                            callback.Invoke(tracks, ex);
                        }
                    },
                cts.Token);
        }

        /// <summary>
        ///   Find duplicate files for specific tracks
        /// </summary>
        /// <param name = "tracks">Tracks to search in</param>
        /// <param name = "callback">Callback invoked at each processed track</param>
        /// <returns>Set of tracks that are duplicate</returns>
        public HashSet<Track>[] FindDuplicates(List<Track> tracks, Action<Track, int, int> callback)
        {
            return repository.FindDuplicates(tracks, ThresholdVotes, ThresholdPercentage, callback);
        }

        /// <summary>
        ///   Find all duplicate files from the storage
        /// </summary>
        /// <param name = "callback">Callback invoked at each processed track</param>
        /// <returns>Set of tracks that are duplicate</returns>
        public HashSet<Track>[] FindAllDuplicates(Action<Track, int, int> callback)
        {
            return repository.FindDuplicates(storage.GetAllTracks(), ThresholdVotes, ThresholdPercentage, callback);
        }

        /// <summary>
        ///   Abort processing the files (at any stage)
        /// </summary>
        public void AbortProcessing()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
        }

        /// <summary>
        ///   Process files (get fingerprint signatures, hash them into storage)
        /// </summary>
        /// <param name = "files">List of files to be hashed</param>
        /// <param name = "processed">Callback invoked once 1 track is processed</param>
        /// <returns>List of processed tracks</returns>
        private List<Track> ProcessFiles(IEnumerable<string> files, Action<Track> processed)
        {
            /*preprocessing stage ended, now make sure to do the actual job*/

            int numProcs = Environment.ProcessorCount;

            // 1024 (Kb) * BufferSize / SampleRate * SecondsRead * 4 (1 float = 4 bytes) / 1024 (Kb)
            const int Buffersize =
                (int)((1024.0 * BufferSize) / ((double)SampleRate * MillisecondsToProcess / 1000 * 4 / 1024));

            // ~317 songs are allowed for 15 seconds snippet at 5512 Hz sample rate
            BlockingCollection<Tuple<Track, float[]>> buffer = new BlockingCollection<Tuple<Track, float[]>>(Buffersize);
            List<Track> processedtracks = new List<Track>();
            List<Task> consumers = new List<Task>();
            List<Task> producers = new List<Task>();
            CancellationToken token = cts.Token;
            ConcurrentBag<string> bag = new ConcurrentBag<string>(files);

            int maxprod = numProcs > 2 ? 2 : numProcs;
            for (var i = 0; i < maxprod; i++) /*producers*/
            {
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

                                Track track;
                                float[] samples;
                                try
                                {
                                    track = TrackHelper.GetTrackInfo(MinTrackLength, MaxTrackLength, file, tagService); //lame casting I know
                                    samples = TrackHelper.GetTrackSamples(track, audioService, SampleRate, MillisecondsToProcess, MillisecondsStart);
                                }
                                catch
                                {
                                    continue;
                                    /*Continue processing even if getting samples failed*/
                                    /*the failing might be caused by a bunch of File I/O factors, that cannot be considered critical*/
                                }
                                try
                                {
                                    buffer.TryAdd(new Tuple<Track, float[]>(track, samples), 1, token); /*producer*/
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
            Task.Factory.ContinueWhenAll(producers.ToArray(), (p) => buffer.CompleteAdding());

            for (int i = 0; i < numProcs * 4; i++) /*consumer*/
            {
                consumers.Add(Task.Factory.StartNew(
                    () =>
                    {
                        foreach (Tuple<Track, float[]> tuple in buffer.GetConsumingEnumerable()) /*If OCE is thrown it will be caught in the caller's AggregateException*/
                        {
                            if (tuple != null)
                            {
                                /*Long running procedure*/
                                repository.CreateInsertFingerprints(
                                    tuple.Item2, tuple.Item1, createStride, NumberOfHashTables, NumberOfKeys);

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