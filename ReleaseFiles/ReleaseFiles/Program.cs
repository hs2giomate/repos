using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseFiles
{
    class Program
    {
        public static void Main(string[] args)
        {
            string ProjectName ="TestDoor"; ;
            string ReleaseNumber= "01";

            if ((args.Length<1))
            {
              //  throw new ArgumentNullException("No Arguments");
             
            }else {

            
                ProjectName = args[0];
                ReleaseNumber = args[1];
            }
            
           
            try
            {

                string CurrentDir = Directory.GetCurrentDirectory();
                DirectoryInfo dir = new DirectoryInfo(CurrentDir);
                string BN_Dir = Path.Combine(Directory.GetCurrentDirectory(), "BuildNumber");
                if (Directory.Exists(BN_Dir))
                {
                    
                    string  VC_file     =   Path.Combine(CurrentDir, "Application\\version.c");
                    string  PatternB    = "const uint16_t build =" + @"\s?\d*";
                    string BuildNumber  =   "0";
                    DirectoryInfo dirInfo = new DirectoryInfo(CurrentDir);

                    using (StreamReader version = new StreamReader(VC_file))
                    {
                        while (!version.EndOfStream)
                        {
                            var line = version.ReadLine();
                            if (Regex.IsMatch(line, PatternB))
                            {
                                string[] replacement = line.Split('=', ';');
                                BuildNumber = replacement[1];
                            }
                        }
                    }
                    string buildingName = "_Release"+ ReleaseNumber+ "_B"+ BuildNumber;
                    ProjectName += buildingName;
                    string PN_Dir = Path.Combine(BN_Dir, ProjectName);
                    if (Directory.Exists(PN_Dir))
                    {
                        Console.WriteLine("That Build Number exists already.");
                        return;
                    }
                    else
                    {
                        DirectoryInfo dp = Directory.CreateDirectory(PN_Dir);
                        Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(PN_Dir));
                    }

                    string fName = "";
                    string dName = "";

                    try
                    {
                        string[] files = Directory.GetFiles(CurrentDir);
                        string ext;
                        try
                        {
                            string[] ProjectFile = Directory.GetFiles(CurrentDir, "*.cppproj");
                            foreach (string file in ProjectFile)
                            {
                                fName = file.Substring(CurrentDir.Length + 1);
                                string[] replacement = fName.Split('.');
                                dName = replacement[0] + "_R" + ReleaseNumber + "_B" + BuildNumber + "." + replacement[1];

                                File.Copy(Path.Combine(CurrentDir, fName), Path.Combine(PN_Dir, dName), true);
                            }
                                                       
                            foreach (string file in files)
                            {
                                ext = Path.GetExtension(file);
                                if ((file != ProjectFile[0]) && ((ext == ".c") || (ext == ".cpp") || (ext == ".h")))
                                {
                                    fName = file.Substring(CurrentDir.Length + 1);

                                    File.Copy(Path.Combine(CurrentDir, fName), Path.Combine(PN_Dir, fName), true);
                                }

                            }
                        }
                        catch (Exception e1)
                        {

                            Console.WriteLine("Not Project found: {0}", e1.ToString());
                        }

                       
                           

                       

                        files = Directory.GetFiles(BN_Dir);
                        foreach (string file in files)
                        {
                            ext = Path.GetExtension(file);
                            if ((ext == ".bin") || (ext == ".eep") || (ext == ".elf")|| (ext == ".hex") || (ext == ".lss") || (ext == ".map") || (ext == ".txt"))
                            {
                               
                                fName = file.Substring(BN_Dir.Length + 1);

                                
                                if (ext == ".txt")
                                {
                                    using (StreamReader memoryFile = new StreamReader(Path.Combine(BN_Dir, fName)))
                                    {
                                                    
                                        using (StreamWriter BuildingNote = new StreamWriter(Path.Combine(PN_Dir, "buildNote.txt")))
                                        {
                                            BuildingNote.WriteLine("Memory ocupation ");
                                            BuildingNote.WriteLine("");
                                            while (!memoryFile.EndOfStream)
                                            {
                                                var line = memoryFile.ReadLine();
                                                BuildingNote.WriteLine(line);
                                            }
                                            BuildingNote.WriteLine("");
                                            BuildingNote.WriteLine("");
                                            BuildingNote.WriteLine(string.Concat("For more Details, Please check ", fName, "."));
                                        }
                                    }
                                }
                                else
                                {
                                    File.Copy(Path.Combine(BN_Dir, fName), Path.Combine(PN_Dir, fName), true);

                                }
                            }

                        }

                        string[] dirs= Directory.GetDirectories(CurrentDir);
                        foreach (string subdir in dirs)
                        {
                            int pos = subdir.LastIndexOf("\\") + 1;
                            dName = subdir.Substring(pos, subdir.Length - pos);
                            if ((dName != "Debug")&&(dName != "Release")&& (dName != ".git")&& (dName != ".vs")&& (dName != "BuildNumber"))
                            {
                                
                                DirectoryInfo ds= Directory.CreateDirectory(Path.Combine(PN_Dir, dName));
                                string[] sources = Directory.GetFiles(subdir);
                                foreach (string file in sources)
                                {
                                    fName = file.Substring(subdir.Length + 1);
                                    File.Copy(Path.Combine(subdir, fName), Path.Combine(ds.FullName, fName), true);
                                }
                            }
                        }
                         
                                          

                        
                    }

                    catch (DirectoryNotFoundException dirNotFound)
                    {
                        Console.WriteLine(dirNotFound.Message);
                    }

                }
                else
                {
                    DirectoryInfo db = Directory.CreateDirectory(BN_Dir);
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(BN_Dir));

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            
        }
    }
}
