using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using Microsoft.AspNetCore.Http;

namespace vykuttolib.Services.PhotoProcessing
{
	public class MagickPhotoProcessor : IPhotoProcessor
	{
		public byte[] ProcessUploadedImage(Stream stream, bool cropToSquare, bool transparency)
		{
			try
			{
				using var processedStream = new MemoryStream();
				using MagickImage image = new MagickImage(stream);

				if (cropToSquare)
				{
					var shroterSide = Math.Min(image.Width, image.Height);
					image.Crop(shroterSide, shroterSide, Gravity.Center);
					image.RePage();
				}

				if (transparency) image.Write(processedStream, MagickFormat.Png);
				else image.Write(processedStream, MagickFormat.Jpg);

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

		public byte[] CreateThumbnail(Stream stream, int width, int height, bool transparency)
		{
			try
			{
				using var processedStream = new MemoryStream();
				using MagickImage image = new MagickImage(stream);

				image.Resize(width, height);
				image.RePage();

				if (transparency) image.Write(processedStream, MagickFormat.Png);
				else image.Write(processedStream, MagickFormat.Jpg);

				processedStream.Seek(0, SeekOrigin.Begin);

				using var reader = new BinaryReader(processedStream);
				return reader.ReadBytes((int)processedStream.Length);
			}
			catch (MagickMissingDelegateErrorException e)
			{
				throw new InvalidDataException("Uploaded image could not be decoded by ImageMagick", e);
			}
		}

		public TrimPhoto Trim(Stream stream, bool transparency)
		{
			var photo = new TrimPhoto();

			try
			{
				using var processedStream = new MemoryStream();
				using MagickImage image = new MagickImage(stream);

				image.Trim();
				photo.Width = image.Width;
				photo.Height = image.Height;
				photo.X = image.Page.X;
				photo.Y = image.Page.Y;

				image.RePage();

				if (transparency) image.Write(processedStream, MagickFormat.Png);
				else image.Write(processedStream, MagickFormat.Jpg);

				processedStream.Seek(0, SeekOrigin.Begin);

				using var reader = new BinaryReader(processedStream);
				photo.Data = reader.ReadBytes((int)processedStream.Length);

				return photo;
			}
			catch (MagickMissingDelegateErrorException e)
			{
				throw new InvalidDataException("Uploaded image could not be decoded by ImageMagick", e);
			}
		}

		public List<TrimPhoto> ProcessUploadedImage(Stream stream, bool cropToSquare, List<Slice> slices, bool transparency = false)
		{
			var photos = new List<TrimPhoto>();
			using MagickImage image = new MagickImage(stream);

			var vSlices = slices
				.Where(s => s.Direction == Slice.SliceDirection.Vertical)
				.OrderBy(s => s.Coordinate);

			var hSlices = slices
				.Where(s => s.Direction == Slice.SliceDirection.Horizontal)
				.OrderBy(s => s.Coordinate);

			// Top left corner
			var row = new List<int> { 0 };
			// All points along top edge
			foreach (var vSlice in vSlices) row.Add(vSlice.Coordinate);
			// Top right
			row.Add(image.Width);

			// Top left corner
			var column = new List<int> { 0 };
			// All points along left edge
			foreach (var hSlice in hSlices) column.Add(hSlice.Coordinate);
			// Bottom left
			column.Add(image.Height);

			for (int i = 0; i < row.Count - 1; i++)
			{
				for (int j = 0; j < column.Count - 1; j++)
				{
					stream.Seek(0, SeekOrigin.Begin);
					using var processedStream = new MemoryStream();
					using MagickImage spliceImage = new MagickImage(stream);

					spliceImage.Crop(new MagickGeometry
					{
						Width = row[i + 1] - row[i],
						Height = column[j + 1] - column[j],
						X = row[i],
						Y = column[j]
					});

					spliceImage.RePage();

					if (transparency) spliceImage.Write(processedStream, MagickFormat.Png);
					else spliceImage.Write(processedStream, MagickFormat.Jpg);

					processedStream.Seek(0, SeekOrigin.Begin);

					using var reader = new BinaryReader(processedStream);
					var photo = new TrimPhoto
					{
						Data = reader.ReadBytes((int)processedStream.Length),
						Width = spliceImage.Width,
						Height = spliceImage.Height,
						X = row[i],
						Y = column[j]
					};

					photos.Add(photo);
				}
			}

			return photos;
		}

        public (int w, int h) GetDimensions(Stream stream)
        {
			try
			{
				using MagickImage image = new MagickImage(stream);
				return (image.Width, image.Height);
			}
			catch (MagickMissingDelegateErrorException e)
			{
				throw new InvalidDataException("Uploaded image could not be decoded by ImageMagick", e);
			}
		}

        public byte[] ProcessUploadedImage(Stream stream, CropTransform crop, bool transparency = false)
        {
			try
			{
				using var processedStream = new MemoryStream();
				using MagickImage image = new MagickImage(stream);

				var magickCrop = new MagickGeometry((int)crop.X, (int)crop.Y, (int)crop.Width, (int)crop.Height);
				image.Crop(magickCrop);
				image.RePage();

				if (transparency) image.Write(processedStream, MagickFormat.Png);
				else image.Write(processedStream, MagickFormat.Jpg);

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
