using FluentValidation;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Domain.Common.Validation;

namespace GruntiMaps.WebAPI.Domain.Validators
{
    public class UpdateLayerDtoValidator : FluentValidator<UpdateLayerDto>
    {
        public UpdateLayerDtoValidator()
        {
            RuleFor(x => x)
                .Must(ProvideNameAndDescriptionIfUpdatingLayerSource)
                .WithMessage("Name, Description and DataLocation must be provided if updating layer source");
        }

        private bool ProvideNameAndDescriptionIfUpdatingLayerSource(UpdateLayerDto dto)
        {
            var hasName = !string.IsNullOrWhiteSpace(dto.Name);
            var hasDescription = !string.IsNullOrWhiteSpace(dto.Description);
            var hasData = !string.IsNullOrWhiteSpace(dto.DataLocation);
            return (hasName && hasDescription && hasData) || (!hasName && !hasDescription && !hasData);
        }
    }
}
