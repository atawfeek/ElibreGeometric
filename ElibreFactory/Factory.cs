using ClipperLib;
using Elibre.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibreFactory
{

    public static class Factory
    {
        private static Dictionary<string, Polygons> polygons;

        static Factory() //Simple Factory Pattern
        {
            polygons = new Dictionary<string, Polygons>();
        }

        public static Polygons Create(string polType)
        {
            if (polygons.Count == 0) //Lazy Loading Pattern
            {
                polygons.Add("subject", new Subject());
                polygons.Add("clip", new Clip());
                polygons.Add("solution", new Solution());
            }

            return polygons[polType]; //Polymorphism design pattern
        }

    }

    public class Subject : Polygons
    {

    }

    public class Clip : Polygons
    {

    }

    public class Solution : Polygons
    {

    }
}
