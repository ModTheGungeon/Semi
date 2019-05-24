using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Net;
using Logger = ModTheGungeon.Logger;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Semi {
	public static class ModVerification {
		internal const string BASE_VERIFICATION_URL = "https://raw.githubusercontent.com/ModTheGungeon/SemiModVerification/master/";

		internal static Logger Logger = new Logger("ModVerification");

		internal static Comparison<string> StringAlphabeticComparison = (x, y) => string.Compare(x, y);

		public static byte[] GetModHash(string dir_path, ModConfig config) {
			var hash = new List<byte>();

			dir_path = dir_path + Path.DirectorySeparatorChar;
			var files = Directory.GetFiles(dir_path, "*", SearchOption.AllDirectories);
			Array.Sort(files, StringAlphabeticComparison);
			for (int i = 0; i < files.Length; i++) {
				var file = files[i];
				if (file.Contains("\0")) throw new Exception("File name may not contain null bytes");

				if (!file.Contains(dir_path)) throw new Exception($"File '{file}' is outside of the specified mod folder");

				var rel_path = file.Substring(dir_path.Length).Replace("\\", "/");

				hash.AddRange(Encoding.UTF8.GetBytes(rel_path));
				hash.Add(0);
				using (var sha256 = SHA256.Create()) {
					hash.AddRange(sha256.ComputeHash(File.ReadAllBytes(file)));
				}
			}

			hash.Add(0);

			using (var sha256 = SHA256.Create()) {
				return sha256.ComputeHash(hash.ToArray());
			}
		}

		public static bool ValidateHash(byte[] valid, byte[] computed) {
			if (valid.Length != computed.Length) return false;
			for (int i = 0; i < valid.Length; i++) {
				if (valid[i] != computed[i]) return false;
			}
			return true;
		}

		public static bool ValidateHashOnline(string mod_id, byte[] hash) {
			var url = $"{BASE_VERIFICATION_URL}{mod_id}";

			Logger.Debug($"Validating from URL: {url}");
			byte[] online_hash = null;
			using (var client = new WebClient()) {
				ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
				client.Encoding = Encoding.UTF8;
				try {
					online_hash = client.DownloadData(url);
				} catch (WebException e) {
					Logger.Error($"[{e.GetType().Name}] {e.Message}");
					Logger.ErrorPretty(e.StackTrace);
					return false;
				}
			}

			return ValidateHash(online_hash, hash);
		}

		//https://stackoverflow.com/a/33391290
		//needed for https
		internal static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			bool isOk = true;
			// If there are errors in the certificate chain,
			// look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None) {
				for (int i = 0; i < chain.ChainStatus.Length; i++) {
					if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
						continue;
					}
					chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
					chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
					chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
					chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
					bool chainIsValid = chain.Build((X509Certificate2)certificate);
					if (!chainIsValid) {
						isOk = false;
						break;
					}
				}
			}
			return isOk;
		}
	}
}
