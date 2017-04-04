using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogClient {
    public class HedgehogException : Exception {

        public HedgehogException(string message) : base(message) { }
    }
}
