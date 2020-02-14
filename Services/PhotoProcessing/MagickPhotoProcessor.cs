using System;
using System.IO;
using ImageMagick;
using Microsoft.AspNetCore.Http;

namespace vykuttolib.Services.PhotoProcessing
{
	public class MagickPhotoProcessor : IPhotoProcessor
	{
		public byte[] ProcessUploadedImage(IFormFile photo, bool cropToSquare)
		{
			try
			{
				using var processedStream = new MemoryStream();
				using var uploadedStream = photo.OpenReadStream();
				using MagickImage image = new MagickImage(uploadedStream);

				if (cropToSquare)
				{
					var shroterSide = Math.Min(image.Width, image.Height);
					image.Crop(shroterSide, shroterSide, Gravity.Center);
					image.RePage();
				}

				image.Write(processedStream, MagickFormat.Jpg);

				processedStream.Seek(0, SeekOrigin.Begin);

				using var reader = new BinaryReader(processedStream);
				return reader.ReadBytes((int)processedStream.Length);
			}
			catch (MagickMissingDelegateErrorException e)
			{
				throw new InvalidDataException("Uploaded image could not be decoded by ImageMagick", e);
			}
		}

		public (double, double)? GetExifGpsCoordinates(IFormFile photo)
		{
			try
			{
				using var processedStream = new MemoryStream();
				using var uploadedStream = photo.OpenReadStream();
				using MagickImage image = new MagickImage(uploadedStream);

				var exif = image.GetExifProfile();
				if (exif == null) return null;

				Rational[] exifLatitude = exif.GetValue(ExifTag.GPSLatitude)?.Value as Rational[];
				string latitudeRef = exif.GetValue(ExifTag.GPSLatitudeRef)?.Value as string;
				Rational[] exifLongitude = exif.GetValue(ExifTag.GPSLongitude)?.Value as Rational[];
				string longitudeRef = exif.GetValue(ExifTag.GPSLongitudeRef)?.Value as string;

				double latitude, longitude;

				if (exifLatitude == null || latitudeRef == null || exifLongitude == null || longitudeRef == null) return null;

				if (latitudeRef == "S") latitude = (exifLatitude[0].ToDouble() * -1.0) + (((exifLatitude[1].ToDouble() * -60.0) + (exifLatitude[2].ToDouble() * -1.0)) / 3600.0);
				else latitude = exifLatitude[0].ToDouble() + (((exifLatitude[1].ToDouble() * 60.0) + exifLatitude[2].ToDouble()) / 3600.0);

				if (longitudeRef == "W") longitude = (exifLongitude[0].ToDouble() * -1.0) + (((exifLongitude[1].ToDouble() * -60.0) + (exifLongitude[2].ToDouble() * -1)) / 3600.0);
				else longitude = exifLongitude[0].ToDouble() + (((exifLongitude[1].ToDouble() * 60.0) + exifLongitude[2].ToDouble()) / 3600.0);

				return (latitude, longitude);
			}
			catch (MagickMissingDelegateErrorException e)
			{
				throw new InvalidDataException("Uploaded image could not be decoded by ImageMagick", e);
			}
		}

		public byte[] CreateThumbnail(IFormFile photo, int width, int height)
		{
			try
			{
				using var processedStream = new MemoryStream();
				using var uploadedStream = photo.OpenReadStream();
				using MagickImage image = new MagickImage(uploadedStream);

				image.Resize(width, height);
				image.RePage();

				image.Write(processedStream, MagickFormat.Jpg);

				processedStream.Seek(0, SeekOrigin.Begin);

				using var reader = new BinaryReader(processedStream);
				return reader.ReadBytes((int)processedStream.Length);
			}
			catch (MagickMissingDelegateErrorException e)
			{
				throw new InvalidDataException("Uploaded image could not be decoded by ImageMagick", e);
			}
		}
	}
}
