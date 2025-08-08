## Audio/Video fingerprinting and recognition in .NET

[![Join the chat at https://gitter.im/soundfingerprinting/Lobby](https://badges.gitter.im/soundfingerprinting/Lobby.svg)](https://gitter.im/soundfingerprinting/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
![.NET Core](https://github.com/AddictedCS/soundfingerprinting/workflows/.NET%20Core/badge.svg)
[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](license.txt)
[![NuGet](https://img.shields.io/nuget/dt/SoundFingerprinting.svg)](https://www.nuget.org/packages/SoundFingerprinting)

_soundfingerprinting_ is a C# framework designed for companies, enthusiasts, researchers in the fields of digital signal processing, data mining and audio/video recognition. It implements an efficient algorithm which provides fast insert and retrieval of acoustic and video fingerprints with high precision and recall rate.

## Documentation

Full documentation is available on the [Wiki][wiki-page] page.

Below code snippet shows how to extract acoustic fingerprints from an audio file and later use them as identifiers to recognize unknown audio query. These _fingerprints_ will be stored in a configurable datastore.

```csharp
private readonly IModelService modelService = new InMemoryModelService(); // store fingerprints in RAM
private readonly IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library

public async Task StoreForLaterRetrieval(string file)
{
    var track = new TrackInfo("GBBKS1200164", "Skyfall", "Adele");

    // create fingerprints
    var avHashes = await FingerprintCommandBuilder.Instance
                                .BuildFingerprintCommand()
                                .From(file)
                                .UsingServices(audioService)
                                .Hash();
								
    // store hashes in the database for later retrieval
    modelService.Insert(track, avHashes);
}
```

### Querying
Once you've inserted the fingerprints into the datastore, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, URL, microphone, radio tuner, etc. It's up to your application, where you get the samples from.

```csharp

public async Task<TrackData> GetBestMatchForSong(string file)
{
    int secondsToAnalyze = 10; // number of seconds to analyze from query file
    int startAtSecond = 0; // start at the begining
	
    // query the underlying database for similar audio sub-fingerprints
    var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                                         .From(file, secondsToAnalyze, startAtSecond)
                                         .UsingServices(modelService, audioService)
                                         .Query();
    
    return queryResult.BestMatch.Track;
}
```
### Fingerprints Storage
The default storage, which comes bundled with _soundfingerprinting_ NuGet package, is a plain in-memory storage, available via <code>InMemoryModelService</code> class. If you plan to use an external persistent storage for fingerprints **Emy** is the preferred choice. **Emy** provides a community version which is free for non-commercial use. More about **Emy** can be found [on wiki page][emy-wiki-page].

### Supported audio/video formats
Read [Supported Media Formats][audio-services-wiki-page] page for details about processing different file formats or realtime streams.

### Video fingerprinting support since version 8.0.0
Since `v8.0.0` video fingerprinting support has been added. Similarly to audio fingerprinting, video fingerprints are generated from video frames, and used to insert and later query the datastore for exact and similar matches. You can use `SoundFingerprinting` to fingerprint either audio or video content or both at the same time. More details about video fingerprinting are available [here][video-fingerprinting-wiki-page].

### Version Matrix
If you are using `FFmpegAudioService` as described in the [wiki][audio-services-wiki-page], follow the below version matrix.
| SoundFingerprinting  | SoundFingerprinting.Emy | FFmpeg |
| ---- | ------ |-----|
| 8.x  | 8.x    | 4.x |
| 9.x  | 9.x    | 5.x |
| 10.x | 10.x   | 6.x |
| 11.x | 11.x   | 6.x |
| 12.x | 12.x   | 7.x |



### FAQ
- Can I apply this algorithm for speech recognition purposes?
> No. The granularity of one fingerprint is roughly ~1.46 seconds.
- Can the algorithm detect exact query position in resulted track?
> Yes.
- Can I use **SoundFingerprinting** to detect ads in radio streams?
> Yes. Actually this is the most frequent use-case where SoundFingerprinting was successfully used.
- How many tracks can I store in `InMemoryModelService`?
> 100 hours of content with `DefaultFingerprintingConfiguration` will consume ~5GB of RAM.

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

&copy; Soundfingerprinting, 2010-2025, sergiu@emysound.com


[emy-nuget]: https://www.nuget.org/packages/SoundFingerprinting.Emy
[emysound-how-it-works]: https://emysound.com/blog/open-source/2020/06/12/how-audio-fingerprinting-works.html
[emysound-video-fingerprinting]: https://emysound.com/blog/open-source/2021/08/01/video-fingerprinting.html
[emysound]: https://emysound.com
[wiki-page]: https://github.com/AddictedCS/soundfingerprinting/wiki
[emy-wiki-page]: https://github.com/AddictedCS/soundfingerprinting/wiki/Emy-Storage
[audio-services-wiki-page]: https://github.com/AddictedCS/soundfingerprinting/wiki/Audio-Services
[video-fingerprinting-wiki-page]: https://github.com/AddictedCS/soundfingerprinting/wiki/Video-Fingerprints
