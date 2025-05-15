using autoElytraETF;

class Program
{
    static string repoRoot = Directory.GetCurrentDirectory();

    static string sourcePath = Path.Combine(repoRoot, "assets", "minecraft", "optifine", "cit");
    
    static string destinationElytraPath = Path.Combine(repoRoot, "assets", "minecraft", "optifine", "random", "entity", "equipment", "wings");
    static List<CitElytra> elytras = new List<CitElytra>();
    static int id = 2;
    static void Main()
    {
        DeleteAllContents(destinationElytraPath);
        string filePath = Path.Combine(destinationElytraPath,"elytra.properties");
        try
        {
            ProcessElytraPropertyDirectories(sourcePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        List<CitElytra> newElytras = reOrderElytra();
        using (StreamWriter writer = new StreamWriter(filePath, append: true))
        {
            foreach (var elytra in newElytras)
            {
                if(elytra != null)
                {
                    string destinationPath = Path.Combine(destinationElytraPath, "elytra" + id + ".png");
                    string destinationEmissivePath = Path.Combine(destinationElytraPath, "elytra" + id + "_e.png");
                    string sourceEmissivePath = elytra.texturePath.Replace(".png", "_e.png");
                    try
                    {
                        File.Copy(sourceEmissivePath, destinationEmissivePath, overwrite: true);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                    string destinationPBREmissivePath = Path.Combine(destinationElytraPath, "elytra" + id + "_s.png");
                    string sourcePBREmissivePath = elytra.texturePath.Replace(".png", "_s.png");
                    try
                    {
                        File.Copy(sourcePBREmissivePath, destinationPBREmissivePath, overwrite: true);
                    }
                    catch (Exception ex)
                    {

                    }
                    File.Copy(elytra.texturePath, destinationPath, overwrite: true);
                    Console.WriteLine("Copied elytra: " + elytra.texturePath);
                    writer.Write(buildElytraString(elytra));
                    id++;
                }
            }
        }
    }

    static void ProcessElytraPropertyDirectories(string directory)
    {
        var jsonFiles = Directory.GetFiles(directory, "*.json");
        var propertiesFiles = Directory.GetFiles(directory, "*.properties");
        foreach (string subDir in Directory.GetDirectories(directory))
        {
            ProcessElytraPropertyDirectories(subDir);
        }
        foreach (var propertiesPath in propertiesFiles)
        {
            elytras.Add(ReadPropertiesFile(propertiesPath, directory));
        }

        
    }

    static CitElytra ReadPropertiesFile(string filePath, string directory)
    {
        string relativePath = Path.GetRelativePath(sourcePath, directory);
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            string type = string.Empty;
            string items = string.Empty;
            string texture = string.Empty;
            string displayName = string.Empty;
            string displayNameRaw = string.Empty;

            string textures = relativePath;
            int offset = 0;
            foreach (string line in lines)
            {
                if (line.Contains("type="))
                {
                    type = line.Substring("type=".Length - offset);
                    if (type != "elytra")
                    {
                        return null;
                    }
                }
                else if (line.Contains("items="))
                {
                    items = line.Substring("items=".Length - offset);
                }
                else if (line.Contains("texture="))
                {
                    texture = line.Substring("texture=".Length - offset) + ".png";
                }
                else if (line.Contains("nbt.display.Name=ipattern:"))
                {
                    displayName = line.Substring("nbt.display.Name=ipattern:".Length - offset).Replace("*", ".*");
                    displayNameRaw = line.Substring("nbt.display.Name=ipattern:".Length - offset).Replace("*", ".*");
                }
                else if (line.Contains("nbt.display.Name=pattern:"))
                {
                    displayName = line.Substring("nbt.display.Name=pattern:".Length - offset).Replace("*", ".*");
                    displayNameRaw = line.Substring("nbt.display.Name=pattern:".Length - offset).Replace("*", ".*");
                }
                else if (line.Contains("nbt.display.Name=iregex:"))
                {
                    displayName = line.Substring("nbt.display.Name=iregex:".Length - offset);
                    displayNameRaw = line.Substring("nbt.display.Name=iregex:".Length - offset);
                }
                else if (line.Contains("nbt.display.Name=regex:"))
                {
                    displayName = line.Substring("nbt.display.Name=regex:".Length - offset);
                    displayNameRaw = line.Substring("nbt.display.Name=regex:".Length - offset);
                }
                else if (line.Contains("nbt.display.Name="))
                {
                    displayNameRaw = line.Substring("nbt.display.Name=".Length - offset);
                    displayName = line.Substring("nbt.display.Name=".Length - offset);
                }
            }
            string texturePath = directory + "/" + texture;
            CitElytra citItem = new CitElytra(displayName, texturePath, displayNameRaw);
            return citItem;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading properties file: {ex.Message}");
            return null;
        }
    }

    public static string buildElytraString(CitElytra elytra)
    {
        string output = "\ntextures." + id + "=" + id + "\r\nnbt." + id + ".equipment.chest=raw:iregex:.*((" + elytra.displayName + ")|("+elytra.displayNameRaw +")).*\n";
        return output;
    }

    public static List<CitElytra> reOrderElytra()
    {
        List<CitElytra> newElytras = new List<CitElytra>();
        foreach(var elytra in elytras)
        {
            bool hasMatch = false;
            foreach (var newElytra in newElytras)
            {
                if (newElytra != null && elytra != null && newElytra.displayName.ToLower().Contains(elytra.displayName.ToLower()))
                {
                    hasMatch = true;
                }
            }
            if (!hasMatch)
            {
                newElytras.Insert(0,elytra);
            }
            else
            {
                newElytras.Add(elytra);
            }
        }
        return newElytras;
    }

    static void DeleteAllContents(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Directory does not exist: {folderPath}");
                return;
            }

            foreach (string file in Directory.GetFiles(folderPath))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            // Delete all subdirectories
            foreach (string dir in Directory.GetDirectories(folderPath))
            {
                Directory.Delete(dir, true);
            }

            Console.WriteLine($"All contents deleted from: {folderPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting contents from {folderPath}: {ex.Message}");
        }
    }
}