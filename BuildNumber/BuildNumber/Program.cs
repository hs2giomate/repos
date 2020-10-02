using System;
using System.IO;
using System.Text.RegularExpressions;

namespace BuildNumberTest2
{
    class Program
    {
        

        public static void Main()
        {
            string CD = Directory.GetCurrentDirectory();
            string BN_file =Path.Combine(CD,"BuildNumber.txt");
            string VC_file = Path.Combine(CD,"Application\version.c");
            int build_number;
            if (File.Exists(BN_file))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(BN_file))
                    {
                        build_number = Int16.Parse(File.ReadAllText(BN_file));
                        build_number++;
                        Console.WriteLine(build_number);
                    }

                    using (StreamWriter sw = new StreamWriter(BN_file))
                    {
                        sw.WriteLine(build_number.ToString());
                    }
                    
                }
                catch(FileNotFoundException e)
                {
                    Console.WriteLine($"The file is not Found: '{e}'");
                }
                catch(IOException e)
                {
                    Console.WriteLine($"The file coudl not be opened: '{e}'");
                }

            }
            else
            {
                using (StreamWriter sw = new StreamWriter(BN_file))
                {
                    Console.WriteLine("New Building Number");
                    sw.WriteLine("0");
                }
                    
            }

            try
            {
                string versionFile = null;
                using (StreamReader srvc = new StreamReader(VC_file))
                {
                    versionFile = srvc.ReadToEnd();
                    
                }
                Regex  Old_build_number= new Regex("build =%bd)

                using (StreamWriter sw = new StreamWriter(BN_file))
                {
                    sw.WriteLine(build_number.ToString());
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
