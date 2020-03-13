using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
