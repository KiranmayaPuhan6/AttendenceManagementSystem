namespace AMS.Services.Utility.HelperMethods
{
    public static class RandomNumberGenerator
    { 
        private static readonly Random _random = new Random();    
        public static int Generate(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
