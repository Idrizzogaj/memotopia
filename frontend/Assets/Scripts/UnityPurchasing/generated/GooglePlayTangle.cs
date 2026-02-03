#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("0s/FRK/FwrfiNUW6AW+h6seoj+93vmk9ywmQjS6mbTDWmbBOX56vL7TQZH+jso5K2pzQ26LvpiFYMr2LjpACUfvBYqpQWaw0jw8cxjXYNklM/n1eTHF6dVb6NPqLcX19fXl8f7YahKslTQtArZdDO3j9nrqVdakqWchAakZYfHoj1uWTfVQmL8AAH5EQiqxKtH3MMxFrYjGApS2uCy1Jp/59c3xM/n12fv59fXzfUuxJ9m6GoAk/34hYQmDz4wUN9wVSr2qpYeNRjOClUHiKOE7xAKK4fkrmC/Uj19fgvR47rI57bIEfpzPEamES5xov7T33fbK9OcekUQIXej23dhL357x2nh9xZ0s9xrx2b+4SZN+hplUZFdTrqyD0F4IsPX5/fXx9");
        private static int[] order = new int[] { 9,11,4,3,9,6,13,7,11,12,10,11,12,13,14 };
        private static int key = 124;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
