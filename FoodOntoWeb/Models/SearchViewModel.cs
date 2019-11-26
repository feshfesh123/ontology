using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodOntoWeb.Models
{
    public class SearchViewModel
    {
        public float Energy { get; set; }
        public float Protein { get; set; }
        public float Fat { get; set; }
        public float Carb { get; set; }
        public int Type { get; set; } = 0;
      
        public List<string> Result { get; set; } = new List<string>();
    }
}
