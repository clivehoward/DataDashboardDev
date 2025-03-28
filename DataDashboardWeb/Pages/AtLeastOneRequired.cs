using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DataDashboardWeb.Pages
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AtLeastOneRequired : ValidationAttribute, IClientModelValidator
    {
        public string OtherPropertyNames;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherPropertyNames">Multiple property name with comma(,) separator</param>
        public AtLeastOneRequired(string otherPropertyNames)
        {
            OtherPropertyNames = otherPropertyNames;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string[] propertyNames = OtherPropertyNames.Split(',');
            bool isAllNull = true;
            foreach (var i in propertyNames)
            {
                var p = validationContext.ObjectType.GetProperty(i);
                var val = p.GetValue(validationContext.ObjectInstance, null);
                if (val != null && val.ToString().Trim() != "")
                {
                    isAllNull = false;
                    break;
                }
            }

            if (isAllNull)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            else
            {
                return ValidationResult.Success;
            }
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            MergeAttribute(context.Attributes, "atleastonerequired", errorMessage);
            MergeAttribute(context.Attributes, "otherpropertynames", OtherPropertyNames);
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
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
