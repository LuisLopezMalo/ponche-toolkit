using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Builder
{
    public class BuildEffects
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                Console.WriteLine("Arguments missing. Trying to compile every shader from the current path.");

            EffectHelper helper = new EffectHelper();
            helper.ParseArguments(args);
            helper.CompileShader();
        }

        
    }
}
