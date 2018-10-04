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
// ReSharper disable InconsistentNaming (these are used to generate/parse mapbox json styles etc)
namespace GruntiMaps.Interfaces
{
    public interface IMapBoxSource
    {
        string tilejson { get; set; }
        string name { get; set; }
        string description { get; set; }
        string version { get; set; }
        string attribution { get; set; }
        string template { get; set; }
        string legend { get; set; }
        string scheme { get; set; }
        string[] tiles { get; set; }
        string[] grids { get; set; }
        string[] data { get; set; }
        double minzoom { get; set; }
        double maxzoom { get; set; }
        double[] bounds { get; set; }
        double[] center { get; set; }
        string type { get; set; }
        string format { get; set; }
    }
}