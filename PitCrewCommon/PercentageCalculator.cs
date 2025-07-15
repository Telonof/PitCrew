namespace PitCrewCommon
{
    public class PercentageCalculator
    {
        private static int TotalPercentage;

        private static int CurrentPercentage;

        public static event Action<int> CurrentChange;
        public static event Action<int> TotalChange;

        public static void IncrementProgress(int amount = 1)
        {
            CurrentPercentage += amount;
            CurrentChange.Invoke(CurrentPercentage);
        }

        public static void Reset()
        {
            TotalPercentage = 0;
            CurrentPercentage = 0;
        }

        public static void IncrementTotal(int amount = 1)
        {
            TotalPercentage += amount;
            TotalChange.Invoke(TotalPercentage);
        }
    }
}
