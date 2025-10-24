using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UsernameGenerator
{
    private static readonly Random random = new Random();

    // 网名前缀（形容词、名词等）
    private static readonly string[] prefixes = {
        "Dark", "Shadow", "Cyber", "Neon", "Electric", "Quantum", "Steel", "Iron",
        "Silver", "Golden", "Crystal", "Mystic", "Ancient", "Epic", "Legendary",
        "Ghost", "Phantom", "Stealth", "Silent", "Rapid", "Swift", "Furious",
        "Cosmic", "Galactic", "Solar", "Lunar", "Stellar", "Atomic", "Nuclear",
        "Digital", "Virtual", "Cyber", "Techno", "Meta", "Hyper", "Ultra", "Mega"
    };

    // 网名后缀
    private static readonly string[] suffixes = {
        "Slayer", "Hunter", "Warrior", "Knight", "Master", "Lord", "King", "Queen",
        "Dragon", "Phoenix", "Wolf", "Tiger", "Eagle", "Hawk", "Falcon", "Shark",
        "Blade", "Sword", "Axe", "Arrow", "Bullet", "Shot", "Strike", "Impact",
        "Walker", "Rider", "Runner", "Jumper", "Dancer", "Singer", "Player",
        "Ninja", "Samurai", "Viking", "Gladiator", "Spartan", "Assassin",
        "Wizard", "Mage", "Sorcerer", "Warlock", "Necromancer", "Cleric",
        "Reaper", "Destroyer", "Annihilator", "Dominator", "Conqueror"
    };

    // 游戏相关词汇
    private static readonly string[] gamingTerms = {
        "Pro", "Noob", "Elite", "Veteran", "Newbie", "Gamer", "Player", "Streamer",
        "God", "Demon", "Angel", "Titan", "Giant", "Colossus", "Beast", "Monster",
        "Bot", "AI", "CPU", "GPU", "RAM", "Byte", "Bit", "Pixel", "Vector",
        "Lag", "Ping", "FPS", "HD", "4K", "VR", "AR", "XP", "HP", "MP"
    };

    // 动物名称
    private static readonly string[] animals = {
        "Fox", "Wolf", "Bear", "Lion", "Tiger", "Eagle", "Hawk", "Owl", "Raven",
        "Snake", "Dragon", "Phoenix", "Griffin", "Unicorn", "Panther", "Leopard",
        "Falcon", "Shark", "Orca", "Rhino", "Elephant", "Gorilla", "Monkey"
    };

    // 自然元素
    private static readonly string[] elements = {
        "Fire", "Ice", "Water", "Earth", "Air", "Wind", "Storm", "Thunder",
        "Lightning", "Flame", "Frost", "Blizzard", "Tsunami", "Earthquake",
        "Volcano", "Tornado", "Hurricane", "Typhoon", "Avalanche"
    };

    // 搞笑/创意网名
    private static readonly string[] funnyUsernames = {
        "BananaHammock", "TacoTuesday", "PizzaTheHut", "SirLaughsALot",
        "CaptainObvious", "Error404", "InfiniteLoop", "NullPointer",
        "StackOverflow", "SyntaxError", "DivideByZero", "BlueScreen",
        "CtrlAltDefeat", "NoWiFiNoLife", "Loading...", "Buffering",
        "LowBattery", "OutOfMemory", "RAMenNoodles", "JavaTheHutt"
    };

    /// <summary>
    /// 生成随机英文网名
    /// </summary>
    /// <param name="style">网名风格：0-经典，1-游戏，2-科技，3-自然，4-动物</param>
    /// <param name="includeNumbers">是否包含数字</param>
    /// <param name="includeSpecialChars">是否包含特殊字符</param>
    /// <returns>随机英文网名</returns>
    public static string GenerateUsername(int style = 0, bool includeNumbers = false, bool includeSpecialChars = false)
    {
        string username = style switch
        {
            1 => GenerateGamingUsername(),    // 游戏风格
            2 => GenerateTechUsername(),      // 科技风格
            3 => GenerateNatureUsername(),    // 自然风格
            4 => GenerateAnimalUsername(),    // 动物风格
            5 => funnyUsernames[random.Next(funnyUsernames.Length)], // 搞笑风格
            _ => GenerateClassicUsername()    // 经典风格
        };

        // 添加数字
        if (includeNumbers && random.Next(3) > 0) // 66% 几率添加数字
        {
            username += random.Next(1000).ToString();
        }

        // 添加特殊字符
        if (includeSpecialChars && random.Next(4) == 0) // 25% 几率添加特殊字符
        {
            username = AddSpecialCharacters(username);
        }

        return username;
    }

    private static string GenerateClassicUsername()
    {
        // 经典风格：前缀 + 后缀
        string prefix = prefixes[random.Next(prefixes.Length)];
        string suffix = suffixes[random.Next(suffixes.Length)];

        // 随机决定是否使用下划线连接
        if (random.Next(2) == 0)
        {
            return prefix + "_" + suffix;
        }
        return prefix + suffix;
    }

    private static string GenerateGamingUsername()
    {
        string[] patterns = {
            $"{prefixes[random.Next(prefixes.Length)]}{gamingTerms[random.Next(gamingTerms.Length)]}",
            $"{animals[random.Next(animals.Length)]}{gamingTerms[random.Next(gamingTerms.Length)]}",
            $"{gamingTerms[random.Next(gamingTerms.Length)]}{suffixes[random.Next(suffixes.Length)]}",
            $"The{gamingTerms[random.Next(gamingTerms.Length)]}"
        };

        return patterns[random.Next(patterns.Length)];
    }

    private static string GenerateTechUsername()
    {
        string[] techPrefixes = { "Cyber", "Digital", "Virtual", "Techno", "Meta", "Neo", "Quantum" };
        string[] techSuffixes = { "Byte", "Bit", "Code", "Data", "Core", "Drive", "Matrix", "Grid" };

        return techPrefixes[random.Next(techPrefixes.Length)] + techSuffixes[random.Next(techSuffixes.Length)];
    }

    private static string GenerateNatureUsername()
    {
        string[] naturePatterns = {
            $"{elements[random.Next(elements.Length)]}{suffixes[random.Next(suffixes.Length)]}",
            $"{elements[random.Next(elements.Length)]}{elements[random.Next(elements.Length)]}",
            $"The{elements[random.Next(elements.Length)]}",
            $"{prefixes[random.Next(prefixes.Length)]}{elements[random.Next(elements.Length)]}"
        };

        return naturePatterns[random.Next(naturePatterns.Length)];
    }

    private static string GenerateAnimalUsername()
    {
        string[] animalPatterns = {
            $"{prefixes[random.Next(prefixes.Length)]}{animals[random.Next(animals.Length)]}",
            $"{animals[random.Next(animals.Length)]}{suffixes[random.Next(suffixes.Length)]}",
            $"The{animals[random.Next(animals.Length)]}",
            $"{animals[random.Next(animals.Length)]}Of{elements[random.Next(elements.Length)]}"
        };

        return animalPatterns[random.Next(animalPatterns.Length)];
    }

    private static string AddSpecialCharacters(string username)
    {
        char[] specialChars = { '_', '-', '.', 'x', 'X', '0', '1' };
        char specialChar = specialChars[random.Next(specialChars.Length)];

        // 随机位置添加特殊字符
        int position = random.Next(username.Length);
        return username.Insert(position, specialChar.ToString());
    }
}