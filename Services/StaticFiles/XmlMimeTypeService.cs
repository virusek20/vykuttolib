using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using vykuttolib.Configuration;

namespace vykuttolib.Services.StaticFiles
{
	public class XmlMimeTypeService : IMimeTypeService
	{
		private readonly MimeTypeConfiguration _configuration = new MimeTypeConfiguration();
		private readonly AllowedMimeTypes _allowedTypes;

		public XmlMimeTypeService(IConfiguration config)
		{
			config.GetSection("AllowedMimeTypes").Bind(_configuration);

			var serializer = new XmlSerializer(typeof(AllowedMimeTypes));
			using var xmlDoc = File.OpenRead(_configuration.XmlFilePath);

			_allowedTypes = serializer.Deserialize(xmlDoc) as AllowedMimeTypes;
		}

		public bool CheckSignature(string mimeType, Stream data)
		{
			var type = _allowedTypes.MimeType.FirstOrDefault(t => t.Name.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
			if (type == null) return false; // Not allowed type

			var signatures = type.BinarySignatures.ToList();
			var offset = type.Offset;

			var longestSignature = signatures.Select(s => s.Length + offset).OrderByDescending(l => l).FirstOrDefault();
			if (longestSignature == 0) return false; // Either empty or no signatures defined

			var checkedData = new byte[longestSignature].AsSpan();
			data.Read(checkedData);

			foreach (var signature in signatures)
			{
				var signatureSpan = signature.AsSpan();
				var matchingData = checkedData.Slice(offset); // Ignore first offset bytes

				if (signatureSpan.SequenceEqual(matchingData)) return true;
			}

			return false;
		}

		public string DetermineExtension(string mimeType)
		{
			var type = _allowedTypes.MimeType.FirstOrDefault(t => t.Name.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
			return type?.Extension;
		}
	}
}
