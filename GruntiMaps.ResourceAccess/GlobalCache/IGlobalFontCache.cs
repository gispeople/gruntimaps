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
