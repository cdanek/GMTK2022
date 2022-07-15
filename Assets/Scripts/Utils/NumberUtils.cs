using System;

namespace KaimiraGames.GameJam
{
    /// <summary>
    /// Some number utils I've needed from time to time.
    /// </summary>
    public static class NumberUtils
    {
        private static readonly Random _internalRand;

        static NumberUtils()
        {
            _internalRand = new Random();
        }

        /// <summary>
        /// Returns a random value sampled from the standard Gaussian distribution, i.e., with mean of 0 and standard deviation of 1.
        ///
        /// </summary>
        /// <returns>A random sample.</returns>
        public static double NormalDistribution() => NormalDistribution(1, 0);

        /// <summary>
        /// Extension method on double types to add variance to a number for randomness. 
        /// Typical is 0.1 (plus/minus 10%).
        /// </summary>
        /// <param name="in"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static double AddVariance(this double @in, double amount = 0.1d)
        {
            if (amount < 0) throw new ArgumentException("Can't add negative variance to a number.");
            double randomness = NextDouble(amount * 2) - amount; // -0.1 to 0.1
            return @in + (@in * randomness);
        }

        public static float AddVariance(this float @in, double amount = 0.1d) => (float)AddVariance((double)@in, amount);

        public static int AddVariance(this int @in, double howMuch = 0.1d) => (int)AddVariance((double)@in, howMuch);

        /// <summary>
        /// Returns a random value sampled from the standard Gaussian distribution, with a specified mean and standard deviation.
        ///
        /// Implementation from https://stackoverflow.com/questions/218060/random-gaussian-variables
        ///
        /// For reference, remember the 68-95-99.7 rule (1/2/3 standard deviations). Only 3 in 1000 events will be 3 standard deviations
        /// above/below the mean.
        ///
        /// </summary>
        /// <param name="stddev"></param>
        /// <param name="mean"></param>
        /// <returns></returns>
        public static double NormalDistribution(float stddev, float mean)
        {
            double u1 = 1.0 - _internalRand.NextDouble();
            double u2 = 1.0 - _internalRand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = mean + stddev + randStdNormal;
            return randNormal;
        }

        /// <summary>
        /// Returns a random int from 0-maxValue exclusive. 
        /// 
        /// I'm bypassing using Math.Random or Unity.Mathf.Random here in case I decide to implement a more performant or secure solution. 
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int maxValueExclusive) => _internalRand.Next(maxValueExclusive);
        public static int Next(int minInclusive, int maxExclusive) => Next(maxExclusive - minInclusive) + minInclusive;

        public static bool NextBool() => _internalRand.Next(2) == 0;

        /// <summary>
        /// Random number between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        /// <returns></returns>
        public static double NextDouble() => _internalRand.NextDouble();
        public static double NextDouble(double maxDouble) => NextDouble() * maxDouble;
        public static double NextDouble(double minInclusive, double maxExclusive) => NextDouble(maxExclusive - minInclusive) + minInclusive;

        public static float NextFloat() => (float)_internalRand.NextDouble();
        public static float NextFloat(float maxFloat) => NextFloat() * maxFloat;
        public static float NextFloat(float minInclusive, float maxExclusive) => NextFloat(maxExclusive - minInclusive) + minInclusive;

    }
}
