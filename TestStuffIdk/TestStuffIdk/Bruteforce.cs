using System;
using System.Collections;
using System.Text;
namespace TestStuffIdk
{
    /// <summary>
    /// Brute force class to generate all possible combinations between a max and minimum length
    /// </summary>
    public class Bruteforce
    {
        #region Attributes
        private StringBuilder sb = new StringBuilder();
        public string validChars;
        private ulong len;
        private int _max;
        public int max { get { return _max; } set { _max = value; } }
        private int _min;
        public int min { get { return _min; } set { _min = value; } }
        #endregion
        public IEnumerator GetEnumerator()
        {
            // Set the length to the length of the valid chars and convert to ulong
            len = (ulong)validChars.Length; 
            for (double x = min; x <= max; x++)
            {
                // Calculate the total
                ulong total = (ulong)Math.Pow((double)validChars.Length, (double)x);
                ulong counter = 0;
                // Loop until counter is less than total
                while (counter < total)
                {                    
                    string a = factor(counter, x - 1);
                    yield return a;
                    counter++;
                }
            }
        }
        /// <summary>
        /// Calculates combiantions of a given length
        /// </summary>
        /// <param name="length"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        private string factor(ulong length, double power)
        {
            sb.Length = 0;
            while (power >= 0)
            {
                sb = sb.Append(this.validChars[(int)(length % len)]);
                length /= len;
                power--;
            }
            return sb.ToString();
        }
    }
}