using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace GruntiMaps.Domain.Common.Exceptions
{
    public class ValidatorException : Exception
    {
        public const string ErrorMessageKey = "ErrorMessage";
        public readonly IDictionary<string, IEnumerable<string>> Errors;

        public ValidatorException()
            : this(new ValidatorError[] { })
        { }

        public ValidatorException(params ValidatorError[] errors)
            : base(BuildErrorMessage(errors))
        {
            this.Errors = ToDictionary(errors);
        }

        public ValidatorException(IEnumerable<ValidatorError> errors)
            : this(errors.ToArray())
        { }

        private static string BuildErrorMessage(ValidatorError[] errors)
        {
            var dictionary = ToDictionary(errors);
            return JsonConvert.SerializeObject(dictionary, Formatting.Indented);
        }

        private static IDictionary<string, IEnumerable<string>> ToDictionary(ValidatorError[] errors)
        {
            var lookup = errors.ToLookup(e => e.Field, e => e.Message);
            var dictionary = lookup.ToDictionary(e => e.Key, e => lookup[e.Key]);

            var combinedMessage = new StringBuilder();
            foreach (var error in errors)
            {
                combinedMessage.AppendLine($"{error.Field}: {error.Message}");
            }

            dictionary[ErrorMessageKey] = new[] { combinedMessage.ToString() };
            return dictionary;
        }
    }
}
