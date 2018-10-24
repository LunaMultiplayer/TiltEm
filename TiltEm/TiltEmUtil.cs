using System;
using System.IO;
using System.Linq;

namespace TiltEm
{
    public class TiltEmUtil
    {        
        /// <summary>
        /// Combine the paths specified as .net 3.5 doesn't give you a good method
        /// </summary>
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }
            return paths.Aggregate(Path.Combine);
        }
    }
}
