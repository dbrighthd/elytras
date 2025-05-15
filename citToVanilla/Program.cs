using Newtonsoft.Json.Linq;
using citToVanilla;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

class Program
{
    static string repoRoot = Directory.GetCurrentDirectory();
    static string sourcePath = Path.Combine(repoRoot, "assets", "minecraft", "optifine", "cit");

    static string destinationModelPath = Path.Combine(repoRoot, "assets", "minecraft", "models", "item");
    static List<CitItem> items = new List<CitItem>();
    static List<CitItem> items_broken = new List<CitItem>();
    static void Main()
    {
        string destinationTexturePath = Path.Combine(repoRoot, "assets", "minecraft", "textures", "item");
        string[] excludedTextureExtensions = { ".json", ".properties" };
        string[] excludedModelExtensions = { ".png", ".properties" };

        try
        {
            CopyDirectory(sourcePath, destinationTexturePath, excludedTextureExtensions);
            DeleteEmptyDirectories(destinationTexturePath);
            CopyDirectory(sourcePath, destinationModelPath, excludedModelExtensions);
            ProcessPropertyDirectories(sourcePath);
            ProcessJsonDirectories(destinationModelPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        JObject pumpkinsJson = CreateJson("elytra");
        items = reOrderElytra();
        foreach (var item in items)
        {
            if (item != null)
            {
                AddCase(pumpkinsJson, CapitalizeWords(item.displayName), item.displayNameRaw, "minecraft:item/" + item.modelPath.Replace("\\", "/"));

                if (item.displayName.ToLower().Contains("cape"))
                {
                    AddCase(pumpkinsJson, CapitalizeWords(item.displayName) + " Elytra", item.displayNameRaw + "*elytra*", "minecraft:item/" + item.modelPath.Replace("\\", "/"));
                }
                Console.WriteLine(CapitalizeWords(item.displayName));
                using (StreamWriter writer = new StreamWriter(item.absoluteJsonPath))
                {
                    writer.WriteLine(printElytraJson(item));
                }
                using (StreamWriter writer = new StreamWriter(item.absoluteJsonPath))
                {
                    writer.WriteLine(printElytraJson(item));
                }
                items_broken.RemoveAll(x => x == null);
                try
                {
                    using (StreamWriter writer = new StreamWriter(items_broken.First(x => x != null && x.displayName.ToLower() == item.displayName.ToLower()).absoluteJsonPath))
                    {
                        writer.WriteLine(printElytraJson(items_broken.First(x => x.displayName.ToLower() == item.displayName.ToLower())));
                    }
                }
                catch
                {

                }
            }
        }
        string outputPath = Path.Combine(repoRoot, "assets", "minecraft", "items", "elytra.json");
        string directoryPath = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        File.WriteAllText(outputPath, pumpkinsJson.ToString());
    }

    static void CopyDirectory(string sourceDir, string destinationDir, string[] excludedExtensions)
    {
        // Create destination directory if it doesn't exist
        if (!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        // Copy all files
        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string extension = Path.GetExtension(filePath);

            if (!excludedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destinationDir, fileName);
                File.Copy(filePath, destFilePath, true);
            }
        }

        // Copy all subdirectories recursively
        foreach (string dirPath in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(dirPath);
            string destDirPath = Path.Combine(destinationDir, dirName);
            CopyDirectory(dirPath, destDirPath, excludedExtensions);
        }

    }
    static void ProcessPropertyDirectories(string directory)
    {
        var jsonFiles = Directory.GetFiles(directory, "*.json");
        var propertiesFiles = Directory.GetFiles(directory, "*.properties");

        foreach (var propertiesPath in propertiesFiles)
        {
            items.Add(ReadPropertiesFile(propertiesPath, directory));
            items_broken.Add(ReadBrokenPropertiesFile(propertiesPath, directory));
        }

        foreach (string subDir in Directory.GetDirectories(directory))
        {
            ProcessPropertyDirectories(subDir);
        }
    }
    static void ProcessJsonDirectories(string directory)
    {
        var jsonFiles = Directory.GetFiles(directory, "*.json");

        foreach (var jsonPath in jsonFiles)
        {
            ReadAndModifyJson(jsonPath, directory);
        }

        foreach (string subDir in Directory.GetDirectories(directory))
        {
            ProcessJsonDirectories(subDir);
        }
    }
    static CitItem ReadPropertiesFile(string filePath, string directory)
    {
        string relativePath = Path.GetRelativePath(sourcePath, directory);
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            //Console.WriteLine($"Contents of {filePath}:");
            string type = string.Empty;
            string items = string.Empty;
            string model = string.Empty;
            string displayName = string.Empty;
            string displayNameRaw = string.Empty;


            string citTexture = string.Empty;
            int offset = 0;
            foreach (string line in lines)
            {
                if (line.Contains("type="))
                {
                    type = line.Substring("type=".Length - offset);
                    if (type != "item")
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
                    citTexture = line.Substring("texture=".Length - offset);
                }
                else if (line.Contains("texture.elytra="))
                {
                    citTexture = line.Substring("texture.elytra=".Length - offset);
                }
                else if (line.Contains("model="))
                {
                    model = line.Substring("model=".Length - offset);
                }
                else if (line.Contains("nbt.display.Name=ipattern:"))
                {
                    displayName = line.Substring("nbt.display.Name=ipattern:".Length - offset).Replace("*", "");
                }
                else if (line.Contains("nbt.display.Name=pattern:"))
                {
                    displayName = line.Substring("nbt.display.Name=pattern:".Length - offset).Replace("*", "");
                }
                else if (line.Contains("nbt.display.Name=iregex:"))
                {
                    displayName = findMostBasicRegex(line.Substring("nbt.display.Name=iregex:".Length - offset));
                }
                else if (line.Contains("nbt.display.Name=regex:"))
                {
                    displayName = findMostBasicRegex(line.Substring("nbt.display.Name=regex:".Length - offset));
                }
                else if (line.Contains("nbt.display.Name="))
                {
                    displayName = line.Substring("nbt.display.Name=".Length - offset);
                }
                else if (line.Contains("damage"))
                {
                    return null;
                }
                if (line.Contains("nbt.display.Name="))
                {
                    displayNameRaw = line.Substring("nbt.display.Name=".Length - offset);
                }
            }
            string modelPath = relativePath + "/" + "elytra";
            string absoluteJsonPath = Path.Combine(Path.Combine(destinationModelPath, relativePath), "elytra.json");
            string textures = Path.Combine(relativePath, citTexture);
            CitItem citItem = new CitItem(type, items, model, displayName, modelPath, textures, absoluteJsonPath, displayNameRaw);
            return citItem;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading properties file: {ex.Message}");
            return null;
        }
    }

    static CitItem ReadBrokenPropertiesFile(string filePath, string directory)
    {
        string relativePath = Path.GetRelativePath(sourcePath, directory);
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            //Console.WriteLine($"Contents of {filePath}:");
            string type = string.Empty;
            string items = string.Empty;
            string model = string.Empty;
            string displayName = string.Empty;
            string displayNameRaw = string.Empty;

            bool isBroken = false;
            string citTexture = string.Empty;
            int offset = 0;
            foreach (string line in lines)
            {
                if (line.Contains("type="))
                {
                    type = line.Substring("type=".Length - offset);
                    if (type != "item")
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
                    citTexture = line.Substring("texture=".Length - offset);
                }
                else if (line.Contains("texture.elytra="))
                {
                    citTexture = line.Substring("texture.elytra=".Length - offset);
                }
                else if (line.Contains("model="))
                {
                    model = line.Substring("model=".Length - offset);
                }
                else if (line.Contains("nbt.display.Name=ipattern:"))
                {
                    displayName = line.Substring("nbt.display.Name=ipattern:".Length - offset).Replace("*", "");
                }
                else if (line.Contains("nbt.display.Name=pattern:"))
                {
                    displayName = line.Substring("nbt.display.Name=pattern:".Length - offset).Replace("*", "");
                }
                else if (line.Contains("nbt.display.Name=iregex:"))
                {
                    displayName = findMostBasicRegex(line.Substring("nbt.display.Name=iregex:".Length - offset));
                }
                else if (line.Contains("nbt.display.Name=regex:"))
                {
                    displayName = findMostBasicRegex(line.Substring("nbt.display.Name=regex:".Length - offset));
                }
                else if (line.Contains("nbt.display.Name="))
                {
                    displayName = line.Substring("nbt.display.Name=".Length - offset);
                }
                else if (line.Contains("damage"))
                {
                    isBroken = true;
                }
                if (line.Contains("nbt.display.Name="))
                {
                    displayNameRaw = line.Substring("nbt.display.Name=".Length - offset);
                }
            }
            if (!isBroken)
            {
                return null;
            }
            string modelPath = relativePath + "/" + "elytra";
            string absoluteJsonPath = Path.Combine(Path.Combine(destinationModelPath, relativePath), "elytra_broken.json");
            string textures = Path.Combine(relativePath, citTexture);
            CitItem citItem = new CitItem(type, items, model, displayName, modelPath, textures, absoluteJsonPath, displayName);
            return citItem;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading properties file: {ex.Message}");
            return null;
        }
    }
    public static void ReadAndModifyJson(string filePath, string directory)
    {
        try
        {
            string relativePath = "item/" + Path.GetRelativePath(destinationModelPath, directory).Replace("\\", "/");
            string jsonContent = File.ReadAllText(filePath);
            var jsonObject = JObject.Parse(jsonContent);

            if (jsonObject["textures"] is JObject textures)
            {
                foreach (var property in textures.Properties())
                {
                    string value = property.Value.ToString();
                    if (value.StartsWith("./"))
                    {
                        value = value.Substring(2);
                    }
                    if (!value.StartsWith("block"))
                    {
                        property.Value = $"{relativePath}/{value}";
                    }
                }
            }

            File.WriteAllText(filePath, jsonObject.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    static JObject CreateJson(string model)
    {
        return new JObject
        {
            ["model"] = new JObject
            {
                ["type"] = "minecraft:select",
                ["property"] = "minecraft:component",
                ["component"] = "minecraft:custom_name",
                ["cases"] = new JArray(),
                ["fallback"] = new JObject
                {
                    ["type"] = "minecraft:condition",
                    ["property"] = "minecraft:broken",
                    ["on_false"] = new JObject
                    {
                        ["type"] = "minecraft:model",
                        ["model"] = "minecraft:item/" + model
                    },
                    ["on_true"] = new JObject
                    {
                        ["type"] = "minecraft:model",
                        ["model"] = "minecraft:item/" + model + "_broken"
                    }
                }
            }
        };
    }
    static void AddCase(JObject json, string name, string raw_name, string path)
    {
        JArray cases = (JArray)json["model"]["cases"];

        string lenient_name = "iregex:.*" + name + ".*";
        string lower_name = name.ToLower();
        bool caseExists = cases.Any(c =>
        {
            var when = c["when"] as JArray;
            return when != null &&
                   when.Contains(name);
        });

        if (caseExists)
        {
            //Console.WriteLine($"Case with name '{name}' already exists. Skipping.");
            return;
        }
        var whenNames = new JArray(new[] { name, lower_name, raw_name, lenient_name }.Distinct());
        JObject newCase = new JObject
        {
            ["when"] = whenNames,
            ["model"] = new JObject
            {
                ["type"] = "minecraft:condition",
                ["property"] = "minecraft:broken",
                ["on_false"] = new JObject
                {
                    ["type"] = "minecraft:model",
                    ["model"] = path
                },
                ["on_true"] = new JObject
                {
                    ["type"] = "minecraft:model",
                    ["model"] = path + "_broken"
                }
            }
        };

        ((JArray)json["model"]["cases"]).Add(newCase);
    }

    static string CapitalizeWords(string input)
    {
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower()).Replace("Dbrighthd", "dbrighthd").Replace("Tnt", "TNT").Replace("Minecon", "MineCon").Replace("Lgbtq", "LGBTQ").Replace("Vhs", "VHS").Replace("Oclock", "OClock").Replace("Mapmaker", "MapMaker").Replace("Dannybstyle", "dannybstyle").Replace("Followers", "Follower\'s").Replace("Usa", "USA");
    }
    static string findMostBasicRegex(string input)
    {
        return ExtractFirstTerm(input).Replace(".*", "").Replace("(", "").Replace(")", "").Replace("?", "").Replace("batmanelytra", "batman cape elytra").Replace("missing texture", "missing texture elytra");
    }
    static string ExtractFirstTerm(string input)
    {
        int lastIndex = input.LastIndexOf(")");
        string thePartThatSwiftMadeMeCheckForWithHisHats = string.Empty;
        if (lastIndex != -1)
        {
            thePartThatSwiftMadeMeCheckForWithHisHats = input.Substring(lastIndex + 1);
        }
        var pattern = @"\(([^()]*\([^()]*\)[^()]*)\|";
        var match = Regex.Match(input, pattern);

        if (match.Success)
        {
            return ExtractTerm(match.Groups[1].Value);
        }
        return input.Split('|')[0] + thePartThatSwiftMadeMeCheckForWithHisHats;
    }

    static string ExtractTerm(string input)
    {
        var match = Regex.Match(input, @"\(([^|]+)\|");

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return input.Split('|')[0];
    }

    static void DeleteEmptyDirectories(string path)
    {
        foreach (var dir in Directory.GetDirectories(path))
        {
            DeleteEmptyDirectories(dir);
        }
        try
        {
            if (Directory.GetFileSystemEntries(path).Length == 0)
            {
                Directory.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory {path}: {ex.Message}");
        }
    }

    static string printElytraJson(CitItem citItem)
    {
        return "{\r\n  \"parent\": \"minecraft:item/generated\",\r\n  \"textures\": {\r\n    \"layer0\": \"minecraft:item/" + citItem.texturePath.Replace("\\", "/") + "\"\r\n  }\r\n}";
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
    public static List<CitItem> reOrderElytra()
    {
        List<CitItem> newElytras = new List<CitItem>();
        foreach(var elytra in items)
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
}