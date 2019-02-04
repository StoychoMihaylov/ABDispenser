namespace ABDispenser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    class Program
    {
        public static int stepCounter = 0;

        static void Main(string[] args)
        {
            PrintInitials();

            var persons = new List<string>();
            var removedPersons = new List<string>();

            string[] commands = new string[]
            {
                "add",
                "remove",
                "start",
                "participants",
                "?"
            };

            while (true)
            {
                Console.Write("ABDispenser#>");
                var input = Console.ReadLine().Split();

                if (input[0] == "" || input[0] == null)
                {
                    
                    Console.WriteLine("");
                    continue;
                }

                if (input[0] == commands[0])
                {
                    if (persons.Contains(input[1]))
                    {                  
                        Console.WriteLine($"The name '{input[1]}' is already added!");
                    }
                    else
                    {
                        persons.Add(input[1]);
                        Console.WriteLine($"The name '{input[1]}' is added!");

                        // Set step counter to start from the new added person.
                        stepCounter = persons.Count() -1;
                    }
                }
                else if(input[0] == commands[1])
                {
                    if (persons.Contains(input[1]))
                    {
                        var removedPerson = input[1];
                        removedPersons.Add(removedPerson);
                        persons.Remove(removedPerson);
                        Console.WriteLine($"'{input[1]}' removed!");

                        // set step counter -1, safely avoiding overflowing the array of persons
                        while (stepCounter >= persons.Count())
                        {
                            stepCounter--;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"name '{input[1]}' doesn't exist!");
                    }
                }
                else if (input[0] == commands[3])
                {
                    Console.WriteLine("-Participants-");
                    for (int i = 0; i < persons.Count; i++)
                    {
                        Console.WriteLine($"{i + 1} - {persons[i]}");
                    }
                }
                else if (input[0] == commands[4])
                {
                    Console.WriteLine("----Commands----");
                    foreach (var command in commands)
                    {
                        Console.WriteLine("  " + command);
                    }
                    Console.WriteLine("----------------");
                }
                else if (input[0] == commands[2])
                {
                    // Start 
                    var result = DispenseAB(persons, removedPersons);
                    Console.WriteLine();
                    Console.WriteLine(result);
                }
            }
        }

        private static string DispenseAB(List<string> persons, List<string> removedPersons)
        {
            string[] simbs = new string[] { "-", "\\", "|", "/"};
            var stepLength = persons.Count;

            do
            {
                while (!Console.KeyAvailable)
                {
                    foreach (var simb in simbs)
                    {
                        Console.Write($"\rABDispenser is running: {simb}");
                        Thread.Sleep(100);
                    }

                    string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    DirectoryInfo directory = new DirectoryInfo(assemblyFolder);
                    FileInfo[] fileArray = directory.GetFiles("*.pdf");

                    foreach (FileInfo file in fileArray)
                    {
                        var filename = file.Name.ToLower();

                        var isMatchanyPerosn = persons.Any(pr => filename.Contains(pr.ToLower()));
                        var isMatchAnyRemovedPerson = removedPersons.Any(pr => filename.Contains(pr.ToLower()));

                        // If one of both matchs it will miss working with this file(continue with next one)
                        if (isMatchanyPerosn || isMatchAnyRemovedPerson)
                        {
                            continue;
                        }

                        if (!filename.Contains("rechnung") 
                            && !filename.Contains("warten") 
                            && !filename.Contains("lieferschein") 
                            && !filename.Contains("offen")
                            && !filename.Contains("gotov"))
                        {
                            
                            if (stepLength == 0)
                            {
                                return "Error: Please add persons!";
                            }
                            else if (stepCounter == stepLength)
                            {
                                stepCounter = 0;
                            }

                            try
                            {
                                var person = persons[stepCounter];
                                var newName = file.Name.Insert(0, persons[stepCounter] + "_");
                                File.Move(file.Name, newName);
                                stepCounter++;
                                AddLog(file.Name, person);
                            }
                            catch { }          
                        }
                    }
                } 
            } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);

            return "The ABDispenser is stopped!";
        }

        private static void AddLog(string abName, string person)
        {

            
            try
            {
                StreamWriter log = null;
                FileStream fileStream = null;
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);      
                
                var logFilePath = assemblyFolder + "\\" + "AB_Dispernser_LogFile.txt";
                FileInfo logFileInfo = new FileInfo(logFilePath);
                DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
                //if (!logDirInfo.Exists) logDirInfo.Create();
                if (!logFileInfo.Exists)
                {
                    fileStream = logFileInfo.Create();
                }
                else
                {
                    fileStream = new FileStream(logFilePath, FileMode.Append);
                }
                log = new StreamWriter(fileStream);
                log.WriteLine("--------------------------------------------------------------------------------------");
                log.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy")} | {person} | {abName}");
                log.WriteLine("--------------------------------------------------------------------------------------");
                log.Close();
            }
            catch { }   
        }

        private static void PrintInitials()
        {
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("                                             .d8b.          ");
            Console.WriteLine("       .d8888.              d8b    d8b      88 R 88         ");
            Thread.Sleep(700);
            Console.WriteLine("       88'  YP              888b  d888       'q8p'          ");
            Thread.Sleep(500);
            Console.WriteLine("       '8bo.   d88888888b   88'8bd8'88                      ");
            Thread.Sleep(300);
            Console.WriteLine("         'Y8b.    '88'      88  88  88                      ");
            Thread.Sleep(200);
            Console.WriteLine("       db   8D     88       88      88                      ");
            Thread.Sleep(10);
            Console.WriteLine("       '8888Y'     88  @    88      88  @                   ");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("1.To add person write 'add' followed by 'name' of the person.");
            Console.WriteLine("2.To remove person write 'remove' followed by the chosen 'name'.");
            Console.WriteLine("3.Use 'participants' to se all added persons.");
            Console.WriteLine("4.Use 'start' to start dispensing or click 'Spacebar' to stop it.");
            Console.WriteLine("5.Use '?' to see all the commands.");
            Console.WriteLine("-------------------------------------------------------------");
        }
    }
}
