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
