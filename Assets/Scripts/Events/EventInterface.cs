using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public interface EventInterface
    {
        public void GatherOutflow();
        public void ApplyAndViability();
        public void GlobalRecharge();
        public void EntropyDiffuse();

    }
}
