using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.Tfa
{
    internal class Rfc4226SecretClaim
    {
        public string Address { get; set; }
        public byte[] Secret { get; set; }
        public long Counter { get; set; }

        public long StartValue { get; set; }

        public long TimeBase { get; set; }

        public int CodeLength { get; set; }

        public bool Initialized { get; set; }
        
    }
}
