using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics2D
{
    /// <summary>
    /// Class that has Fonts properties.
    /// </summary>
    public class PTFont
    {
        private string name;

        /// <summary>
        /// Name of the font.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="familyFontName">The name of the font family.</param>
        public PTFont(string familyFontName)
        {
            this.name = familyFontName;
        }
    }
}
