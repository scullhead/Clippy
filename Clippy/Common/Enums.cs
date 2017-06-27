﻿namespace Clippy.Common
{
    public enum DataKind
    {
        Undefined,
        PlainText,
        Image
    }

    public enum ItemsChangeType
    {
        Initialized,
        ListCleared,
        ItemAdded,
        ItemRemoved,
        ItemsLoaded
    }

    /// <summary>
    /// Possible modes how to handle the logfile
    /// If the logfile doesn't exist it will be created in all cases
    /// </summary>
    public enum LogMode
    {
        /// <summary>
        /// Attach log entries to existing file
        /// (Default log mode)
        /// </summary>
        attach,
        /// <summary>
        /// Overwrite file if existing
        /// </summary>
        overwrite,
    }
}