using CsvHelper;
using System;
using System.Globalization;
using System.Linq;

namespace AiConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string inputFile = "";
            string zonesTargets = "";
            string outputFile = "out.bin";
            if (args.Length == 0) {
                Console.WriteLine("Manual Mode Enabled");
                while (!(zonesTargets.ToLower() == "zones" || zonesTargets.ToLower() == "targets"))
                {
                    Console.WriteLine("\"zones\" or \"targets\":");
                    zonesTargets = Console.ReadLine();
                }
                while (!(File.Exists(inputFile) || inputFile.ToLower().EndsWith(".csv")))
                {
                    Console.WriteLine("Path to input file (.csv):");
                    inputFile = Console.ReadLine();
                }
                Console.WriteLine("Path to output file (Leave blank for default):");
                var output = Console.ReadLine();
                if (output != null && output != "") outputFile = output;
            } else
            {
                if (args.Length < 2) {
                    Help();
                    return;
                }
                zonesTargets = args[0];
                inputFile = args[1];
                if (args.Length > 2)
                {
                    outputFile = args[2];
                }
                if (!File.Exists(inputFile) || !args[2].EndsWith(".csv"))
                {
                    Help();
                    return;
                }
                
            }

            if (zonesTargets == "targets")
            {
                Console.WriteLine("Targets are not implemented in this verion.");
            }
            else if (zonesTargets == "zones")
            {
                List<byte> rawZones = new List<byte>();
                using (var reader = new StreamReader(inputFile))
                using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
                {
                    var records = csv.GetRecords<Zone>();

                    var zones = records.ToArray();
                    foreach(Zone zone in zones)
                    {
                        var zonebytes = zone.GetBytes();
                        if (zonebytes == null) { return; }
                        rawZones.AddRange(zonebytes);
                    }
                    File.WriteAllBytes(outputFile, rawZones.ToArray());
                }
            } else
            {
                Help();
                return;
            }
        }
        public static void Help()
        {
            Console.WriteLine("usage: AiConverter {zones|targets(Incomplete)} csvfile.csv [output]");
            Console.WriteLine("Example CSV and template can be found with the executable");
            Console.WriteLine("Make sure your file is a .csv file and not another spreadsheet format like .xlsx");
            Console.WriteLine("Zones values should be even or they will be incorrect in game.");
            Console.WriteLine("Zones Shapes:");
            Console.WriteLine("    0, r, rect, rectangle                         = rectangle");
            Console.WriteLine("    1, tul, triupleft, triangle up left           = triangle, corner in upper left");
            Console.WriteLine("    2, tur, triupright, triangle up right         = triangle, corner in upper right");
            Console.WriteLine("    3, tbl, tribotleft, triangle bottom left      = triangle, corner in bottom left");
            Console.WriteLine("    4, tbr, tribotright, rectriangle bottom right = triangle, corner in bottom right");
        }
    }

    public class Zone
    {
        public ushort PosX { get; set; }
        public ushort PosY { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public string Shape { get; set; }

        public byte[] GetBytes()
        {
            byte shape=0;
            switch (Shape.ToLower())
            {
                case "0":
                case "r":
                case "rect":
                case "rectangle":
                    shape = 0;
                    break;
                case "1":
                case "tul":
                case "triupleft":
                case "triangle up left":
                    shape = 1;
                    break;
                case "2":
                case "tur":
                case "triupright":
                case "triangle up right":
                    shape = 2;
                    break;
                case "3":
                case "tbl":
                case "tribotleft":
                case "triangle bottom left":
                    shape = 3;
                    break;
                case "4":
                case "tbr":
                case "tribotright":
                case "triangle bottom right":
                    shape = 4;
                    break;
                default:
                    Console.WriteLine("Incorrect Zone Shape!");

                    Program.Help();
                    return null;
            }

            return 
                new byte[] {shape}.Concat(
                BitConverter.GetBytes(PosX / 2)[0..2].Concat(
                BitConverter.GetBytes(PosY / 2)[0..2].Concat(
                BitConverter.GetBytes(Width / 2)[0..2].Concat(
                BitConverter.GetBytes(Height / 2)[0..2].Concat(
                new byte[] {0,0,0}
                ))))).ToArray();
        }
    }
}
