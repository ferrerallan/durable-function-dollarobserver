using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableDollarObserver.Models
{
    public class Rates
    {
        public double BRL { get; set; }
    }

    public class CurrenciesResponse
    {        
        public string disclaimer { get; set; }
        public string license { get; set; }
        public string timestamp { get; set; }
        public Rates rates { get; set; }

    }
}
