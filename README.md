## Sound Fingerprinting

_soundfingerprinting_ is a C# framework designed for developers, enthusiasts, researchers in the fields of audio and digital signal processing, data mining, and alike.  It implements an [efficient algorithm|http://static.googleusercontent.com/media/research.google.com/en//pubs/archive/32685.pdf] of digital signal processing which allows developing a system of audio fingerprinting and recognition.

## Documentation

Following is a code sample that shows you how to extract unique characteristics from an audio file and later use them as identifiers to recognize unknown snippets from a variety of sources. These characteristics known as _sub-fingerprints_ (or _fingerprints_, these 2 terms used interchangeably) will be stored in the configurable backend. The interfaces for fingerprinting and querying audio files have been implemented as [Fluent Interfaces](http://martinfowler.com/bliki/FluentInterface.html) with [Builder](http://en.wikipedia.org/wiki/Builder_pattern) and [Command](http://en.wikipedia.org/wiki/Command_pattern) patterns in mind.
```csharp
private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in memory
private readonly IAudioService audioService = new NAudioService(); // use NAudio audio processing library
private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();

public void StoreAudioFileFingerprintsInStorageForLaterRetrieval(string pathToAudioFile)
{
    TrackData track = new TrackData("GBBKS1200164", "Adele", "Skyfall", "Skyfall", 2012, 290);
	
    // store track metadata in the database
    var trackReference = modelService.InsertTrack(track);

    // create sub-fingerprints and its hash representation
    var hashedFingerprints = fingerprintCommandBuilder
                                .BuildFingerprintCommand()
                                .From(pathToAudioFile)
                                .UsingServices(audioService)
                                .Hash()
                                .Result;
								
    // store sub-fingerprints and its hash representation in the database 
    modelService.InsertHashedFingerprintsForTrack(hashedFingerprints, trackReference);
}
```
The default storage, which comes bundled with _soundfingerprinting_ package, is a plain in memory storage, managed by <code>InMemoryModelService</code>. In case you would like to store fingerprints in a perstistent database you can take advantage of MSSQL integration available in [SoundFingerprinting.SQL](https://www.nuget.org/packages/SoundFingerprinting.SQL) package via <code>SqlModelService</code> class. The MSSQL database initialization script as well as source files can be found [here|https://github.com/AddictedCS/soundfingerprinting.sql]. Do not forget to add connection string <code>FingerprintConnectionString</code> in your app.config file.

Once you've inserted the fingerprints into the database, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, URL, microphone, radio tuner, etc. It's up to your application, where you get the samples from.
```csharp
private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();

public TrackData GetBestMatchForSong(string queryAudioFile)
{
    int secondsToAnalyze = 10; // number of seconds to analyze from query file
    int startAtSecond = 0; // start at the begining
	
    // query the underlying database for similar audio sub-fingerprints
    var queryResult = queryCommandBuilder.BuildQueryCommand()
                                         .From(queryAudioFile, secondsToAnalyze, startAtSecond)
                                         .UsingServices(modelService, audioService)
                                         .Query()
                                         .Result;
    if(queryResult.IsSuccessful)
    {
        return queryResult.BestMatch.Track; // successful match has been found
    }
	
    return null; // no match has been found
}
```
See the [Wiki Page](https://github.com/AddictedCS/soundfingerprinting/wiki) for the operational details and information. 

### Upgrade from 1.0.1 to 2.0.0
_soundfingerprinting_ project structure, as well interface signatures has been drastically changed from release 1.0.1 to 2.0.0. This is due to the fact that git repository has been splitted to accommodate correct release cycles for all developed modules. [Bass|https://github.com/AddictedCS/soundfingerprinting.audio.bass] audio library, [MSSQL|https://github.com/AddictedCS/soundfingerprinting.sql], [MongoDB|https://github.com/AddictedCS/soundfingerprinting.mongodb], [NeuralHasher|https://github.com/AddictedCS/soundfingerprinting.neuralhasher], and all demo apps are now located in separate git repositories.

### Find exact audio snippet location within the resulting track
This feature has been asked for a long time and is now available starting from release 2.0.0. <code>QueryCommand</code> interface has one additional method <code>QueryWithTimeSequenceInformation</code> which returns best candidate as well as its location in the resulting track.

### Extension capabilities
Some of the interfaces which are used by the framework can be easily substituted according to your needs. In case you don't want to use _NAudio_ as your audio library, you can take advantage of Bass.Net integration available through [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) package.

####Available integrations:
* [SoundFingerprinting.Audio.NAudio](https://www.nuget.org/packages/SoundFingerprinting.Audio.NAudio) - NAudio library used for audio processing. Comes bundled as the default audio library.
* [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) - Bass.Net audio library integration. Works faster, more accurate resampling, independent upon target OS. Sources available [here|https://github.com/AddictedCS/soundfingerprinting.audio.bass]
* [SoundFingerprinting.SQL](https://www.nuget.org/packages/SoundFingerprinting.SQL) - implements integration with MSSQL storage. Sources available [here|https://github.com/AddictedCS/soundfingerprinting.sql]
* [SoundFingerprinting.MongoDb](https://www.nuget.org/packages/SoundFingerprinting.MongoDb) - implements integration with MongoDb, still in alpha phase. Source available [here|https://github.com/AddictedCS/soundfingerprinting.mongodb]

### Algorithm configuration
Fingerprinting and Querying algorithms can be easily parametrized with corresponding configuration objects passed as parameters on command creation.

```csharp
 var hashDatas = fingerprintCommandBuilder
                           .BuildFingerprintCommand()
                           .From(samples)
                           .WithFingerprintConfig(
	                            config =>
	                            {
	                                config.TopWavelets = 250; // increase number of top wavelets
	                                config.Stride = new RandomStride(512, 256); // stride between sub-fingerprints
	                            })
                           .UsingServices(audioService)
                           .Hash()
                           .Result;
```
Each and every configuration parameter can influence the recognition rate, required storage, computational cost, etc. Stick with the defaults, unless you would like to experiment. 

### Third party libraries involved
Links to the third party libraries used by SoundFingerprinting project.
* [NAudio](http://naudio.codeplex.com/)
* [FFTW](http://www.fftw.org/) - used as a default framework for FFT algorithm.
* [Ninject](http://www.ninject.org/) - used to take advantage of dependency inversion principle.

### Unit testing
Even though a couple of controversial topics are discussed recently in software community ([here](http://david.heinemeierhansson.com/2014/tdd-is-dead-long-live-testing.html), and [here](https://www.youtube.com/watch?v=z9quxZsLcfo)) I'm still strongly committed to TDD practices. In case you'd like to contribute, your code has to come with well written unit or integration tests (when appropriate). Below are some coverage percentages for the released modules:
* SoundFingerprinting - 83%
* SoundFingerprinting.Audio.Bass - 82%
* SoundFingerprinting.Audio.NAudio - 78%
* SoundFingerprinting.SQL - 76%
* SoundFingerprinting.MongoDb - 98%

These coverage percentages are given only for the reference, they do not neceserally mean the code is without bugs (which is obvisouly not true).

### FAQ
- Can I apply this algorithm for speech recognition purposes?
No. The granularity of one fingerprint is roughly ~1.86 seconds, thus any sound recording which is less than that will be disregarded.
- Can the algorithm detect exact query position in resulted track?
Yes. Starting from version 2.0.0 it's possible to detect exact query snippet position within the best-matched track.

## Binaries
    git clone git@github.com:AddictedCS/soundfingerprinting.git
    
In order to build latest version of the <code>SoundFingerprinting</code> assembly run the following command from repository root

    .\build.cmd
### Get it on NuGet

    Install-Package SoundFingerprinting

## Demo
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting)
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/SoundFingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

## Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

## Licence
The framework is provided under [GPLv3](http://www.gnu.org/licenses/gpl.html) licence agreement.

The framework implements the algorithm from [Content Fingerprinting Using Wavelets](http://www.nhchau.com/files/cvmp_BalujaCovell.A4color.pdf) paper.

&copy; Soundfingerprinting, 2010-2015, ciumac.sergiu@gmail.com
