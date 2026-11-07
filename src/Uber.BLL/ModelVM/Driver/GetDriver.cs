using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uber.BLL.ModelVM.Driver
{
    public class GetDriver
    {
        public string Name { get; set; }
        public double Rating { get; set; }
        public double Distance { get; set; }
        public double CurrentLng {  get; set; }
        public double CurrentLat { get; set; }
    }
}
