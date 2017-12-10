namespace SoundFingerprinting.Utils
{
    using System;
    using System.IO;
    using System.Linq;

    internal class TestRunnerScenarioValidator
    {
        private readonly ITestRunnerUtils utils;
        private readonly TestRunnerConfig testRunnerConfig = new TestRunnerConfig();

        public TestRunnerScenarioValidator() : this(new TestRunnerUtils())
        {
        }

        internal TestRunnerScenarioValidator(ITestRunnerUtils utils)
        {
            this.utils = utils;
        }

        public TestScenariousValidationResult ValidateScenarious(string[] scenarious)
        {
            foreach (var scenario in scenarious)
            {
                string[] parameters = scenario.Split(',');
                switch (parameters[0])
                {
                    case "Insert":
                        var insertAction = ValidateInsertAction(parameters);
                        if (!insertAction.IsValid)
                        {
                            return insertAction;
                        }

                        break;
                    case "Run":
                        var runAction = ValidateRunAction(parameters);
                        if (!runAction.IsValid)
                        {
                            return runAction;
                        }

                        break;
                    default:
                        return
                            TestScenariousValidationResult.InvalidResult(
                                string.Format("Bad action '{0}'. Should be either 'Run' or 'Insert'", parameters[0]));
                }
            }

            return TestScenariousValidationResult.ValidResult();
        }

        private TestScenariousValidationResult ValidateInsertAction(string[] parameters)
        {
            string folderWithSongs = parameters[1];
            if (!Directory.Exists(folderWithSongs))
            {
                return
                    TestScenariousValidationResult.InvalidResult(
                        string.Format("Path to songs folder '{0}' is not valid", folderWithSongs));
            }

            var allSongsToInsert = utils.ListFiles(folderWithSongs, testRunnerConfig.AudioFileFilters);
            if (!allSongsToInsert.Any())
            {
                return
                    TestScenariousValidationResult.InvalidResult(
                        string.Format("Path to songs folder '{0}' contains no items for insertion!", folderWithSongs));
            }

            try
            {
                utils.ToStride(parameters[2], parameters[3], parameters[4]);
            }
            catch (Exception e)
            {
                return TestScenariousValidationResult.InvalidResult(e.Message);
            }

            return TestScenariousValidationResult.ValidResult();
        }

        private TestScenariousValidationResult ValidateRunAction(string[] parameters)
        {
            string folderWithPositives = parameters[1];
            if (!Directory.Exists(folderWithPositives))
            {
                return
                    TestScenariousValidationResult.InvalidResult(
                        string.Format("Path to folder with positives '{0}' is not valid", folderWithPositives));
            }

            string folderWithNegatives = parameters[2];
            if (!Directory.Exists(folderWithNegatives))
            {
                return
                    TestScenariousValidationResult.InvalidResult(
                        string.Format("Path to folder with negatives '{0}' is not valid", folderWithNegatives));
            }

            try
            {
                utils.ToStride(parameters[3], parameters[4], parameters[5]);
                int secondsToProcess = int.Parse(parameters[6]);
                utils.ParseInts(parameters[7], testRunnerConfig.StartAtsSeparator);
            }
            catch (Exception e)
            {
                return TestScenariousValidationResult.InvalidResult(e.Message);
            }

            var positives = utils.ListFiles(folderWithPositives, testRunnerConfig.AudioFileFilters);
            if (!positives.Any())
            {
                return
                    TestScenariousValidationResult.InvalidResult(
                        string.Format("Folder with positives is empty: {0}", folderWithPositives));
            }

            return TestScenariousValidationResult.ValidResult();
        }
    }
}
