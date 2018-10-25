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
		
		public static string DecodeCaesarBySpace(byte[] input) {
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
			
			char maxChar = Encoding.UTF8.GetString(new byte[]{(byte)maxIndex})[0];
			
			string res = AlgoHelpers.DecodeCaesarXor(Encoding.UTF8.GetString(input), maxChar);
			return res;
		}
		
		public static string DecodeVigenere(byte[] bytes, string[] words) {
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
			double percentOfEngToCandidate = 0.75;
			var candidates = new List<List<Tuple<byte, byte[]>>>();
			var resolves = new List<string>();
			byte[] symCodes = new byte[keyLen];
			for (int i = 1; i <= keyLen; i++) {
				var currentLetterCandidates = new List<Tuple<byte, byte[]>>();
				candidates.Add(currentLetterCandidates);
				byte[] parted = AlgoHelpers.TakeEachNSymbol(bytes, i);
				
				//string resolved = Algo.DecodeCaesarBySpace(parted);
				//resolves.Add(resolved);
				for (int symCode = 1; symCode < 255; symCode++) {
					byte[] variant = AlgoHelpers.DecodeCaesarXor(parted, (byte)symCode);
					int count = AlgoHelpers.GetCountLetters(AlgoHelpers.BytesToString(variant));
					if (count > percentOfEngToCandidate * variant.Length) {
						currentLetterCandidates.Add(new Tuple<byte, byte[]>((byte)symCode, variant));
					}
				}
			}
			
			for (int keyIndex = 0; keyIndex < candidates.Count; keyIndex++) {
				
			}
			
			
			return AlgoHelpers.BytesToString( AlgoHelpers.DecodeVigenere(bytes, symCodes));
		}
	}
	
	public static class AlgoHelpers {
		
		private static char[] _letters = Enumerable.Range((int)'a', ((int)'z') - ((int)'a'))
			.Concat(Enumerable.Range((int)'A', ((int)'Z') - ((int)'A')))
			.Select(x => (char)x).ToArray();
		
		public static string GetCombined(List<List<byte>> resolves) {
			var common = new List<byte>();
			for(int i = 0; i < resolves[0].Count; i++) {
				for(int j = 0; j < resolves.Count; j++) {
					if (resolves[j].Count > i)
						common.Add(resolves[j][i]);
				}
			}
			
			return Encoding.UTF8.GetString(common.ToArray());
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
		
		public static byte[] TakeEachNSymbol(byte[] bytes, int n) {;
			var result = new List<byte>();
			for(int i = n; i < bytes.Length; i += n) {
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
	
	
}