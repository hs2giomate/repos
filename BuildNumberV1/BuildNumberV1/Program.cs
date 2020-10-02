using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Principal;


namespace BuildNumberV1
{
    class Program
    {
        public static void Main()
        {
            string CD = Directory.GetCurrentDirectory();
            string VC_file = Path.Combine(CD, "Application\\version.c");
            int build_number;

            try
            {
                using (StreamReader version = new StreamReader(VC_file))

                {
                    string build_line = "const uint16_t build =";
                    string PatternB = build_line + @"\s?\d*";
                    string PatternU = "const char" + @"\*\s?" + "DeveloperName =";
                    string PatternC = "const char" + @"\*\s?" + "ComputerName =";

                    using (StreamWriter versionTemp = new StreamWriter(Path.Combine(CD, "Application\\versionTemp.c")))
                    {
                        while (!version.EndOfStream)
                        {

                            var line = version.ReadLine();
                            if (Regex.IsMatch(line, PatternB))
                            {
                                string[] replacement = line.Split('=', ';');
                                build_number = Int16.Parse(replacement[1]);
                                build_number++;
                                build_line += build_number.ToString() + ";";
                                versionTemp.WriteLine(build_line);
                            }
                            else if (Regex.IsMatch(line, PatternU))
                            {
                                string DeveloperName = Environment.UserName;
                                string[] replacement = line.Split('"');
                                build_line = replacement[0] + '"' + DeveloperName + '"' + ";";
                                versionTemp.WriteLine(build_line);
                            }
                            else if (Regex.IsMatch(line, PatternC))
                            {
                                string ComputerName = Environment.MachineName;
                                string[] replacement = line.Split('"');
                                build_line = replacement[0] + '"' + ComputerName + '"' + ";";
                                versionTemp.WriteLine(build_line);
                            }

                            else
                            {
                                versionTemp.WriteLine(line);
                            }



                        }
                    }



                }


            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file is not Found: '{e}'");
            }
            catch (IOException e)
            {
                Console.WriteLine($"The file coudl not be opened: '{e}'");
            }
        }
    }
}
