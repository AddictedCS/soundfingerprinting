## Audio fingerprinting and recognition in .NET

[![Join the chat at https://gitter.im/soundfingerprinting/Lobby](https://badges.gitter.im/soundfingerprinting/Lobby.svg)](https://gitter.im/soundfingerprinting/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

_soundfingerprinting_ is a C# framework designed for companies, enthusiasts, researchers in the fields of audio and digital signal processing, data mining and audio recognition. It implements an efficient algorithm which provides fast insert and retrieval of acoustic fingerprints with high precision and recall rate.

[![Build Status](https://travis-ci.org/AddictedCS/soundfingerprinting.png)](https://travis-ci.org/AddictedCS/soundfingerprinting)

## Documentation

Below code snippet shows how to extract acoustic fingerprints from an audio file and later use them as identifiers to recognize unknown audio query. These _sub-fingerprints_ (or _fingerprints_, 2 terms are used interchangeably) will be stored in a configurable backend. The interfaces for fingerprinting and querying audio files are implemented as [Fluent Interfaces](http://martinfowler.com/bliki/FluentInterface.html).

```csharp
private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
private readonly IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library

public void StoreAudioFileFingerprintsInStorageForLaterRetrieval(string pathToAudioFile)
{
    var track = new TrackData("GBBKS1200164", "Adele", "Skyfall", "Skyfall", 2012, 290);
	
    // store track metadata in the datasource
    var trackReference = modelService.InsertTrack(track);

    // create hashed fingerprints
    var hashedFingerprints = FingerprintCommandBuilder.Instance
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
- ***SoundFingerprinting.Emy*** contact ciumac.sergiu@gmail.com for early access to a enterprise fingerprinting storage that is both super fast and resilient.
- ***Solr*** efficient non-relational storage [soundfingerprinting.solr](https://github.com/AddictedCS/soundfingerprinting.solr). Useful for scenarious when you don't exceed 1000 tracks.
- ***MSSQL*** [soundfingerprinrint.sql](https://github.com/AddictedCS/soundfingerprinting.sql) [deprecated].

Once you've inserted the fingerprints into the datastore, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, URL, microphone, radio tuner, etc. It's up to your application, where you get the samples from.

```csharp

public TrackData GetBestMatchForSong(string queryAudioFile)
{
    int secondsToAnalyze = 10; // number of seconds to analyze from query file
    int startAtSecond = 0; // start at the begining
	
    // query the underlying database for similar audio sub-fingerprints
    var queryResult = QueryCommandBuilder.Instance.BuildQueryCommand()
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

### Version 5.2.0
Version 5.2.0 provides a query configuration option `AllowMultipleMatchesOfTheSameTrackInQuery` which will instruct the framework to consider the scenario of having the same track matched multiple times within the same query. This is handy for long queries that can contain same match scattered across the query. Default value is `false`.

### Version 5.1.0
Starting from version 5.1.0 the fingerprints signature has changed to be more resilient to noise. You can try `HighPrecisionFingerprintConfiguration` in case your audio samples come from recordings that contain ambient noise. All users that migrate to 5.1.x have to re-index the data, since fingerprint signatures from 5.x version are not compatible.

### Version 5.0.0
Starting from version 5.0.0 _soundfingerprinting_ library supports .NET Standard 2.0. You can run the application not only on Window environment but on any other .NET Standard [compliant](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) runtime.

### List of additional soundfingerprinting integrations
- [SoundFingerprinting.Audio.NAudio](https://www.nuget.org/packages/SoundFingerprinting.Audio.NAudio) - replacement for default `SoundFingerprintingAudioService` audio service. Provides support for *.mp3* audio processing. Runs only on Windows as it uses [NAudio](https://github.com/naudio/NAudio) framework for underlying decoding and resampling.
- [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) - Bass.Net audio library integration, comes as a replacement for default service. Works faster than the default or NAudio, more accurate resampling, supports multiple audio formats (*.wav*, *.ogg*, *.mp3*, *.flac*). [Bass](http://www.un4seen.com) is free for non-comercial use. Recommended for enterprise users.
- All demo apps are now located in separate git repositories, [duplicates detector](https://github.com/AddictedCS/soundfingerprinting.duplicatesdetector), [sound tools](https://github.com/AddictedCS/soundfingerprinting.soundtools).

### Algorithm configuration
Fingerprinting and Querying algorithms can be easily parametrized with corresponding configuration objects passed as parameters on command creation.

```csharp
 var hashDatas = FingerprintCommandBuilder.Instance
                           .BuildFingerprintCommand()
                           .From(samples)
                           .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                           .UsingServices(audioService)
                           .Hash()
                           .Result;
```
Similarly during query time you can specify a more high precision query configuration in case if you are trying to detect audio in noisy environments.

```csharp
QueryResult queryResult = QueryCommandBuilder.Instances
                                   .BuildQueryCommand()
                                   .From(PathToFile)
                                   .WithQueryConfig(new HighPrecisionQueryConfiguration())
                                   .UsingServices(modelService, audioService)
                                   .Query()
                                   .Result;
```
There are 3 pre-built configurations to choose from: `LowLatency`, `Default`, `HighPrecision`. Nevertheless you are not limited to use just these 3. You can ammed each particular configuration property by your own via overloads.

In case you need directions for fine-tunning the algorithm for your particular use case do not hesitate to contact me. Specifically if you are trying to use it on mobile platforms `HighPrecisionFingerprintConfiguration` may not be enought.

Please fingerprinting configuration counterpart during query (i.e. `HighPrecisionFingerprintConfiguration` with `HighPrecisionQueryConfiguration`). Different configuration analyze different spectrum ranges, thus they have to be used in pair.

### Substituting audio or model services
Most critical parts of the _soundfingerprinting_ framework are interchangeable with extensions. If you want to use `NAudio` as the underlying audio processing library just install `SoundFingerprinting.Audio.NAudio` package and substitute `IAudioService` with `NAudioService`. Same holds for database storages. Install the extensions which you want to use (i.e. `SoundFingerprinting.Solr`) and provide new `ModelService` where needed. 

### Third party dependencies
Links to the third party libraries used by _soundfingerprinting_ project.
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
- Can I use **SoundFingerprinting** framework on **Mono** or .NET Core app?
> Yes. SoundFingerprinting can be used in cross-platform applications. Keep in mind though, cross platform audio service `SoundFingerprintingAudioService` supports only *.wav* files at it's input.
- How many tracks can I store in `InMemoryModelService`?
> 100 hours of content with `HighPrecision` fingerprinting configuration will yeild in ~5GB or RAM usage.

### Binaries

    git clone git@github.com:AddictedCS/soundfingerprinting.git    
In order to build latest version of the **SoundFingerprinting** assembly run the following command from repository root.

    .\build.cmd
### Get it on NuGet

    Install-Package SoundFingerprinting
### How it works
_soundfingerprinting_ employs computer vision techniques to generate audio fingerprints. The fingerprints are generated from spectrogram images taken every *N* samples. Below is a 30 seconds long non-overlaping spectrogram cut at 318-2000Hz frequency range.

![Spectrums](https://i.imgur.com/yuOY9Jh.png)

After a list of subsequent transformations these are converted into hashes, which are stored and used at query time. The fingerprints are robust to degradations to a certain degree. The `DefaultFingerprintConfiguration` class can be successfully used for radio stream monitoring. It handles well different audio formats, aliased signals and sampling differences accross tracks. Ambient noise is a different beast and you will probably need `HighPrecisionFingerprintConfiguration` to deal with it.
    
### Demo
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting)
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/SoundFingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

### Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

### License
The framework is provided under [MIT](https://opensource.org/licenses/MIT) license agreement.

Special thanks to [JetBrains](https://www.jetbrains.com/) for providing this project with a license for [ReSharper](https://www.jetbrains.com/resharper/)!

![JetBrains](http://blog.jetbrains.com/webide/files/2012/12/logo_JB_tagline-300x108.png)

&copy; Soundfingerprinting, 2010-2018, ciumac.sergiu@gmail.com
