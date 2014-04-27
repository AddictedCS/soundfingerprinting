## Sound Fingerprinting

Soundfingerprinting is a C# framework designed for developers, enthusiasts, researchers in the fields of audio processing, data mining, digital signal processing.  It implements an efficient algorithm of signal processing which allows having a competent system of audio fingerprinting and signal recognition.

## Documentation

Following is a code sample that shows you how to extract unique characteristics from an audio file and later use them as identifiers to recognize unknown snippets from a variaty of sources. These characteristics known as sub-fingerprints will be stored in the configurable backend. The interfaces for fingerprinting and querying audio files have been implemented as [Fluent Interfaces](http://martinfowler.com/bliki/FluentInterface.html) with [Builder](http://en.wikipedia.org/wiki/Builder_pattern) and [Command](http://en.wikipedia.org/wiki/Command_pattern) patterns in mind.
```csharp
private readonly IModelService modelService = new InMemoryModelService();
private readonly IAudioService audioService = new NAudioService();
private readonly IFingerprintCommandBuilder fingerprintCommandBuilder = new FingerprintCommandBuilder();

public void StoreAudioFileFingerprintsInDatabaseForLaterRetrieval(string pathToAudioFile)
{
    TrackData track = new TrackData("GBBKS1200164", "Adele", "Skyfall", "Skyfall", 2012, 290);
	
    // store track metadata in the database
    var trackReference = modelService.InsertTrack(track);

    // create sub-fingerprints and its hash representation
    var hashDatas = fingerprintCommandBuilder
                                .BuildFingerprintCommand()
                                .From(pathToAudioFile)
                                .WithDefaultFingerprintConfig()
                                .UsingServices(audioService)
                                .Hash()
                                .Result;
								
    // store sub-fingerprints and its hash representation in the database 
    modelService.InsertHashDataForTrack(hashDatas, trackReference);
}
```
The default storage, which comes bundled with SoundFingerprinting package, is a plain in memory storage, managed by <code>InMemoryModelService</code>. In case you would like to store fingerprints in a perstistent database you can take advantage of MSSQL integration available in [SoundFingerprinting.SQL](https://www.nuget.org/packages/SoundFingerprinting.SQL) package via <code>SqlModelService</code> class. The MSSQL database initialization script can be find [here](src/Scripts/DBScript.sql). Do not forget to add connection string <code>FingerprintConnectionString</code> in your app.config file.

Once you've inserted the fingerprints into the database, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, url, microphone, radio tuner, etc. It's up to your application, where you get the samples from.
```csharp
private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();

public TrackData GetBestMatchForSong(string queryAudioFile)
{
    int secondsToAnalyze = 10; // number of seconds to analyze from query file
    int startAtSecond = 0; // start at the begining
	
    // query the underlying database for similar audio sub-fingerprints
    var queryResult = queryCommandBuilder.BuildQueryCommand()
                                         .From(queryAudioFile, secondsToAnalyze, startAtSecond)
                                         .WithDefaultConfigs()
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
The code is still in active development phase, thus the signatures of the above used classes might change.
See the [Wiki Page](https://github.com/AddictedCS/soundfingerprinting/wiki) for the operational details and information. 

### Extension capabilities
Some of the interfaces which are used by the framework can be easily subsituted according to your needs. In case you dont want to use NAudio as your audio library in order to be independent of running OS version, you can take advantage of Bass.Net integration available through [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) package.
####Available integrations:
* [SoundFingerprinting.Audio.NAudio](https://www.nuget.org/packages/SoundFingerprinting.Audio.NAudio) - NAudio library used for audio processing. Comes bundled as the default audio library.
* [SoundFingerprinting.Audio.Bass](https://www.nuget.org/packages/SoundFingerprinting.Audio.Bass) - Bass.Net audio library integration. Works faster, more accurate resampling, completely independent upon target OS.
* [SoundFingerprinting.SQL](https://www.nuget.org/packages/SoundFingerprinting.SQL) - implements integration with MSSQL storage.
* [SoundFingerprinting.MongoDb](https://www.nuget.org/packages/SoundFingerprinting.MongoDb) - implements integration with MongoDb, still in pre-release phase.

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
* [Bass.Net](http://www.un4seen.com/)
* [NAudio](http://naudio.codeplex.com/)
* [FFTW](http://www.fftw.org/) - used as a default framework for FFT algorithm.
* [Exocortex](http://www.exocortex.org/dsp/) - can be used as a substitution for FFTW (deprecated)
* [Encog](http://www.heatonresearch.com/encog) - used by Neural Hasher (which is still under development, and will be released as a separate component). SoundFingerprinting library does not include it in its release.
* [Ninject](http://www.ninject.org/) - used to take advantage of dependency inversion principle.

### FAQ
- Can I apply this algorithm for speech recognition purposes?
No. The granularity of one fingerprint is roughly ~1.86 seconds, thus any sound recording which is less than that will be disregarded.

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

&copy; Soundfingerprinting, 2010-2014, ciumac.sergiu@gmail.com

