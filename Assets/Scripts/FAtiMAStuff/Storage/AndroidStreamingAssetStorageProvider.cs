#if UNITY_ANDROID

using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace FAtiMAScripts
{
	public sealed class AndroidStreamingAssetStorageProvider : IStorageProvider
	{
		private static Dictionary<string, byte[]> m_retrievedFiles = new Dictionary<string, byte[]>();

		[System.Obsolete]
		public object LoadFile(string absoluteFilePath, FileMode mode, FileAccess access)
		{
			using (var file = File.OpenRead(Application.dataPath))
			{

				var zip = new ZipFile(file);
				try
				{
					var entryIndex = zip.FindEntry("assets" + absoluteFilePath.Replace('\\', '/'), false);
					if (entryIndex < 0)
						throw new FileNotFoundException();

					var stream = zip.GetInputStream(entryIndex);
					var m = new MemoryStream();
					stream.CopyTo(m);
					m.Position = 0;
					return m;
				}
				finally {
					zip.Close();
				}
			}
		}

		[System.Obsolete]
		public bool FileExists(string absoluteFilePath)
		{
			using (var file = File.OpenRead(Application.dataPath))
			{
				var zip = new ZipFile(file);
				try
				{
					var entryIndex = zip.FindEntry("assets" + absoluteFilePath.Replace('\\', '/'), false);
					return entryIndex >= 0;
				}
				finally {
					zip.Close();
				}
			}
		}

		Stream IStorageProvider.LoadFile(string absoluteFilePath, FileMode mode, FileAccess access)
		{
			throw new System.NotImplementedException();
		}
	}
}

#endif