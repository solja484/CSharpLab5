using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMAAndrusiv05
{
    enum ProcessState
    {
        Active, NotResponding
    }

    class ProcessEntry
    {
        public string Name { get; private set; }

        public int Id { get; private set; }

        public ProcessState State { get; private set; }
        public int CPULoad{ get; private set; }

        public int MemoryPercent { get; private set; }
        public int Memory { get; private set; }
        public int ThreadCount { get; private set; }
        public string Username { get; private set; }
        public string Filepath { get; private set; }
        public DateTime LaunchTime { get; private set; }

    }
}
