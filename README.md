## Audio fingerprinting and recognition in .NET

[![Join the chat at https://gitter.im/soundfingerprinting/Lobby](https://badges.gitter.im/soundfingerprinting/Lobby.svg)](https://gitter.im/soundfingerprinting/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
![.NET Core](https://github.com/AddictedCS/soundfingerprinting/workflows/.NET%20Core/badge.svg)
[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](license.txt)
[![NuGet](https://img.shields.io/nuget/dt/SoundFingerprinting.svg)](https://www.nuget.org/packages/SoundFingerprinting)

_soundfingerprinting_ is a C# framework designed for companies, enthusiasts, researchers in the fields of audio and digital signal processing, data mining and audio recognition. It implements an efficient algorithm which provides fast insert and retrieval of acoustic fingerprints with high precision and recall rate.
Paired with [SoundFingerprinting.Emy][emy-nuget] it can be used to generate fingerprints from video content as well.

## Documentation

Full documentation is available on the [Wiki][wiki-page] page.

Below code snippet shows how to extract acoustic fingerprints from an audio file and later use them as identifiers to recognize unknown audio query. These _fingerprints_ will be stored in a configurable datastore.

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

    docker run -d -v /persistent-dir:/app/data -p 3399:3399 -p 3340:3340 addictedcs/soundfingerprinting.emy:latest

**Emy** provides a backoffice interface which you can access on port :3340. 
In order to insert and query **Emy** server please install [SoundFingerprinting.Emy][emy-nuget] NuGet package.

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

Similarly, [SoundFingerprinting.Emy][emy-nuget] provides `FFmpegAudioService`, which supports a wide variety of formats for both audio and video fingerprinting. More details about `FFmpegAudioService` can be found below.

<img src="https://i.imgur.com/lhqUY74.png" width="800">

If you plan to use **Emy** storage in a commercial project please contact sergiu@emysound.com for details. Enterprise version is ~12.5x faster when number of tracks exceeds ~10K, supports clustering, replication and much more. By using **Emy** you will also support core SoundFingerprinting library and its ongoing development.
More details can be found [here][emysound].

Previous storages are now considered deprecate, as **Emy** is now considered the default choice for persistent storage. 

- ***Solr*** non-relational storage [soundfingerprinting.solr](https://github.com/AddictedCS/soundfingerprinting.solr). MIT licensed, useful when the number of tracks does not exceed 5000 tracks [deprecated].
- ***MSSQL*** [soundfingerprinrint.sql](https://github.com/AddictedCS/soundfingerprinting.sql) [deprecated]. MIT licensed.

### Supported audio formats
Read [Supported Audio Formats](https://github.com/AddictedCS/soundfingerprinting/wiki/Supported-Audio-Formats) page for details about different audio services and how you can use them in various operating systems.

### Query result details
Every `ResultEntry` object will contain the following information:
- `Track` - matched track from the datastore
- `QueryMatchLength` - returns how many query seconds matched the resulting track
- `QueryMatchStartsAt` - returns time position where resulting track started to match in the query
- `TrackMatchStartsAt` - returns time position where the query started to match in the resulting track
- `TrackStartsAt` - returns an approximation where does the matched track starts, always relative to the query
- `Confidence` - returns a value between [0, 1]. A value below 0.15 is most probably a false positive. A value bigger than 0.15 is very likely to be an exact match. For good audio quality queries you can expect getting a confidence > 0.5.
- `MatchedAt` - returns timestamp showing at what time did the match occured. Usefull for realtime queries.

`Stats` contains useful statistics information for fine-tuning the algorithm:
- `QueryDuration` - time in milliseconds spend just querying the fingerprints datasource.
- `FingerprintingDuration` - time in milliseconds spent generating the acousting fingerprints from the media file.
- `TotalTracksAnalyzed` - total # of tracks analyzed during query time. If this number exceeds 50, try optimizing your configuration.
- `TotalFingerprintsAnalyzed` - total # of fingerprints analyzed during query time. If this number exceeds 500, try optimizing your configuration.

Read [Different Types of Coverage](https://github.com/AddictedCS/soundfingerprinting/wiki/Different-Types-of-Coverage) to understand how query coverage is calculated.

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
[Audio Fingerprinting][emysound-how-it-works].
[Video Fingerprinting][emysound-video-fingerprinting].

    
### Demo
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting). The article is from 2011, and may be outdated.
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/SoundFingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

### Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

### License
The framework is provided under [MIT](https://opensource.org/licenses/MIT) license agreement.

&copy; Soundfingerprinting, 2010-2021, sergiu@emysound.com


[emy-nuget]: https://www.nuget.org/packages/SoundFingerprinting.Emy
[emysound-how-it-works]: https://emysound.com/blog/open-source/2020/06/12/how-audio-fingerprinting-works.html
[emysound-video-fingerprinting]: https://emysound.com/blog/open-source/2021/08/01/video-fingerprinting.html
[emysound]: https://emysound.com
[wiki-page]: https://github.com/AddictedCS/soundfingerprinting/wiki
