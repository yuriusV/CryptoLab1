/*
 * Created by SharpDevelop.
 * User: yuriusV
 */
using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Ciphers
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Process();
			Console.ReadLine();
		}
		
		public static void Process() {
			string allTask = File.ReadAllText("input.txt");
			Log("Весь текст", allTask);
			// Хмм.. на вид base64
			allTask = AlgoHelpers.DecodeBase64(allTask);
			// Дежавю..
			allTask = AlgoHelpers.DecodeBase64(allTask);
			Log("Расшифрован", allTask);
			// А вот и таски
			string task2Text = File.ReadAllText("task2.txt");
			Log("Таска 2", task2Text);
			// Напишем xor дешифратор
			string task2Decrypted = Algo.DecodeCaesar(task2Text);
			task2Decrypted = task2Decrypted.Replace('=', 'l');
			Log("Расшифрованая 2 таска", task2Decrypted);
			// Well done
			string task3Text = File.ReadAllText("task3.txt");
			Log("Задача 3", task3Text);
			// Шифр Виженера
			byte[] bytesFromHex = AlgoHelpers.HexToByteArray(task3Text);
			string task3Decoded = Algo.DecodeVigenere(bytesFromHex);
			Log("Задача 3 ответ", task3Decoded);
		}
		
		public static void Log(string message) {
			Console.WriteLine(message);
		}
		
		public static void Log(string header, string message) {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(header);
			Console.ResetColor();
			Console.WriteLine(message);
		}
		
	}

	public static class Algo {
		public static string DecodeCaesar(string input) {
			var symbolsCountsToVariants = Enumerable.Range(1, 255)
				.Select(x => (char)x)
				.Select(c => new Tuple<char, int>(c, AlgoHelpers.GetCountLetters(
					AlgoHelpers.DecodeCaesarXor(input, c))));
			int maxCount = symbolsCountsToVariants.Max(x => x.Item2);
			char key = symbolsCountsToVariants.First(x => x.Item2 == maxCount).Item1;
			
			return AlgoHelpers.DecodeCaesarXor(input, key);
		}
		
		public static byte[] DecodeCaesarBySpace(byte[] input) {
			int[] codes = new int[256];
			for (int i = 0; i < codes.Length; i++) {
				codes[i] = 0;
			}
			
			for (int i = 0; i < input.Length; i++) {
				codes[input[i]]++;
			}
			
			int maxIndex = -1;
			int max = int.MinValue;
			for (int i = 0; i < codes.Length; i++) {
				if (codes[i] > max) {
					max = codes[i];
					maxIndex = i;
				}
			}
			
			char maxChar = (char)maxIndex;
			
			return AlgoHelpers.DecodeCaesarXor(input, (byte)maxChar);
		}
		
		public static string DecodeVigenere(byte[] bytes) {
			const double engAlphabetCoefficient = 0.0667;
			
			var coincidenceValues = Enumerable
				.Range(1, bytes.Length / 2)
				.Select(x => new Tuple<int, double>(x, AlgoHelpers.GetIndexOfCoincidence(bytes, x)))
				.ToArray();
			
			double minDistance = double.MaxValue;
			int keyLen = 0;
			foreach(var value in coincidenceValues) {
				if (Math.Abs(value.Item2 - engAlphabetCoefficient) < minDistance) {
					minDistance = Math.Abs(value.Item2 - engAlphabetCoefficient);
					keyLen = value.Item1;
				}
			}
			
			// Узнали длину ключа, будем подбирать все варианты сдвига и оценивать его качество
			var resolves = new List<byte[]>();
			byte[] symCodes = new byte[keyLen];
			for (int i = 0; i < keyLen; i++) {
				byte[] parted = AlgoHelpers.TakeEachNSymbol(bytes, keyLen, i);
				
				byte[] resolved = Algo.DecodeCaesarBySpace(parted);
				resolves.Add(resolved);
			}
			
			return Encoding.UTF8.GetString(AlgoHelpers.GetCombined(resolves));
		}
		
//		public static string DecodeSubstitution(string input) {
//			const int TestLength = 6;
//			
//			input = input.ToLower();
//			var testMapping = new Dictionary<char, char>() {
//				
//			};
//			
//			
//			while (true) { // todo
//				GetWordScore(input);
//			}
//		}
//		
//		public static string ReplaceSpaces(string input) {
//			char charForSpace = input.Select(x => input.Count(c => ));
//		}
		
		private static string MakeReplace(string input, Dictionary<char, char> mapping) {
			foreach (var key in mapping.Keys) {
				input = input.Replace(key, mapping[key]);
			}
			
			return input;
		}
//		
//		private static void MutateMapping(Dictionary<char, char> mapping) {
//			foreach (var key in mapping.Keys) {
//				char beforeValue = mapping[key];
//				char generatedValue;
//				while ((generatedValue = AlgoHelpers.GetRandomChar()) == beforeValue) {
//					//empty	
//				}
//				
//				mapping[key] = generatedValue;
//			}
//		}
		
		private static int GetWordScore(string input, string[] dictionary) {
			//
			//var words
			return dictionary.Any(x => x == input) ? 1 : 0;
		}
		
		
	}
	
	public static class AlgoHelpers {
		
		private static char[] _letters = Enumerable.Range((int)'a', ((int)'z') - ((int)'a'))
			.Concat(Enumerable.Range((int)'A', ((int)'Z') - ((int)'A')))
			.Select(x => (char)x).ToArray();
		
		private static char[] _smallLetters = Enumerable.Range((int)'a', ((int)'z') - ((int)'a'))
			.Select(x => (char)x).ToArray();
		
		private static Random _random = new Random();
//		
//		public static char GetRandomChar() {
//			return (char)('a' + _random.Next(_smallLetters.Count));
//		}
		
		public static string GetCombined(List<string> resolves) {
			var common = new List<char>();
			for(int i = 0; i < resolves[0].Count(); i++) {
				for(int j = 0; j < resolves.Count(); j++) {
					if (resolves[j].Count() > i)
						common.Add(resolves[j][i]);
				}
			}
			
			return new string(common.ToArray());
		}
		
		public static byte[] GetCombined(List<byte[]> resolves) {
			var common = new List<byte>();
			for(int i = 0; i < resolves[0].Length; i++) {
				for(int j = 0; j < resolves.Count; j++) {
					if (resolves[j].Length > i)
						common.Add(resolves[j][i]);
				}
			}
			
			return common.ToArray();
		}
		
		
		public static string DecodeBase64(string input) {
			byte[] data = Convert.FromBase64String(input);
			string decodedString = Encoding.UTF8.GetString(data);
			return decodedString;
		}
		
		public static string DecodeCaesarXor(string input, char key) {
			return string.Join("", input.Select(c => (char)( ((int)c) ^ ((int)key) )));
		}
		
		public static byte[] DecodeCaesarXor(byte[] input, byte key) {
			return input.Select(c => (byte)( ((byte)c) ^ key )).ToArray();
		}
		
		public static byte[] DecodeVigenere(byte[] bytes, byte[] key) {
			var result = new List<byte>();
			for (int i = 0;i < bytes.Length; i++) {
				result.Add((byte)(bytes[i] ^ key[i % key.Length]));
			}
			return result.ToArray();
		}
		
		public static double GetIndexOfCoincidence(byte[] input, int shift) {
			byte[] shiftedString = input.Skip(input.Length - shift).Concat( input.Take(input.Length - shift) ).ToArray();
			double count = 0;
			for (int i = 0; i < input.Length; i++) {
				if (input[i] == shiftedString[i]) {
					count++;
				}
			}
			
			return count / (double)input.Length;
		}
		
		public static byte[] HexToByteArray(string hex) {
    		return Enumerable.Range(0, hex.Length)
				.Where(x => x % 2 == 0)
				.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
				.ToArray();
		}
		
		public static byte[] TakeEachNSymbol(byte[] bytes, int n, int shift) {
			var result = new List<byte>();
			for(int i = shift; i < bytes.Length; i += n) {
				result.Add(bytes[i]);
			}
			return result.ToArray();
		}
		
		public static int GetCountLetters(string input) {
			return input.Count(c => _letters.Contains(c));
		}
		
		public static string BytesToString(byte[] bytes) {
			return Encoding.UTF8.GetString(bytes);
		}
	}
	
	public static class Extensions {
		public static T MaxElement<T>(this IEnumerable<T> collection, Func<T, int> sortFunction) {
			int max = int.MinValue;
			T maxItem = collection.FirstOrDefault();
			foreach (var item in collection) {
				int score = sortFunction(item);
				if (score > max) {
					max = score;
					maxItem = item;
				}
			}
			
			return maxItem;
		}
	}
	
}