namespace Assets.Scripts.MachineLearning
{
    public static class MLConstants
    {
        public static string VersionNumberPlacement => "{VERSION}";

        public static string RawDataFilePath => @"D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\Raw Data";
        public static string RawDataArchiveFilePath => @"D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\Raw Data\Archive";

        public static string NormalizedDataFilePath => $@"D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\V{VersionNumberPlacement}\Normalized Data";
        public static string NormalizedArchivedFilePath => $@"D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\V{VersionNumberPlacement}\Normalized Data\Archive";

        public static string LearnedDataFilePath => $@"D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\V{VersionNumberPlacement}\Learned Data";
        public static string LearnedArchivedFilePath => $@"D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\V{VersionNumberPlacement}\Learned Data\Archive";
    }
}
