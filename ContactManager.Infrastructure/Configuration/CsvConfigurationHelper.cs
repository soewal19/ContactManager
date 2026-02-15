using System.Globalization;
using CsvHelper.Configuration;

namespace ContactManager.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration helper for CSV parsing operations
    /// Provides standardized configurations for different scenarios
    /// </summary>
    public static class CsvConfigurationHelper
    {
        /// <summary>
        /// Gets the default CSV configuration optimized for contact data
        /// Uses invariant culture for consistent parsing across different locales
        /// </summary>
        /// <returns>Default CSV configuration</returns>
        public static CsvConfiguration GetDefaultConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                AllowComments = false,
                BadDataFound = null,
                ReadingExceptionOccurred = null,
                // Additional settings for robust parsing
                DetectColumnCountChanges = false,
                IgnoreReferences = true
            };
        }

        /// <summary>
        /// Gets CSV configuration with specific culture settings
        /// </summary>
        /// <param name="culture">Culture information for parsing</param>
        /// <returns>CSV configuration with specified culture</returns>
        public static CsvConfiguration GetConfigurationWithCulture(CultureInfo culture)
        {
            return new CsvConfiguration(culture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                AllowComments = false,
                BadDataFound = null,
                ReadingExceptionOccurred = null,
                DetectColumnCountChanges = false,
                IgnoreReferences = true
            };
        }

        /// <summary>
        /// Gets CSV configuration optimized for Russian locale
        /// </summary>
        /// <returns>Russian culture CSV configuration</returns>
        public static CsvConfiguration GetRussianConfiguration()
        {
            return GetConfigurationWithCulture(new CultureInfo("ru-RU"));
        }

        /// <summary>
        /// Gets CSV configuration for strict validation
        /// Throws exceptions for any data quality issues
        /// </summary>
        /// <returns>Strict CSV configuration</returns>
        public static CsvConfiguration GetStrictConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = args => throw new InvalidDataException($"Missing field at index: {args.Index}"),
                HeaderValidated = args => throw new InvalidDataException($"Header validation failed"),
                BadDataFound = args => throw new InvalidDataException($"Bad data found: {args.RawRecord}"),
                ReadingExceptionOccurred = args => throw new InvalidOperationException($"Reading exception: {args.Exception?.Message}"),
                IgnoreBlankLines = false,
                TrimOptions = TrimOptions.None,
                AllowComments = false,
                DetectColumnCountChanges = true
            };
        }

        /// <summary>
        /// Gets CSV configuration for bulk operations
        /// Optimized for performance with large datasets
        /// </summary>
        /// <returns>Bulk operation CSV configuration</returns>
        public static CsvConfiguration GetBulkConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                AllowComments = false,
                BadDataFound = null,
                ReadingExceptionOccurred = null,
                DetectColumnCountChanges = false,
                IgnoreReferences = true,
                BufferSize = 65536, // 64KB buffer for better performance
                CountBytes = false // Disable byte counting for performance
            };
        }
    }
}