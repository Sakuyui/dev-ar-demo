using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using AR.ActivationControl;
using System.IO;
using System.Linq;


namespace AR.Parser
{

    public class ConfigurationParser
    {
        public static List<ConnectionGraph> LoadConnectionConfiguration(string path)
        {
            int parsingState = 0;
            var lines = File.ReadAllLines(path);

            ConnectionGraph? ParseLine(string line)
            {
                List<IndirectedEdge> edges = new List<IndirectedEdge>();
                List<int> currentParsedNumbers = new List<int>();
                StringBuilder currentParsingString = new StringBuilder();
                IndirectedEdge lastParsingEdge = null;
                var firstSpaceIndex = line.IndexOf(' ');
                int activateObjectID = int.Parse(line[..(firstSpaceIndex + 1)]);
                var items = line[(firstSpaceIndex + 1)..];
                for (int i = 0; i < items.Length; i++)
                {
                    char ch = items[i];
                    Console.WriteLine($"Encounter {ch}.");
                    if (parsingState == 0)
                    {
                        if (ch == '[')
                        {
                            parsingState = 1;
                            edges = new List<IndirectedEdge>();
                            Console.WriteLine($"set state to 1. Create new edge list.");

                        }
                    }
                    else if (parsingState == 1)
                    {
                        if (ch == '[')
                        {
                            currentParsingString.Clear();
                            currentParsedNumbers = new List<int>();
                            Console.WriteLine($"set state to 2. Create new number list.");
                            parsingState = 2;
                        }
                        else if (ch == ',')
                        {
                            edges.Add(lastParsingEdge);
                            lastParsingEdge = null;
                        }
                        else if (ch == ']')
                        {
                            edges.Add(lastParsingEdge);
                            lastParsingEdge = null;
                            return new()
                            {
                                activateObjectID = activateObjectID,
                                edges = edges
                            };
                        }
                    }
                    else if (parsingState == 2)
                    {
                        if (ch >= '0' && ch <= '9')
                        {
                            currentParsingString.Append(ch);
                            Console.WriteLine($"add number {ch}.");

                        }
                        else if (ch == ',')
                        {
                            currentParsedNumbers.Add(int.Parse(currentParsingString.ToString()));
                            Console.WriteLine($"Push {currentParsingString}");

                            currentParsingString.Clear();
                        }
                        else if (ch == ']')
                        {
                            currentParsedNumbers.Add(int.Parse(currentParsingString.ToString()));
                            Console.WriteLine($"Push {currentParsingString}");

                            currentParsingString.Clear();
                            if (currentParsedNumbers.Count != 2)
                            {
                                Console.WriteLine("Configuration Parsing Error");
                            }
                            lastParsingEdge = new()
                            {
                                vertex1 = currentParsedNumbers[0],
                                vertex2 = currentParsedNumbers[1]
                            };
                            currentParsedNumbers = null;

                            parsingState = 1;

                        }
                    }
                }
                return null;
            }


            List<ConnectionGraph> result = lines.Select(ParseLine).ToList();

            foreach (var config in result)
            {
                UnityEngine.Debug.Log($"activate id = {config.activateObjectID}");
                int i = 0;
                foreach (var edge in config.edges)
                {
                    UnityEngine.Debug.Log($" - edge {i} v1 = {edge.vertex1}, v2 = {edge.vertex2}");
                    i += 1;
                }
            }
            return result;

        }
    }

}