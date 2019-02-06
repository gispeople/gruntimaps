using System.Threading.Tasks;

namespace GruntiMaps.Domain.Common.Validation
{
    public interface IValidator<in T>
    {
        /// <summary>
        /// Validates the input and throws a ValidatorException if it's invalid.
        /// </summary>
        Task Validate(T value);
    }
}
