Soundfingerprinting

Copyright © Soundfingerprinting, 2010-2011
ciumac.sergiu@gmail.com

Soundfingerprinting is a C# framework designed for developers and researchers in the fields of audio processing, data mining, digital signal processing. The aim is to build a publicly available framework for audio fingerprinting and recognition.

Please, contact project owners for any offers, remarks, ideas, etc.

The framework is provided under CPOL v.1.02 licence agreement. 

Following are described the steps in the project setup for those who want a deeper analysis:

1) Install Microsoft SQL Server 2008 (any other .NET RDBMS system is supported with the proper connector).
2) Run the installation script, that will construct the database where the fingerprints/tracks will be stored
The installation script is located under <Root Path>\src\Scripts\DBScript. Please note that the name of the database will be 'FingerprintsDb'.
3) Once installed, you can run the associated tools for sound processing.
3.1) Before running the recognition tests, you need of course to populate the database with Tracks/Fingerprints that later will be used for recognition purposes.
3.1.1) Run SoundTools application.
3.1.2) Click on 'Fill Database' button
3.1.3) Select a 'Connection String' to your database (the connection strings are read from the 'app.config' file, so if you want to use a different connection string from the default one, please add it to your configuration file first).
3.1.4) Next you need to select songs that will be processed and inserted in the database. In this case you have 2 options: either select a specific song, or select a root folder with containing music files that will be processed. Double click on text box, select folder (if folder (or child folder) contains music files, 'Total Songs' field will display its number).
3.1.5) Select an algorithm for fingerprinting (currently only LSH+MinHash is supported), thus specify the number of hashtables and hash keys (E.g. 25/4)
3.1.6) Select stride size between 2 consecutive fingerprints (the stride is specified in samples, thus a 5115 stride for 5512Hz sample will result in 928ms stride).
3.1.7) Select stride type:
Static - a specific equal on each iteration stride between fingerprints
	   --------------   ---------------   ---------------   ------------- //static
Random - random stride (range equaling [0; selecte stride])
	   --------------       --------------- ---------------   ------------- //random
Incremental (static random) - overlapping stride, meaning that a 1024 samples will be added to the beginning of the song and not to the end of the last fingerprint. This stride will generate a big amount of fingerprints if equaling less than 5512.
	   --------------
		   --------------
				 --------------
					  --------------
3.1.8) Select Top Wavelets (number of top wavelets that will be used in fingerprint creation).
3.1.9) Click 'Start' and wait for the fingerprinting to end.
4) Once fingerprinting ends, you can check how well recognition works.
4.1) Run Main application ('SoundTools')
4.1.1) Click on 'Query Fingerprint'
4.1.2) Select the connection string to your database with containing fingerprints.
4.1.3) Select # of seconds to analyze from each music file presented to the algorithm.
4.1.4) Select the starting point of the song from where fingerprinting will begin (random start point can be considered any start point except '0')
4.1.5) Select Max/Min Stride and Stride type.
4.1.6) Select Threshold (# of table votes for the fingerprint to be considered a potential candidate E.g. For 25 tables, 5 threshold votes is considered a good choice).
4.1.7) Select number of Keys/Hashtables/TopWavelets (Please set them equal to the same value as in case of creating the fingerprint).
4.1.8) Select files to be analyzed.
4.1.9) Click start.
5) Analyzing the results:
Finally, you'll see the results in the datagrid with the associated statistics (Hamming Distance average between query and final fingerprint, Minimum hamming, number of table votes, etc). Using this data will help you in processing the results and choosing better configuration of the system. Actually, tuning the data, might be considered the most difficult procedure within the algorithm.