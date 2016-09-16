using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VersionUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            var location = args[0];
            bool debug;
            if (!bool.TryParse(args[1], out debug))
                return;
            if (!Directory.Exists(location))
                return;
            //Task.Run(() => ReplaceVersion(debug));
            ReplaceVersion(debug, location);
        }

        public static void ReplaceVersion(bool debug, string location)
        {
            try
            {
                var assemblyFiles = Directory.GetFiles(location).Where(x => x.Contains("AssemblyInfo.cs")).ToList();
                foreach (var file in assemblyFiles)
                {
                    int major = 1;
                    int minor = 0;
                    int build = 0;
                    int revision = 0;
                    string newBuild = "";
                    string oldValue = "";
                    using (StreamReader reader = new StreamReader(file))
                    {
                        while ((oldValue = reader.ReadLine()) != null)
                        {
                            if (!oldValue.ToLower().Contains("assemblyversion(") || oldValue.Contains(@"//"))
                                continue;
                            if (oldValue.ToLower().Contains("[assembly: assemblyversion(\""))
                                oldValue = oldValue.ToLower().Replace("[assembly: assemblyversion(\"", "");
                            if (oldValue.Contains("\")]"))
                                oldValue = oldValue.Replace("\")]", "");
                            var versions = oldValue.Split('.');
                            if (versions.Length < 4)
                                continue;
                            major = Convert.ToInt32(versions[0]);
                            minor = Convert.ToInt32(versions[1]);
                            build = debug ? Convert.ToInt32(versions[2]) : Convert.ToInt32(versions[2]) + 1;
                            revision = debug ? Convert.ToInt32(versions[3]) + 1 : Convert.ToInt32(versions[3]);
                            newBuild = $"{major}.{minor}.{build}.{revision}";
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(newBuild))
                        return;
                    var fileContents = File.ReadAllText(file);
                    fileContents = fileContents.Replace("[assembly: AssemblyFileVersion(\"1.0.0.0\")]", "");
                    fileContents = fileContents.Replace(oldValue, newBuild);
                    File.WriteAllText(file, fileContents);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
