## Audio fingerprinting and recognition in .NET

[![Join the chat at https://gitter.im/soundfingerprinting/Lobby](https://badges.gitter.im/soundfingerprinting/Lobby.svg)](https://gitter.im/soundfingerprinting/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

_soundfingerprinting_ is a C# framework designed for developers, enthusiasts, researchers in the fields of audio and digital signal processing, data mining and audio recognition.  It implements an efficient algorithm which provides fast insert and retrieval of acoustic fingerprints with high precision and recall rate.

[![Build Status](https://travis-ci.org/AddictedCS/soundfingerprinting.png)](https://travis-ci.org/AddictedCS/soundfingerprinting)

Donate 5$ to _soundfingerprinting_ via [Paypal](https://www.paypal.me/AddictedCS/5).

## Documentation

Below code snippet shows how to extract acoustic fingerprints from an audio file and later use them as identifiers to recognize unknown audio query. These _sub-fingerprints_ (or _fingerprints_, 2 terms are used interchangeably) will be stored in a configurable backend. The interfaces for fingerprinting and querying audio files are implemented as [Fluent Interfaces](http://martinfowler.com/bliki/FluentInterface.html).

```csharp
private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
private readonly IAudioService audioService = new NAudioService(); // use NAudio audio processing library
private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();

public void StoreAudioFileFingerprintsInStorageForLaterRetrieval(string pathToAudioFile)
{
    var track = new TrackData("GBBKS1200164", "Adele", "Skyfall", "Skyfall", 2012, 290);
	
    // store track metadata in the datasource
    var trackReference = modelService.InsertTrack(track);

    // create hashed fingerprints
    var hashedFingerprints = fingerprintCommandBuilder
                                .BuildFingerprintCommand()
                                .From(pathToAudioFile)
                                .UsingServices(audioService)
                                .Hash()
                                .Result;
								
    // store hashes in the database for later retrieval
    modelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
}
```
The default storage, which comes bundled with _soundfingerprinting_ package, is a plain RAM storage, managed by <code>InMemoryModelService</code>. The following list of persistent storages is available for general use: 
- Starting with v3.2.0 <code>InMemoryModelService</code> can be serialized to filesystem, and reloaded on application startup. Useful for scenarious when you don't want to introduce external data storages.
- ***Solr*** highly efficient non-relational storage [soundfingerprinting.solr](https://github.com/AddictedCS/soundfingerprinting.solr). Considered as the default option for production scenarious which require high number of fingerprinted tracks, highly optimized for both query and insertion.
- ***MSSQL*** [soundfingerprinrint.sql](https://github.com/AddictedCS/soundfingerprinting.sql).

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
    
    return queryResult.BestMatch.Track; // successful match has been found
}
```
### Query result details
Every `ResultEntry` object will contain the following information:
- `Track` - matched track from the datastore
- `QueryMatchLength` - returns how many query seconds matched the resulting track
- `QueryMatchStartsAt` - returns time position where resulting track started to match in the query
- `TrackMatchStartsAt` - returns time position where the query started to match in the resulting track
- `TrackStartsAt` - returns an approximation where does the matched track starts, always relative to the query
- `Coverage` - returns a value between [0, 1], informing how much the query covered the resulting track (i.e. a 2 minutes query found a 30 seconds track within it, starting at 100th second, coverage will be equal to (120 - 100)/30 ~= 0.66)
- `Confidence` - returns a value between [0, 1]. A value below 0.15 is most probably a false positive. A value bigger than 0.15 is very likely to be an exact match. For good audio quality queries you can expect getting a confidence > 0.5.

`Stats` contains useful statistics information for fine-tuning the algorithm:
- `QueryDuration` - time in milliseconds spend just querying the fingerprints datasource.
- `FingerprintingDuration` - time in milliseconds spent generating the acousting fingerprints from the media file.
- `TotalTracksAnalyzed` - total # of tracks analyzed during query time. If this number exceeds 50, try optimizing your configuration.
- `TotalFingerprintsAnalyzed` - total # of fingerprints analyzed during query time. If this number exceeds 500, try optimizing your configuration.

### Upgrade from 2.x to 3.x
All users of _soundfingerprinting_ are encouraged to migrate to v3.x due to all sorts of important bug-fixes and improvements. Version 3.2.0 is faster, more accurate, and provides an intuitive response interface with additional information about the query and the match. When migrating make sure to re-insert the fingerprints into the datasource, since their internal signature changed slightly.

### List of additional soundfingerprinting integrations
- [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) - Bass.Net audio library integration, comes as a replacement for NAudio default service. Works faster, more accurate resampling, supports multiple audio formats, independent upon target OS. [Bass](http://www.un4seen.com) is free for non-comercial use.
- All demo apps are now located in separate git repositories, [duplicates detector](https://github.com/AddictedCS/soundfingerprinting.duplicatesdetector), [sound tools](https://github.com/AddictedCS/soundfingerprinting.soundtools).

### Algorithm configuration
Fingerprinting and Querying algorithms can be easily parametrized with corresponding configuration objects passed as parameters on command creation.

```csharp
 var hashDatas = fingerprintCommandBuilder
                           .BuildFingerprintCommand()
                           .From(samples)
                           .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                           .UsingServices(audioService)
                           .Hash()
                           .Result;
```
Similarly during query time you can specify a more high precision query configuration in case if you are trying to detect audio in noisy environments.

```csharp
QueryResult queryResult = queryCommandBuilder.BuildQueryCommand()
                                   .From(PathToFile)
                                   .WithQueryConfig(new HighPrecisionQueryConfiguration())
                                   .UsingServices(modelService, audioService)
                                   .Query()
                                   .Result;
```
There are 3 pre-built configurations to choose from: `LowLatency`, `Default`, `HighPrecision`. Nevertheless you are not limited to use just these 3. You can ammed each particular configuration property by your own via overloads. 

The most sensitive parameter (which directly affects precision/recall rate) is <code>Stride</code> parameter. Empirically it was determined that using a smaller stride during querying gives a better recall rate, at the expense of execution time.

In case you need directions for fine-tunning the algorithm for your particular use case do not hesitate to contact me.

### Third party dependencies
Links to the third party libraries used by _soundfingerprinting_ project.
* [NAudio](http://naudio.codeplex.com)
* [Ninject](http://www.ninject.org)
* [LomontFFT](http://www.lomont.org/Software/Misc/FFT/LomontFFT.html)
* [ProtobufNet](https://github.com/mgravell/protobuf-net)

### FAQ
- Can I apply this algorithm for speech recognition purposes?
> No. The granularity of one fingerprint is roughly ~1.46 seconds.
- Can the algorithm detect exact query position in resulted track?
> Yes.
- Can I use **SoundFingerprinting** to detect ads in radio streams?
> Yes.
- Will **SoundFingerprinting** match tracks with samples captured in noisy environment?
> Yes, try out `HighPrecision` configurations.
- Can I use **SoundFingerprinting** framework on **Mono**?
> Yes. SoundFingerprinting can be used in cross-platform applications. Just keep in mind that the default audio service **NAudio**, requires Windows native DLLs. Since these are not available in Unix, you can use the override method which asks for `AudioSamples` as the source for fingerprinting and querying. It's the responsability of the caller to provide mono audio samples at 5512 frequency rate. If this condition is met, the algorithm will not invoke any methods from NAudio.
- How many tracks can I store in `InMemoryModelService`?
> 100 hours of content with `HighPrecision` fingerprinting configuration will yeild in ~5GB or RAM usage.

### Binaries

    git clone git@github.com:AddictedCS/soundfingerprinting.git    
In order to build latest version of the **SoundFingerprinting** assembly run the following command from repository root.

    .\build.cmd
### Get it on NuGet

    Install-Package SoundFingerprinting
### Demo
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting)
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/SoundFingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

### Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

### License
The framework is provided under [MIT](https://opensource.org/licenses/MIT) license agreement. The theoretical description of the algorithm can be read in [Content Fingerprinting using Wavelets](http://static.googleusercontent.com/media/research.google.com/en/pubs/archive/32685.pdf) paper.

Special thanks to [JetBrains](https://www.jetbrains.com/) for providing this project with a license for [ReSharper](https://www.jetbrains.com/resharper/)!

![JetBrains](http://blog.jetbrains.com/webide/files/2012/12/logo_JB_tagline-300x108.png)

&copy; Soundfingerprinting, 2010-2017, ciumac.sergiu@gmail.com
