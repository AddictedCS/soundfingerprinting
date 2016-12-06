namespace SoundFingerprinting.Utils
{
    internal class TestScenariousValidationResult
    {
        public bool IsValid { get; set; }

        public string Message { get; set; }

        public static TestScenariousValidationResult InvalidResult(string message)
        {
            return new TestScenariousValidationResult { IsValid = false, Message = message };
        }

        public static TestScenariousValidationResult ValidResult()
        {
            return new TestScenariousValidationResult { IsValid = true };
        }
    }
}