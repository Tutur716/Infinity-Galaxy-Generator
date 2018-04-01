﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Infinity.Datas;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;

namespace Infinity.Generators
{
    class Generator
    {
        /// <summary>
        /// Generates a procedural star
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> Star(
            Dictionary<string, Dictionary<string, string>> starDatabase, Dictionary<string, double> galaxySettings, string gameDataPath, Random random)//galaxySetings is used for the orbit generator
        {
            //Generates the orbit
            Dictionary<string, double> orbit = new Dictionary<string, double>();
            orbit = Orbit.RandomOrbit(galaxySettings, random);

            //Converts with dots instead of commas + in string
            Dictionary<string, string> orbitProperties = new Dictionary<string, string>();
            orbitProperties.Add("Inclination", Convert.ToString(orbit["inclination"]).Replace(",", "."));
            orbitProperties.Add("Eccentricity", Convert.ToString(orbit["eccentricity"]).Replace(",", "."));
            orbitProperties.Add("Semi Major Axis", Convert.ToString(orbit["semiMajorAxis"]).Replace(",", "."));
            orbitProperties.Add("Mean Anomaly At Epoch", Convert.ToString(orbit["meanAnomalyAtEpoch"]).Replace(",", "."));
            orbitProperties.Add("Longitude Of Ascending Node", Convert.ToString(orbit["longitudeOfAscendingNode"]).Replace(",", "."));
            orbitProperties.Add("Epoch", Convert.ToString(orbit["epoch"]).Replace(",", "."));

            //Generates star class
            Dictionary<string, string> globalPropertiesComma = StarGenerator.Generate(starDatabase, gameDataPath, random);

            Dictionary<string, string> globalProperties = new Dictionary<string, string>();
            globalProperties.Add("Star Class", globalPropertiesComma["Star Class"].Replace(",", "."));
            globalProperties.Add("Temperature", globalPropertiesComma["Temperature"].Replace(",", "."));
            globalProperties.Add("Radius", globalPropertiesComma["Radius"].Replace(",", "."));
            globalProperties.Add("Luminosity", globalPropertiesComma["Luminosity"].Replace(",", "."));
            globalProperties.Add("Mass", globalPropertiesComma["Mass"].Replace(",", "."));
            globalProperties.Add("Color Red", globalPropertiesComma["Color Red"].Replace(",", "."));
            globalProperties.Add("Color Green", globalPropertiesComma["Color Green"].Replace(",", "."));
            globalProperties.Add("Color Blue", globalPropertiesComma["Color Blue"].Replace(",", "."));
            globalProperties.Add("Star Luminosity Class", globalPropertiesComma["Star Luminosity Class"].Replace(",", "."));
            globalProperties.Add("Corona Path", globalPropertiesComma["Corona Path"].Replace(",", "."));
            globalProperties.Add("Habitable Zone Min", globalPropertiesComma["Habitable Zone Min"].Replace(",", "."));
            globalProperties.Add("Habitable Zone Best", globalPropertiesComma["Habitable Zone Best"].Replace(",", "."));
            globalProperties.Add("Habitable Zone Max", globalPropertiesComma["Habitable Zone Max"].Replace(",", "."));

            //Generates intensity curves
            Double.TryParse(globalPropertiesComma["Luminosity"], out double luminosity);
            Double.TryParse(globalPropertiesComma["Radius"], out double radius);
            string starClass = globalPropertiesComma["Star Class"];

            //Scaled Space max: 1.5E+11
            double maxValue = 1.5E+11;

            double key1SCx = 2E+07;
            key1SCx *= luminosity;

            double key2SCx = 1E+9;
            key2SCx *= luminosity;

            double key3SCx = 2.82E+9;
            key3SCx *= luminosity;

            //if (starClass.Equals("O"))
            luminosity *= 40;
            if (radius > 1)
                luminosity *= radius;
            else
                luminosity /= radius;



            //Normal
            maxValue = 1.5E+12;

            double key1x = 1.35E+10;
            key1x *= luminosity;

            double key2x = 1E+11;
            key2x *= luminosity;

            double key3x = 2.82E+11;
            key3x *= luminosity;

            //Power number
            double key0y = 1; //Kerbol is 0.9 but hey.
            //key0y *= luminosity;
            double key1y = 0.7;
            //key1y *= luminosity;
            double key2y = 0.2;
            //key2y *= luminosity;


            globalProperties.Add("IN KEY 1x", Convert.ToString(key1x));
            globalProperties.Add("IN KEY 2x", Convert.ToString(key2x));
            globalProperties.Add("IN KEY 3x", Convert.ToString(key3x));
            globalProperties.Add("IN KEY SC 1x", Convert.ToString(key1SCx));
            globalProperties.Add("IN KEY SC 2x", Convert.ToString(key2SCx));
            globalProperties.Add("IN KEY SC 3x", Convert.ToString(key3SCx));
            globalProperties.Add("IN KEY 0y", Convert.ToString(key0y));
            globalProperties.Add("IN KEY 1y", Convert.ToString(key1y));
            globalProperties.Add("IN KEY 2y", Convert.ToString(key2y));


            //Generates sunflare size curve
            Double.TryParse(globalPropertiesComma["Radius"], out double radiusInKerbolRadius);

            radiusInKerbolRadius *= 6.957e+8; //In meters
            radiusInKerbolRadius /= 3; //KSP Size
            radiusInKerbolRadius /= 261600000; //Obtains percentage of kerbol's radius

            key1x = 0.5;
            key1x /= radiusInKerbolRadius;
            key2x = 0.9;
            key2x /= radiusInKerbolRadius;
            key3x = 10;
            key3x /= radiusInKerbolRadius;

            globalProperties.Add("BC KEY 1x", Convert.ToString(key1x).Replace(",", "."));
            globalProperties.Add("BC KEY 2x", Convert.ToString(key2x).Replace(",", "."));
            globalProperties.Add("BC KEY 3x", Convert.ToString(key3x).Replace(",", "."));

            //Makes teh finol packoge
            Dictionary<string, Dictionary<string, string>> properties = new Dictionary<string, Dictionary<string, string>>();
            properties.Add("Orbital Properties", orbitProperties);
            properties.Add("Global Properties", globalProperties);

            return properties;
        }

        /// <summary>
        /// Get the template file then save it with star's generated values
        /// </summary>
        public static string StarFile(Dictionary<string, Dictionary<string, string>> starProperties, string gameDataPath, int starCount, Dictionary<string, double> doubleDataDic)
        {
            string template = File.ReadAllText(gameDataPath + "\\Infinity\\Templates\\Star.cfg");

            string starFile = template
                .Replace("NEEDS[!Kopernicus]", "FOR[Infinity]")
                .Replace("#VAR-ID", Convert.ToString(starCount + 1))                               //ID
                .Replace("#VAR-STARCLASS", Datas.Query.Star.Specific(starProperties, "Global Properties", "Star Class"))
                .Replace("#VAR-INC", Datas.Query.Star.Specific(starProperties, "Orbital Properties", "Inclination"))           //Inclination
                .Replace("#VAR-ECC", Datas.Query.Star.Specific(starProperties, "Orbital Properties", "Eccentricity"))            //Eccentricity
                .Replace("#VAR-SMA", Datas.Query.Star.Specific(starProperties, "Orbital Properties", "Semi Major Axis"))           //SemiMajorAxis
                .Replace("#VAR-MAE", Datas.Query.Star.Specific(starProperties, "Orbital Properties", "Mean Anomaly At Epoch"))      //MeanAnomalyAtEpoch
                .Replace("#VAR-LAN", Datas.Query.Star.Specific(starProperties, "Orbital Properties", "Longitude Of Ascending Node"))//LAN
                .Replace("#VAR-EPO", Datas.Query.Star.Specific(starProperties, "Orbital Properties", "Epoch"))                   //Epoch
                .Replace("#VAR-COLOR-RED", Datas.Query.Star.Specific(starProperties, "Global Properties", "Color Red"))                                     //color
                .Replace("#VAR-COLOR-GREEN", Datas.Query.Star.Specific(starProperties, "Global Properties", "Color Green"))
                .Replace("#VAR-COLOR-BLUE", Datas.Query.Star.Specific(starProperties, "Global Properties", "Color Blue"))
                .Replace("#VAR-RADIUS", Datas.Query.Star.Specific(starProperties, "Global Properties", "Radius"))
                .Replace("#VAR-MASS", Datas.Query.Star.Specific(starProperties, "Global Properties", "Mass"))
                .Replace("#VAR-LUMINOSITY", Datas.Query.Star.Specific(starProperties, "Global Properties", "Luminosity"))
                .Replace("#VAR-TEMPERATURE", Datas.Query.Star.Specific(starProperties, "Global Properties", "Temperature"))
                .Replace("#VAR-CORONA", Datas.Query.Star.Specific(starProperties, "Global Properties", "Corona Path"))
                .Replace("#VAR-STARLUMCLASS", Datas.Query.Star.Specific(starProperties, "Global Properties", "Star Luminosity Class"))
                .Replace("#VAR-INTENSITY-KEY1X", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY 1x"))
                .Replace("#VAR-INTENSITY-KEY2X", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY 2x"))
                .Replace("#VAR-INTENSITY-KEY3X", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY 3x"))
                .Replace("#VAR-INTENSITYSC-KEY1X", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY SC 1x"))
                .Replace("#VAR-INTENSITYSC-KEY2X", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY SC 2x"))
                .Replace("#VAR-INTENSITYSC-KEY3X", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY SC 3x"))
                .Replace("#VAR-INTENSITY-KEY0Y", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY 0y"))
                .Replace("#VAR-INTENSITY-KEY1Y", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY 1y"))
                .Replace("#VAR-INTENSITY-KEY2Y", Datas.Query.Star.Specific(starProperties, "Global Properties", "IN KEY 2y"))
                .Replace("#VAR-BRIGHTNESSCURVE-KEY1X", Datas.Query.Star.Specific(starProperties, "Global Properties", "BC KEY 1x"))
                .Replace("#VAR-BRIGHTNESSCURVE-KEY2X", Datas.Query.Star.Specific(starProperties, "Global Properties", "BC KEY 2x"))
                .Replace("#VAR-BRIGHTNESSCURVE-KEY3X", Datas.Query.Star.Specific(starProperties, "Global Properties", "BC KEY 3x"));

            //habZonePlanetHelp(gameDataPath, starCount, starProperties);

            //Creates Wormholes
            string wormholeFileUp = null;
            string wormholeFileDown = null;
            string targetStar = Convert.ToString(starCount + 2);

            if (doubleDataDic["starNumber"] == starCount + 1)
                targetStar = "Sun";
            Wormhole(gameDataPath, starCount, doubleDataDic, starProperties, out wormholeFileDown, out wormholeFileUp);


            File.WriteAllText(gameDataPath + @"\Infinity\StarSystems\\Wormholes\Wormhole Down to Star " + Convert.ToString(starCount) + ".cfg", wormholeFileDown);
            File.WriteAllText(gameDataPath + @"\Infinity\StarSystems\\Wormholes\Wormhole Up to Star " + Convert.ToString(starCount + 2) + ".cfg", wormholeFileUp);


            return starFile;
        }

        /// <summary>
        /// Creates wormholes to go to the next star and the previous
        /// </summary>
        public static void Wormhole(string gameDataPath, int starCount, Dictionary<string, double> doubleDataDic, Dictionary<string, Dictionary<string, string>> starProperties, out string fileDown, out string fileUp)
        {
            
            string template = File.ReadAllText(gameDataPath + @"\Infinity\Templates\Wormhole.cfg");

            string partner = Convert.ToString(starCount);

            //Do the down way worhole
            if (starCount + 1 == 1) //If first star generated
                partner = "Sun";
            else
                partner = "Star " + starCount;

            fileDown = template
                .Replace("NEEDS[!Kopernicus]", "FOR[Infinity]")
                .Replace("#VAR-PARTNER", partner)
                .Replace("#VAR-ID", Convert.ToString(starCount + 1))
                .Replace("#VAR-RADIUS", Datas.Query.Star.Specific(starProperties, "Global Properties", "Radius"))
                .Replace("#VAR-SMA", "0")
                .Replace("#VAR-WAY", "Down")
                .Replace("#VAR-WAZ", "Up");

            //Do the up way worhole
            partner = "Star " + Convert.ToString(starCount + 2);

            if (starCount + 1 != doubleDataDic["starNumber"]) //If it is not the last star generated
            {
                fileUp = template
                   .Replace("NEEDS[!Kopernicus]", "FOR[Infinity]")
                   .Replace("#VAR-PARTNER", partner)
                   .Replace("#VAR-ID", Convert.ToString(starCount + 1))
                   .Replace("#VAR-RADIUS", Datas.Query.Star.Specific(starProperties, "Global Properties", "Radius"))
                   .Replace("#VAR-SMA", Convert.ToString(Math.PI))
                   .Replace("#VAR-WAY", "Up")
                   .Replace("#VAR-WAZ", "Down");
            }
            else
                fileUp = "";
        }

        /// <summary>
        /// Creates a new orbit for Kerbol
        /// </summary>
        public static string NewKerbolPosition(string gameDataPath, Dictionary<string, double> galaxySettings, Random random)
        {
            string template = File.ReadAllText(gameDataPath + "\\Infinity\\Templates\\BaseSystemOrbit.cfg");

            //Generates the orbit
            Dictionary<string, double> orbit = new Dictionary<string, double>();

            orbit = Orbit.RandomOrbit(galaxySettings, random);

            //Converts with dots instead of commas + in string
            Dictionary<string, string> orbitProperties = new Dictionary<string, string>();
            orbitProperties.Add("Inclination", Convert.ToString(orbit["inclination"]).Replace(",", "."));
            orbitProperties.Add("Eccentricity", Convert.ToString(orbit["eccentricity"]).Replace(",", "."));
            orbitProperties.Add("Semi Major Axis", Convert.ToString(orbit["semiMajorAxis"]).Replace(",", "."));
            orbitProperties.Add("Mean Anomaly At Epoch", Convert.ToString(orbit["meanAnomalyAtEpoch"]).Replace(",", "."));
            orbitProperties.Add("Longitude Of Ascending Node", Convert.ToString(orbit["longitudeOfAscendingNode"]).Replace(",", "."));
            orbitProperties.Add("Epoch", Convert.ToString(orbit["epoch"]).Replace(",", "."));

            string starFile = template
                .Replace("NEEDS[!Kopernicus]", "FOR[Infinity]")
                .Replace("#VAR-INC", orbitProperties["Inclination"])
                .Replace("#VAR-ECC", orbitProperties["Eccentricity"])
                .Replace("#VAR-SMA", orbitProperties["Semi Major Axis"])
                .Replace("#VAR-MAE", orbitProperties["Mean Anomaly At Epoch"])
                .Replace("#VAR-LAN", orbitProperties["Longitude Of Ascending Node"])
                .Replace("#VAR-EPO", orbitProperties["Epoch"]);

            return starFile;
        }
        
        /// <summary>
        /// Creates planets to visualize where is the habitable zone
        /// </summary>
        public static void habZonePlanetHelp(string gameDataPath, int starCount, Dictionary<string, Dictionary<string, string>> starProperties)
        {
            string[] planetFiles = new string[2];
            string template = File.ReadAllText(gameDataPath + @"\Infinity\Templates\TestPlanetHabZone.cfg");

            string planetFileHot = template
                .Replace("NEEDS[!Kopernicus]", "FOR[Infinity]")
                .Replace("#VAR-STARID", Convert.ToString(starCount + 1))
                .Replace("#VAR-ZONE", "Hot")
                .Replace("#VAR-SMA", Datas.Query.Star.Specific(starProperties, "Global Properties", "Habitable Zone Min"))
                .Replace("#VAR-COLOR", "#ff0000");

            string planetFileBest = template
                .Replace("NEEDS[!Kopernicus]", "FOR[Infinity]")
                .Replace("#VAR-STARID", Convert.ToString(starCount + 1))
                .Replace("#VAR-ZONE", "Best")
                .Replace("#VAR-SMA", Datas.Query.Star.Specific(starProperties, "Global Properties", "Habitable Zone Best"))
                .Replace("#VAR-COLOR", "#00ff00");

            string planetFileCold = template
                .Replace("NEEDS[!Kopernicus]", "FOR[Infinity]")
                .Replace("#VAR-STARID", Convert.ToString(starCount + 1))
                .Replace("#VAR-ZONE", "Cold")
                .Replace("#VAR-SMA", Datas.Query.Star.Specific(starProperties, "Global Properties", "Habitable Zone Max"))
                .Replace("#VAR-COLOR", "#0000ff");

            File.WriteAllText(gameDataPath + @"\Infinity\StarSystems\Planets\Planet Hot " + Convert.ToString(starCount + 1) + ".cfg", planetFileHot);
            File.WriteAllText(gameDataPath + @"\Infinity\StarSystems\Planets\Planet Best " + Convert.ToString(starCount + 1) + ".cfg", planetFileBest);
            File.WriteAllText(gameDataPath + @"\Infinity\StarSystems\Planets\Planet Cold " + Convert.ToString(starCount + 1) + ".cfg", planetFileCold);
        }

        /// <summary>
        /// Creates the galaxy with generated stars, planet, and other celestial bodies
        /// </summary>
        public static void Galaxy(
            string gameDataPath, Dictionary<string, double> doubleDataDic, Dictionary<string, Dictionary<string, string>> starDatabase, Random random)
        {

            string starFolder = gameDataPath + "\\Infinity\\StarSystems\\Stars";

            //Generates stars
            Stopwatch stopwatch = new Stopwatch();
            Console.Clear();
            Console.WriteLine("Generating Galaxy... Star     /{0}", doubleDataDic["starNumber"]);
            stopwatch.Start();
            for (int i = 0; i < doubleDataDic["starNumber"]; i++)
            {
                Console.SetCursorPosition(26, 0);
                Console.Write(i + 1);
                Dictionary<string, Dictionary<string, string>> Star = Generator.Star(starDatabase, doubleDataDic, gameDataPath, random);

                File.WriteAllText(gameDataPath + @"\Infinity\StarSystems\Stars\Star " + Convert.ToString(i + 1) + ".cfg", Generator.StarFile(Star, gameDataPath, i, doubleDataDic));

            }

            //Generates new Sun position
            File.WriteAllText(gameDataPath + @"\Infinity\StarSystems\Stars\Sun.cfg", NewKerbolPosition(gameDataPath, doubleDataDic, random));
            stopwatch.Stop();
            Console.WriteLine("\nDone! Time elapsed: {0:hh\\:mm\\:ss}", stopwatch.Elapsed);
        }
    }
}
