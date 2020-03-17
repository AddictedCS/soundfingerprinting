## Audio fingerprinting and recognition in .NET

[![Join the chat at https://gitter.im/soundfingerprinting/Lobby](https://badges.gitter.im/soundfingerprinting/Lobby.svg)](https://gitter.im/soundfingerprinting/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
![.NET Core](https://github.com/AddictedCS/soundfingerprinting/workflows/.NET%20Core/badge.svg)
[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](license.txt)
[![NuGet](https://img.shields.io/nuget/dt/SoundFingerprinting.svg)](https://www.nuget.org/packages/SoundFingerprinting)

_soundfingerprinting_ is a C# framework designed for companies, enthusiasts, researchers in the fields of audio and digital signal processing, data mining and audio recognition. It implements an efficient algorithm which provides fast insert and retrieval of acoustic fingerprints with high precision and recall rate.

## Documentation

Below code snippet shows how to extract acoustic fingerprints from an audio file and later use them as identifiers to recognize unknown audio query. These _sub-fingerprints_ (or _fingerprints_, two terms used interchangeably) will be stored in a configurable datastore.

```csharp
private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
private readonly IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library

public async Task StoreForLaterRetrieval(string pathToAudioFile)
{
    var track = new TrackInfo("GBBKS1200164", "Skyfall", "Adele");

    // create fingerprints
    var hashedFingerprints = await FingerprintCommandBuilder.Instance
                                .BuildFingerprintCommand()
                                .From(pathToAudioFile)
                                .UsingServices(audioService)
                                .Hash();
								
    // store hashes in the database for later retrieval
    modelService.Insert(track, hashedFingerprints);
}
```

### Querying
Once you've inserted the fingerprints into the datastore, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, URL, microphone, radio tuner, etc. It's up to your application, where you get the samples from.

```csharp

public async Task<TrackData> GetBestMatchForSong(string queryAudioFile)
{
    int secondsToAnalyze = 10; // number of seconds to analyze from query file
    int startAtSecond = 0; // start at the begining
	
    // query the underlying database for similar audio sub-fingerprints
    var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                                         .From(queryAudioFile, secondsToAnalyze, startAtSecond)
                                         .UsingServices(modelService, audioService)
                                         .Query();
    
    return queryResult.BestMatch.Track;
}
```
### Fingerprints Storage
The default storage, which comes bundled with _soundfingerprinting_ NuGet package, is a plain in-memory storage, available via <code>InMemoryModelService</code> class. If you plan to use an external persistent storage for audio fingerprints **Emy** is the preferred choice. It is a specialized storage developed for audio fingerprints. **Emy** provides a community version which is free for non-commercial use. You can try it with docker:

    docker run -p 3399:3399 -p 3340:3340 addictedcs/soundfingerprinting.emy

**Emy** provides a backoffice interface which you can access on port :3340. 
In order to insert and query **Emy** server please install [SoundFingerprinting.Emy](https://www.nuget.org/packages/SoundFingerprinting.Emy) NuGet package.

    Install-Package SoundFingerprinting.Emy
    
The package will provide you with <code>EmyModelService</code> class, which can substitute default <code>InMemoryModelService</code>.
```csharp
 // connect to Emy on port 3399
 var emyModelService = EmyModelService.NewInstance("localhost", 3399);
 
 // query Emy database
 var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                                         .From(queryAudioFile, secondsToAnalyze, startAtSecond)
                                         .UsingServices(modelService, audioService)
                                         .Query();
					
// register matches s.t. they appear in the dashboard					
emyModelService.RegisterMatches(queryResult.ResultEntries);
```
Registering matches is now possible with <code>EmyModelService</code>. The results will be displayed in the **Emy** dashboard.

<img src="https://i.imgur.com/lhqUY74.png" width="800">

If you plan to use **Emy** storage in a commercial project please contact sergiu@emysound.com for details. Enterprise version is ~12.5x faster when number of tracks exceeds ~10K, supports clustering, replication and much more. By using **Emy** you will also support core SoundFingerprinting library and its ongoing development.

Previous storages are now considered deprecate, as **Emy** is now considered the default choice for persistent storage. 

- ***Solr*** non-relational storage [soundfingerprinting.solr](https://github.com/AddictedCS/soundfingerprinting.solr). MIT licensed, useful when the number of tracks does not exceed 5000 tracks [deprecated].
- ***MSSQL*** [soundfingerprinrint.sql](https://github.com/AddictedCS/soundfingerprinting.sql) [deprecated]. MIT licensed.

### Query result details
Every `ResultEntry` object will contain the following information:
- `Track` - matched track from the datastore
- `QueryMatchLength` - returns how many query seconds matched the resulting track
- `QueryMatchStartsAt` - returns time position where resulting track started to match in the query
- `TrackMatchStartsAt` - returns time position where the query started to match in the resulting track
- `TrackStartsAt` - returns an approximation where does the matched track starts, always relative to the query
- `Coverage` - returns a value between [0, 1], informing how much the query covered the resulting track (i.e. a 2 minutes query found a 30 seconds track within it, starting at 100th second, coverage will be equal to (120 - 100)/30 ~= 0.66)
- `Confidence` - returns a value between [0, 1]. A value below 0.15 is most probably a false positive. A value bigger than 0.15 is very likely to be an exact match. For good audio quality queries you can expect getting a confidence > 0.5.
- `MatchedAt` - returns timestamp showing at what time did the match occured. Usefull for realtime queries.

`Stats` contains useful statistics information for fine-tuning the algorithm:
- `QueryDuration` - time in milliseconds spend just querying the fingerprints datasource.
- `FingerprintingDuration` - time in milliseconds spent generating the acousting fingerprints from the media file.
- `TotalTracksAnalyzed` - total # of tracks analyzed during query time. If this number exceeds 50, try optimizing your configuration.
- `TotalFingerprintsAnalyzed` - total # of fingerprints analyzed during query time. If this number exceeds 500, try optimizing your configuration.

### Version 6.2.0
Version 6.2.0 provides ability to query realtime datasources. Usefull for scenarious when you would like to monitor a realtime stream and get matching results as fast as possible.

### Version 6.0.0
Version 6.0.0 provides a slightly improved `IModelService` interface. Now you can insert `TrackInfo` and it's corresponding fingerprints in one method call. The signatures of the fingerprints stayed the same, no need to re-index your tracks. Also, instead of inserting `TrackData` objects a new lightweight data class has been added: `TrackInfo`.

### Version 5.2.0
Version 5.2.0 provides a query configuration option `AllowMultipleMatchesOfTheSameTrackInQuery` which will instruct the framework to consider the use case of having the same track matched multiple times within the same query. This is handy for long queries that can contain same match scattered across the query. Default value is `false`.

### Version 5.1.0
Starting from version 5.1.0 the fingerprints signature has changed to be more resilient to noise. You can try `HighPrecisionFingerprintConfiguration` in case your audio samples come from recordings that contain ambient noise. All users that migrate to 5.1.x have to re-index the data, since fingerprint signatures from <= 5.0.x version are not compatible.

### Version 5.0.0
Starting from version 5.0.0 _soundfingerprinting_ library supports .NET Standard 2.0. You can run the application not only on Window environment but on any other .NET Standard [compliant](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) runtime.

### List of additional soundfingerprinting integrations
Default `SoundFingerprintingAudioService` supports only wave file at the input. If you would like to process other formats, consider using below extensions:
- [SoundFingerprinting.Audio.NAudio](https://www.nuget.org/packages/SoundFingerprinting.Audio.NAudio) - replacement for default `SoundFingerprintingAudioService` audio service. Provides support for *.mp3* audio processing. Runs only on Windows as it uses [NAudio](https://github.com/naudio/NAudio) framework for underlying decoding and resampling.
- [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) - Bass.Net audio library integration, comes as a replacement for default service. Works faster than the default or NAudio, more accurate resampling, supports multiple audio formats (*.wav*, *.ogg*, *.mp3*, *.flac*). [Bass](http://www.un4seen.com) is free for non-comercial use. Recommended for enterprise users.
- All demo apps are now located in separate git repositories, [duplicates detector](https://github.com/AddictedCS/soundfingerprinting.duplicatesdetector), [sound tools](https://github.com/AddictedCS/soundfingerprinting.soundtools).

### Algorithm configuration
Fingerprinting and Querying algorithms can be easily parametrized with corresponding configuration objects passed as parameters on command creation.

```csharp
 var hashDatas = await FingerprintCommandBuilder.Instance
                           .BuildFingerprintCommand()
                           .From(samples)
                           .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                           .UsingServices(audioService)
                           .Hash();
```
Similarly during query time you can specify a more high precision query configuration in case if you are trying to detect audio in noisy environments.

```csharp
QueryResult queryResult = await QueryCommandBuilder.Instances
                                   .BuildQueryCommand()
                                   .From(PathToFile)
                                   .WithQueryConfig(new HighPrecisionQueryConfiguration())
                                   .UsingServices(modelService, audioService)
                                   .Query();
```
There are 3 pre-built configurations to choose from: `LowLatency`, `Default`, `HighPrecision`. Nevertheless you are not limited to use just these 3. You can ammed each particular configuration property by your own via overloads.

In case you need directions for fine-tunning the algorithm for your particular use case do not hesitate to contact me. Specifically if you are trying to use it on mobile platforms `HighPrecisionFingerprintConfiguration` may not be accurate enought.

Please use fingerprinting configuration counterpart during query (i.e. `HighPrecisionFingerprintConfiguration` with `HighPrecisionQueryConfiguration`). Different configuration analyze different spectrum ranges, thus they have to be used in pair.

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
> Yes. Actually this is the most frequent use-case where SoundFingerprinting was successfully used.
- Will **SoundFingerprinting** match tracks with samples captured in noisy environment?
> Yes, try out `HighPrecision` configurations, or contact me for additional guidance.
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
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting). The article is from 2011, and may be outdated.
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/SoundFingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

### Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

### License
The framework is provided under [MIT](https://opensource.org/licenses/MIT) license agreement.

&copy; Soundfingerprinting, 2010-2019, sergiu@emysound.com
