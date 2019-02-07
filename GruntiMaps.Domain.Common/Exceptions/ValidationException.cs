/*

Copyright 2016, 2017, 2018 GIS People Pty Ltd

This file is part of GruntiMaps.

GruntiMaps is free software: you can redistribute it and/or modify it under 
the terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

GruntiMaps is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE. See the GNU Affero General Public License for more 
details.

You should have received a copy of the GNU Affero General Public License along
with GruntiMaps.  If not, see <https://www.gnu.org/licenses/>.

*/

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
