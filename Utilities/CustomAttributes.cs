using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace vykuttolib.Utilities
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public class RequiredTrueAttribute : ValidationAttribute, IClientModelValidator
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			return (value is bool b && b) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
		}

		public void AddValidation(ClientModelValidationContext context)
		{
			CustomAttributes.MergeAttribute(context.Attributes, "data-val", "true");
			var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
			CustomAttributes.MergeAttribute(context.Attributes, "data-val-requiredtrue", errorMessage);
		}

	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public class PhotoValidationAttribute : ValidationAttribute, IClientModelValidator
	{
		/// <summary>
		/// Gets or sets the minimum size in pixels (both width and height) an image should have to be accepted
		/// Values equal to MinSize will be accepted
		/// </summary>
		public int MinSize { get; set; } = 256;

		/// <summary>
		/// Gets or sets the maximum size in pixels (both width and height) an image should have to be accepted
		/// Values equal to MaxSize will be accepted
		/// </summary>
		public int MaxSize { get; set; } = 4096;

		/// <summary>
		/// Gets or sets the maximum filesize in bytes an image should have to be accepted
		/// Values equal to MaxFileSize will be accepted
		/// </summary>
		public int MaxFileSize { get; set; } = 4194304;


		public bool CanBeNull { get; set; } = false;

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return CanBeNull ? ValidationResult.Success : new ValidationResult("Musíte zvolit obrázek.");
			}


			// This should not be possible to achieve, it's just a warning for us in case we add it to the wrong type
			if (!(value is IFormFile photo)) return new ValidationResult($"Provided object must be of type 'IFormFile', is '{value.GetType().FullName}'.");

			if (!photo.ContentType.Contains("image/")) return new ValidationResult("Nahraný soubor musí být obrázek.");
			if (photo.Length > MaxFileSize) return new ValidationResult($"Nahraný soubor nesmí být větší než {MaxFileSize / 1024 / 1024} MiB.");

			try
			{
				using (var uploadedStream = photo.OpenReadStream())
				using (MagickImage image = new MagickImage(uploadedStream))
				{
					if (image.Width < MinSize || image.Height < MinSize) return new ValidationResult($"Obrázek musí být minimálně {MinSize}x{MinSize} pixelů velký.");
					if (image.Width > MaxSize || image.Height > MaxSize) return new ValidationResult($"Obrázek musí být maximálně {MaxSize}x{MaxSize} pixelů velký.");
				}
			}
			catch (MagickMissingDelegateErrorException)
			{
				return new ValidationResult("Nahraný soubor musí být podporovaný obrázek.");
			}

			return ValidationResult.Success;
		}

		public void AddValidation(ClientModelValidationContext context)
		{
			CustomAttributes.MergeAttribute(context.Attributes, "data-val", "true");
			var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
			CustomAttributes.MergeAttribute(context.Attributes, "data-val-minsize", MinSize.ToString());
			CustomAttributes.MergeAttribute(context.Attributes, "data-val-maxsize", MaxSize.ToString());
			CustomAttributes.MergeAttribute(context.Attributes, "data-val-maxfilesize", MaxFileSize.ToString());
			CustomAttributes.MergeAttribute(context.Attributes, "data-val-canbenull", CanBeNull.ToString());
			CustomAttributes.MergeAttribute(context.Attributes, "data-val-photovalidation", errorMessage);
			//todo: client side part not implemented
		}
	}

	public static class CustomAttributes
	{
		public static bool MergeAttribute(
			IDictionary<string, string> attributes,
			string key,
			string value)
		{
			if (attributes.ContainsKey(key))
			{
				return false;
			}
			attributes.Add(key, value);
			return true;
		}
	}
}
