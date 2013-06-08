## Sound Fingerprinting

Soundfingerprinting is a C# framework designed for developers and researchers in the fields of audio processing, data mining, digital signal processing.  It implements an efficient algorithm of signal processing which will allow one to have a competent system of audio fingerprinting and signal recognition.

### Documentation
See the [Wiki page](https://github.com/AddictedCS/soundfingerprinting/wiki) with the operational details and information 

Following is a code sample of how you would generate from an audio file sound fingerprints, that can be stored and used later for recognition purposes.

```csharp
public List<byte[]> CreateFingerprintSignaturesFromFile(string pathToAudioFile)
{
    FingerprintUnitBuilder fingerprintBuilder = new FingerprintUnitBuilder();
    return fingerprintBuilder.BuildFingerprints()
                             .On(pathToAudioFile)
                             .With<DefaultFingerprintingConfiguration>()
                             .RunAlgorithmWithHashing()
                             .Result;
}
```
After generating the fingerprint signatures you might want to store them for later retrieval. Below is shown a code snippet for saving them to the default underlying storage, using <code>ModelService</code> class. Default storage is an MSSQL database those initialization script can be find [here](src/Scripts/DBScript.sql).
```csharp
public void StoreFingeprintSignaturesForTrack(List<byte[]> fingerprintSignatures, Track track)
{
    ModelService modelService = new ModelService();
    List<SubFingerprint> fingerprintsToStore = new List<SubFingerprint>();
    foreach (var fingerprint in fingerprintSignatures)
    {
        fingerprintsToStore.Add(new SubFingerprint(fingerprint, track.Id));
    }

    modelService.InsertSubFingerprint(fingerprintsToStore);
}
```
Once you've inserted the fingerprints into the database, later you might want to query the storage in order to recognize the song those samples you have. The origin of query samples may vary: file, url, microphone, radio tuner etc. It's up to your application, where you get the samples from.
```csharp
public Track GetBestMatchForSong(String queryAudioFile)
{
    FingerprintQueryBuilder fingerprintQueryBuilder = new FingerprintQueryBuilder();
    return fingerprintQueryBuilder.BuildQuery()
                           .From(queryAudioFile)
                           .With<DefaultFingerprintingConfiguration, DefaultQueryConfiguration>()
                           .Query()
                           .Result
                           .BestMatch;
}
```

The code is still in pre-release phase, thus the signatures of the above used classes might change.

### Demo
My description of the algorithm alogside with the demo project can be found on [CodeProject](http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting)
The demo project is a Audio File Duplicates Detector. Its latest source code can be found [here](src/Soundfingerprinting.DuplicatesDetector). Its a WPF MVVM project that uses the algorithm to detect what files are perceptually very similar.

### Contribute
If you want to contribute you are welcome to open issues or discuss on [issues](https://github.com/AddictedCS/soundfingerprinting/issues) page. Feel free to contact me for any remarks, ideas, bug reports etc. 

### Licence
The framework is provided under [GPLv3](http://www.gnu.org/licenses/gpl.html) licence agreement.

The framework implements the algorithm from [Content Fingerprinting Using Wavelets](http://www.nhchau.com/files/cvmp_BalujaCovell.A4color.pdf) paper.

&copy; Soundfingerprinting, 2010-2013, ciumac.sergiu@gmail.com

