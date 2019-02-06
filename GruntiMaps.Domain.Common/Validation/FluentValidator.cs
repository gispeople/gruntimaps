using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using System.Threading.Tasks;
using GruntiMaps.Domain.Common.Exceptions;

namespace GruntiMaps.Domain.Common.Validation
{
    public abstract class FluentValidator<TEntity> : AbstractValidator<TEntity>, IValidator<TEntity>
    {
        public new virtual async Task Validate(TEntity value)
        {
            var context = new ValidationContext<TEntity>(value);

            SetupRootContextData(value, context.RootContextData);

            var result = await ValidateAsync(context);

            if (!result.IsValid)
            {
                throw new ValidatorException(result.Errors.Select(e => new ValidatorError(e.PropertyName, e.ErrorMessage)));
            }
        }

        protected virtual void SetupRootContextData(TEntity value, IDictionary<string, object> data)
        {

        }
    }
}
