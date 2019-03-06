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

namespace GruntiMaps.ResourceAccess.GlobalCache
{
    public interface IGlobalFontCache
    {
        /// <summary>
        /// Get file path for specific font file.
        /// The directory is created unless it already exists.
        /// </summary>
        /// <param name="face">font face</param>
        /// <param name="range">font range</param>
        /// <param name="extension">optional, will take default pbf extension if null</param>
        /// <returns>file path</returns>
        string GetFilePath(string face, string range, string extension = "pbf");

        /// <summary>
        /// Get file content for specific font file.
        /// The directory is created unless it already exists.
        /// </summary>
        /// <param name="face">font face</param>
        /// <param name="range">font range</param>
        /// <param name="extension">optional, will take default pbf extension if null</param>
        /// <returns>file path</returns>
        byte[] GetFileContent(string face, string range, string extension = "pbf");

        /// <summary>
        /// Get MD5 hash of file for specific font file.
        /// </summary>
        /// <param name="face">font face</param>
        /// <param name="range">font range</param>
        /// <param name="extension">optional, will take default pbf extension if null</param>
        /// <returns>MD5 of file or null if file doesn't exist</returns>
        string GetFileMd5(string face, string range, string extension = "pbf");

        /// <summary>
        /// Delete the file for specific font file.
        /// </summary>
        /// <param name="face">font face</param>
        /// <param name="range">font range</param>
        /// <param name="extension">optional, will take default pbf extension if null</param>
        /// <returns>true/false for file's existense before deletion</returns>
        bool DeleteIfExist(string face, string range, string extension = "pbf");

        /// <summary>
        /// Check if file exists for specific font file
        /// </summary>
        /// <param name="face">font face</param>
        /// <param name="range">font range</param>
        /// <param name="extension">optional, will take default pbf extension if null</param>
        /// <returns>true/false for file's existense</returns>
        bool FileExists(string face, string range, string extension = "pbf");

        /// <summary>
        /// Check if file is available for specific font file
        /// </summary>
        /// <param name="face">font face</param>
        /// <param name="range">font range</param>
        /// <param name="extension">optional, will take default pbf extension if null</param>
        /// <returns>true/false for file's availability</returns>
        bool FileIsAvailable(string face, string range, string extension = "pbf");

        /// <summary>
        /// List available font faces
        /// </summary>
        /// <returns>font faces</returns>
        string[] ListFontFaces();

        /// <summary>
        /// List availabe font ranges
        /// </summary>
        /// <param name="face">font face</param>
        /// <returns>font ranges</returns>
        string[] ListFontRanges(string face);
    }
}
