using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics2D.Animation
{
    /// <summary>
    /// Contains the number of columns and number of rows.
    /// </summary>
    public struct Animation2DFrameCount
    {
        /// <summary>
        /// Number of columns from the file
        /// </summary>
        public int NumberOfColumns;
        /// <summary>
        /// Number of rows from the file
        /// </summary>
        public int NumberOfRows;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="numberOfColumns"></param>
        /// <param name="numberOfRows"></param>
        public Animation2DFrameCount(int numberOfColumns, int numberOfRows)
        {
            this.NumberOfColumns = numberOfColumns;
            this.NumberOfRows = numberOfRows;
        }
    }
}
