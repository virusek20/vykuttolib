using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace vykuttolib.Services.PhotoProcessing
{
	/// <summary>
	/// Interface describing a service for resizing and convreting uploaded images
	/// </summary>
	public interface IPhotoProcessor
	{
		/// <summary>
		/// !DEPRECATED!
		/// Converts a stream into a representation that can be both stored in the database and served to the user.
		/// This format is JPG for non-transparent and PNG for transparent, unless specified by implemetation.
		/// </summary>
		/// <param name="photo">Input photo file</param>
		/// <param name="cropToSquare">Whether the input photo should be cropped to a square</param>
		/// <param name="transparency">Whether the out photo should have transparency</param>
		/// <returns>Optimized photo data</returns>
		[Obsolete("Use the Stream overload instead, both use the same implementation internally")]
		byte[] ProcessUploadedImage(IFormFile photo, bool cropToSquare, bool transparency = false);

		/// <summary>
		/// Converts a stream into a representation that can be both stored in the database and served to the user.
		/// This format is JPG for non-transparent and PNG for transparent, unless specified by implemetation.
		/// </summary>
		/// <param name="stream">Input photo stream</param>
		/// <param name="cropToSquare">Whether the input photo should be cropped to a square</param>
		/// <param name="transparency">Whether the out photo should have transparency</param>
		/// <returns>Optimized photo data</returns>
		byte[] ProcessUploadedImage(Stream stream, bool cropToSquare, bool transparency = false);

		/// <summary>
		/// Converts a stream into a representation that can be both stored in the database and served to the user.
		/// This format is JPG for non-transparent and PNG for transparent, unless specified by implemetation.
		/// This image is then spliced into multiple subimages depending on <paramref name="slices"/>
		/// </summary>
		/// <param name="stream">Input photo stream</param>
		/// <param name="cropToSquare">Whether the input photo should be cropped to a square</param>
		/// <param name="transparency">Whether the out photo should have transparency</param>
		/// <param name="slices">Slicing information used for creating subimages</param>
		/// <returns>List of created subimages</returns>
		List<TrimPhoto> ProcessUploadedImage(Stream stream, bool cropToSquare, List<Slice> slices, bool transparency = false);

		/// <summary>
		/// Trims an uniform edge around an image (without any noise) and returns both the trimmed image and page information
		/// </summary>
		/// <param name="stream">Input photo stream</param>
		/// <returns>Trim data and information</returns>
		TrimPhoto Trim(Stream stream, bool transparency = false);

		/// <summary>
		/// Converts a stream into a representation that can be both stored in the database and served to the user scaled to a desired resolution.
		/// This format is JPG for non-transparent and PNG for transparent, unless specified by implemetation.
		/// If the aspect ratios of desired size and input file don't match the image will be scaled while preserving original aspect ratio
		/// </summary>
		/// <param name="photo">Input photo file</param>
		/// <param name="width">Target width</param>
		/// <param name="height">Target height</param>
		/// <param name="transparency">Whether the out photo should have transparency</param>
		/// <returns>Thumbnail data</returns>
		byte[] CreateThumbnail(IFormFile photo, int width, int height, bool transparency = false);

		/// <summary>
		/// Attempts to read EXIF data of an image and parse out the GPS coordinates of the photo.
		/// In case no EXIF data exists or location is not saved, returns null.
		/// </summary>
		/// <param name="photo">Input photo file</param>
		/// <returns>GPS coordinates (latitude, longitude) or null</returns>
		(double, double)? GetExifGpsCoordinates(IFormFile photo);
	}
}
