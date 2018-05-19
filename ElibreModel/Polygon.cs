using ClipperLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibreModel
{
    public class Polygonn
    {
        private List<IntPoint> _polygon;

        public Polygon()
        {
            _polygon = new List<IntPoint>();
        }

        public List<IntPoint> Instance()
        {
            return _polygon;
        }
    }

    public class Subject : Polygon
    {

    }

    public class Clip : Polygon
    {

    }

    public class Solution : Polygon
    {

    }
}
