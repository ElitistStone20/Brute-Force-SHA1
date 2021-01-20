using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace TestStuffIdk
{
    public class Program
    {                   
        private static bool Found = false;
        /// <summary>
        /// Main controller function, gets intputs and generates threads to work on brute forcing
        /// </summary>
        public static void Main()
        {
            // Get inputs from user
            Console.WriteLine("Is this a BCH password? y/n");
            string reply = Console.ReadLine().ToLower();
            Console.WriteLine("Enter a password to brute force");
            string password = Console.ReadLine();
            Console.WriteLine($"Hashed password to find: {password}");
            Console.WriteLine("Brute Forcing. This may take a while. Go make a cup of tea.........");
            // Get the start time 
            DateTime StartTime = DateTime.Now;         
            // CHeck if BCH brute force or not
            if (reply == "y")                 
                BruteForceBCH(password);          
            else
            {    
                // Create list of threads to execute each thread generates combinations of a specific length
                List<Task> taskList = new List<Task>
                {
                    Task.Factory.StartNew(() => BruteForceSHA(password, 1)),
                    Task.Factory.StartNew(() => BruteForceSHA(password, 2)),
                    Task.Factory.StartNew(() => BruteForceSHA(password, 3)),
                    Task.Factory.StartNew(() => BruteForceSHA(password, 4)),
                    Task.Factory.StartNew(() => BruteForceSHA(password, 5)),
                    Task.Factory.StartNew(() => BruteForceSHA(password, 6))
                };
                // Wait until all threads have completed
                Task.WaitAll(taskList.ToArray());
            }
            // Get current time and work out time difference between start time and current time
            TimeSpan ts = DateTime.Now - StartTime;
            // Outputs                             
            // Output time taken
            if (Found == false)
                Console.WriteLine("Password has not been found");
            Console.WriteLine($"Time Taken: {Math.Round(ts.TotalHours)} Hours\n{Math.Round(ts.TotalMinutes)} Mins\n{ts.TotalSeconds} Sec");
            Console.ResetColor();
        }     
        private static void BruteForceSHA(string hashedPassword, int Len)
        {
            Console.WriteLine($"Starting generating strings of length {Len}");
            // Configure attributes of instance of brute force 
            Bruteforce brute = new Bruteforce
            {
                min = Len,
                max = Len,
                validChars = "abcdefghijklmnopqrstuvwxyz0123456789"
            };
            // Loop through each result returned from Bruteforce
            foreach (string result in brute)
            {
                // Hash the password using SHA1
                Console.WriteLine(result);
                string hashedResult = TextToHash(result);
                // Check if the hashed result is identical to the hashed password input
                if (hashedResult == hashedPassword)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Password found! \nResult => {result}\nHashed password => {hashedResult}");                    
                    Found = true;
                    return;
                }
                else if (Found == true)
                    return;
            }
            // If no combinations match then output that the thead is finished
            Console.WriteLine($"Finished generating all combinations of length: {Len}");
        }
        private static void BruteForceBCH(string hashedPassword)
        {
            // Configure bruteforce attributes
            Bruteforce brute = new Bruteforce
            {
                min = 6,
                max = 6,
                validChars = "0123456789"
            };
            // Loop through each result returned from brute force
            foreach (string result in brute)
            {
                // Encode last 4 parity bits
                string encodeDigits = EncodeDigits(result);
                // If encode digits doesn't return null
                if (encodeDigits != null)
                {
                    // Check if code generated is a valid BCH code
                    if (isBCHCode(encodeDigits))
                    {         
                        // hash the BCH code and compare against hashed input
                        string hashedResult = TextToHash(encodeDigits);
                        if (hashedResult == hashedPassword)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Password found! \nResult => {encodeDigits}\nHashed password => {hashedResult}");
                            Found = true;
                            return;
                        }
                        else if (Found == true)
                            return;
                    }
                }
            }
            string EncodeDigits(string digits)
            {
                if (digits.Length == 6)
                {
                    int d7 = (4 * (digits[0] - 48) + 10 * (digits[1] - 48) + 9 * (digits[2] - 48) + 2 * (digits[3] - 48) + (digits[4] - 48) + 7 * (digits[5] - 48)) % 11;
                    int d8 = (7 * (digits[0] - 48) + 8 * (digits[1] - 48) + 7 * (digits[2] - 48) + (digits[3] - 48) + 9 * (digits[4] - 48) + 6 * (digits[5] - 48)) % 11;
                    int d9 = (9 * (digits[0] - 48) + (digits[1] - 48) + 7 * (digits[2] - 48) + 8 * (digits[3] - 48) + 7 * (digits[4] - 48) + 7 * (digits[5] - 48)) % 11;
                    int d10 = ((digits[0] - 48) + 2 * (digits[1] - 48) + 9 * (digits[2] - 48) + 10 * (digits[3] - 48) + 4 * (digits[4] - 48) + (digits[5] - 48)) % 11;
                    if (d7 < 10 && d8 < 10 && d9 < 10 && d10 < 10)
                        return $"{digits}{d7}{d8}{d9}{d10}";                 
                }
                return null;
            }
        }
        
        /// <summary>
        /// Hashes the string passed as parameter using SHA1
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string TextToHash(string text)
        {
            StringBuilder hash = new StringBuilder();
            byte[] bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
            foreach (byte b in bytes)
                hash.Append(b.ToString("x2"));
            return hash.ToString();
        }     
        /// <summary>
        /// Checks if the BCH code passed as parameter is a valid BCH code
        /// </summary>
        /// <param name="BCH"></param>
        /// <returns></returns>
        private static bool isBCHCode(string BCH)
        {
            if (BCH.Length == 10 && !string.IsNullOrWhiteSpace(BCH))
            {
                int[] syndromes = GenerateSyndromes();
                if (syndromes[0] == 0 && syndromes[1] == 0 && syndromes[2] == 0 && syndromes[3] == 0)                
                    return true;               
            }
            return false;

            int[] GenerateSyndromes()
            {
                int[] syndromes = new int[4];
                // Calculating first 2 syndromes
                for (int i = 0; i < BCH.Length; i++)
                {
                    syndromes[0] += (BCH[i] - 48);
                    syndromes[1] += (i + 1) * (BCH[i] - 48);
                }
                //Calculating syndrome 3 and 4 since parity digits have no patern
                syndromes[2] = ((BCH[0] - 48) + 4 * (BCH[1] - 48) + 9 * (BCH[2] - 48) + 5 * (BCH[3] - 48) + 3 * (BCH[4] - 48) + 3 * (BCH[5] - 48) + 5 * (BCH[6] - 48) + 9 * (BCH[7] - 48) + 4 * (BCH[8] - 48) + (BCH[9] - 48));
                syndromes[3] = ((BCH[0] - 48) + 8 * (BCH[1] - 48) + 5 * (BCH[2] - 48) + 9 * (BCH[3] - 48) + 4 * (BCH[4] - 48) + 7 * (BCH[5] - 48) + 2 * (BCH[6] - 48) + 6 * (BCH[7] - 48) + 3 * (BCH[8] - 48) + 10 * (BCH[9] - 48));
                // Mod 11 to each syndrome
                for (int j = 0; j < syndromes.Length; j++)
                    syndromes[j] %= 11;
                return syndromes;
            }
        }
    }
}