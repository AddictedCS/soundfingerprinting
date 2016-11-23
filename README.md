## Audio fingerprinting and recognition in .NET

_soundfingerprinting_ is a C# framework designed for developers, enthusiasts, researchers in the fields of audio and digital signal processing, data mining, and alike.  It implements an [efficient algorithm](http://static.googleusercontent.com/media/research.google.com/en/pubs/archive/32685.pdf) of digital signal processing which allows developing a system of acoustic fingerprinting and recognition in .NET.

[![Build Status](https://travis-ci.org/AddictedCS/soundfingerprinting.png)](https://travis-ci.org/AddictedCS/soundfingerprinting)

## Documentation

Below code sample that shows you how to extract unique characteristics from an audio file and later use them as identifiers to recognize unknown snippets from a variety of sources. These characteristics known as _sub-fingerprints_ (or _fingerprints_, these 2 terms are used interchangeably) will be stored in the configurable backend. The interfaces for fingerprinting and querying audio files have been implemented as [Fluent Interfaces](http://martinfowler.com/bliki/FluentInterface.html) with [Builder](http://en.wikipedia.org/wiki/Builder_pattern) and [Command](http://en.wikipedia.org/wiki/Command_pattern) patterns in mind.
```csharp
private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in memory
private readonly IAudioService audioService = new NAudioService(); // use NAudio audio processing library
private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();

public void StoreAudioFileFingerprintsInStorageForLaterRetrieval(string pathToAudioFile)
{
    var track = new TrackData("GBBKS1200164", "Adele", "Skyfall", "Skyfall", 2012, 290);
	
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
    modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
}
```
The default storage, which comes bundled with _soundfingerprinting_ package, is a plain RAM storage, managed by <code>InMemoryModelService</code>. Other storages are available
- SOLR, highly efficient non-relational storage [soundfingerprinting.solr](https://github.com/AddictedCS/soundfingerprinting.solr)
- MSSQL [soundfingerprinrint.sql](https://github.com/AddictedCS/soundfingerprinting.sql)
- MongoDB (in development) [soundfingerprinting.mongodb](https://github.com/AddictedCS/soundfingerprinting.mongodb)

Once you've inserted the fingerprints into the datastore, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, URL, microphone, radio tuner, etc. It's up to your application, where you get the samples from.

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
    if(queryResult.ContainsMatches)
    {
        return queryResult.BestMatch.Track; // successful match has been found
    }
	
    return null; // no match has been found
}
```
See the [Wiki Page](https://github.com/AddictedCS/soundfingerprinting/wiki) for the operational details and information. 

### Upgrade from 2.x to 3.x
All users of _soundfingerprinting_ are encouraged to migrate to v3.x due to all sorts of important bug-fixes and improvements. Most importantly _Query_ method from _QueryCommand_ class is now unified, it will return both best matches as well as additional match information.
Every `ResultEntry` object will contain the following information:
- `Track` - matched track from the datastore
- `QueryMatchLength` - return how many query seconds matched the resulting track
- `TrackStartsAt` - returns where does the matched track starts, always relative to the query
- `Coverage` - returns a value between [0, 1], informing how much the query covered the resulting track (i.e. a 2 minutes query found a 30 seconds track within it, starting at 100th second, coverage will be equal to (120 - 100)/30 ~= 0.66)
- `Confidence` - returns a value between [0, 1]. A value below 0.15 is most probably a false positive. A value bigger than 0.15 is very likely to be an exact match

### List of all soundfingerprinting integrations
- [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) - Bass.Net audio library integration. Works faster, more accurate resampling, independent upon target OS.
- [SoundFingerprinrint.Solr](https://github.com/AddictedCS/soundfingerprinting.solr) - SOLR integration (first choice for persistent storage). Highly scalable non-relational storage.
- [SoundFingerprinrint.SQL](https://github.com/AddictedCS/soundfingerprinting.sql) - MSSQL integration 
- and all demo apps are now located in separate git repositories, [duplicates detector](https://github.com/AddictedCS/soundfingerprinting.duplicatesdetector), [sound tools](https://github.com/AddictedCS/soundfingerprinting.soundtools).

### Extension capabilities
Some of the interfaces which are used by the framework can be easily substituted according to your needs. In case you don't want to use _NAudio_ as your audio library, you can take advantage of Bass.Net integration available through [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) package.

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

#### Changes in default algorithm
The most sensitive parameter (which directly affects precision/recall rate) is the <code>Stride</code> parameter. Empirically it was determined that using a smaller stride during querying gives both better precision and recall rate, at the expense of execution time and CPU load. 

Starting from release 2.1.x new class has been introduced <code>EfficientFingerprintConfigurationForQuerying</code> which overrides default <i>query</i> stride (previously set to <code>IncrementalStaticStride</code> with <code>0.928ms</code>). Fingerprint stride remains the same as in previous versions <code>DefaultFingerprintConfiguration</code>. This change slows down querying but increases both precision and recall rate.

In case you need directions for fine-tunning the algorithm for your particular use case do not hesitate to contact me.

### Third party dependencies
Links to the third party libraries used by _soundfingerprinting_ project.
* [NAudio](http://naudio.codeplex.com)
* [Ninject](http://www.ninject.org)

### FAQ
- Can I apply this algorithm for speech recognition purposes?
No. The granularity of one fingerprint is roughly ~1.46 seconds, thus any sound recording which is less than that will be disregarded.
- Can the algorithm detect exact query position in resulted track?
Yes.
- Can I use SoundFingerprinting to detect ads in radio streams?
Yes.
- Will SoundFingerprinting match tracks with samples captured in noisy environment?
Yes, but you will have to play around with `Stride` (decreasing it) and `ThresholdVotes` query parameter (decreasing it as well).

## Binaries
    git clone git@github.com:AddictedCS/soundfingerprinting.git
    
In order to build latest version of the <code>SoundFingerprinting</code> assembly run the following command from repository root

    .\build.cmd
### Get it on NuGet

    Install-Package SoundFingerprinting -Pre

## Demo
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting)
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/SoundFingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

## Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

## Licence
The framework is provided under [MIT](https://opensource.org/licenses/MIT) licence agreement.

&copy; Soundfingerprinting, 2010-2016, ciumac.sergiu@gmail.com
