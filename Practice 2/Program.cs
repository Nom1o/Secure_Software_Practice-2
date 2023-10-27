using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCracker
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Введите хэш-значение:");
      string hashValue = Console.ReadLine();

      Console.WriteLine("Введите количество потоков:");
      int numThreads = int.Parse(Console.ReadLine());

      Console.WriteLine("Старт подбора паролей...");

      DateTime startTime = DateTime.Now;

      List<string> passwords = CrackPassword(hashValue, numThreads);

      DateTime endTime = DateTime.Now;

      Console.WriteLine("Времени затрачено: " + (endTime - startTime).TotalSeconds + " секунд");

      Console.WriteLine("Возможные пароли:");
      foreach (string password in passwords)
      {
        Console.WriteLine(password);
      }

      Console.ReadLine();
    }

    static List<string> CrackPassword(string hashValue, int numThreads)
    {
      List<string> passwords = new List<string>();

      char[] alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

      int passwordLength = 5;

      int numPasswords = (int)Math.Pow(alphabet.Length, passwordLength);

      int passwordsPerThread = numPasswords / numThreads;

      Task<List<string>>[] tasks = new Task<List<string>>[numThreads];

      for (int i = 0; i < numThreads; i++)
      {
        int startIndex = i * passwordsPerThread;
        int endIndex = (i == numThreads - 1) ? numPasswords : (i + 1) * passwordsPerThread;

        tasks[i] = Task.Run(() => FindPasswordsInRange(startIndex, endIndex, alphabet, passwordLength, hashValue));
      }

      Task.WaitAll(tasks);

      foreach (Task<List<string>> task in tasks)
      {
        passwords.AddRange(task.Result);
      }

      return passwords;
    }

    static List<string> FindPasswordsInRange(int startIndex, int endIndex, char[] alphabet, int passwordLength, string hashValue)
    {
      List<string> passwords = new List<string>();

      for (int i = startIndex; i < endIndex; i++)
      {
        string password = GetPasswordFromIndex(i, alphabet, passwordLength);

        if (GetHash(password) == hashValue)
        {
          passwords.Add(password);
        }
      }

      return passwords;
    }

    static string GetPasswordFromIndex(int index, char[] alphabet, int passwordLength)
    {
      char[] password = new char[passwordLength];

      for (int i = 0; i < passwordLength; i++)
      {
        password[i] = alphabet[index % alphabet.Length];
        index /= alphabet.Length;
      }

      return new string(password);
    }

    static string GetHash(string input)
    {
      using (SHA256 sha256Hash = SHA256.Create())
      {
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < bytes.Length; i++)
        {          
          builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString();
      }
    }
  }
}