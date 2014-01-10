## Sound Fingerprinting

Soundfingerprinting is a C# framework designed for developers and researchers in the fields of audio processing, data mining, digital signal processing.  It implements an efficient algorithm of signal processing which will allow one to have a competent system of audio fingerprinting and signal recognition.

## Documentation
See the [Wiki page](https://github.com/AddictedCS/soundfingerprinting/wiki) for the operational details and information 

Following is a code sample that shows how to generate sub-fingerprints from an audio file. The sub-fingerprints will be stored in a backend and used later by the algorithm to recognize unknown snippets of audio. The interfaces for fingerprinting and querying the stream have been implemented as [FluentInterface](http://martinfowler.com/bliki/FluentInterface.html) with Builder and Command patterns in mind.
```csharp
private readonly IModelService modelService = new ModelService();
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
                                .Hash()
                                .Result;
								
    // store sub-fingerprints and hash representation in the underlying database 
    modelService.InsertHashDataForTrack(hashDatas, trackReference);
}
```
The default underlying database is MSSQL, those connection management is handled by <code>ModelService</code> class. NoSQL data storage will be implemented in the upcomming releases. The MSSQL database initialization script can be find [here](src/Scripts/DBScript.sql). Do not forget to change connection string <code>FingerprintConnectionString</code> in your app.config file.

Once you've inserted the fingerprints into the database, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, url, microphone, radio tuner, etc. It's up to your application, where you get the samples from.
```csharp
private readonly IQueryCommandBuilder queryCommandBuilder = new QueryCommandBuilder();

public TrackData GetBestMatchForSong(string queryAudioFile)
{
    int secondsToAnalyze = 10; // number of seconds to analyze from query file
    int startAtSecond = 0; // start at the begining
	
    // query the underlying database for similary audio sub-fingerprints
    var queryResult = queryCommandBuilder.BuildQueryCommand()
                                         .From(queryAudioFile, secondsToAnalyze, startAtSecond)
                                         .WithDefaultConfigs()
                                         .Query()
                                         .Result;
    if(queryResult.IsSuccessful)
    {
        return queryResult.BestMatch; // successful match has been found
    }
	
    return null; // no match has been found
}
```

The code is still in pre-release phase, thus the signatures of the above used classes might change.

## Binaries
    git clone git@github.com:AddictedCS/soundfingerprinting.git
    
In order to build latest version of the <code>SoundFingerprinting</code> assembly run the following command from repository root

    .\build.cmd
### Get it on NuGet

    Install-Package SoundFingerprinting -Pre

## Demo
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting)
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/Soundfingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

## Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

## Licence
The framework is provided under [GPLv3](http://www.gnu.org/licenses/gpl.html) licence agreement.

The framework implements the algorithm from [Content Fingerprinting Using Wavelets](http://www.nhchau.com/files/cvmp_BalujaCovell.A4color.pdf) paper.

&copy; Soundfingerprinting, 2010-2014, ciumac.sergiu@gmail.com

