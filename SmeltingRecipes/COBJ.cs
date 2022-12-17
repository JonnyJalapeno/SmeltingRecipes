using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmeltingRecipes
{
    public class COBJ
    {
        public FormKey ingot { get; set; }
        public ushort amount { get; set; }
        public Condition? cond;
    }
}
