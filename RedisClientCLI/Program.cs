/*
 * MIT License
 *
 * Copyright (c) 2025 Brent Ferree
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * -----------------------------------------------------------------------------------
 * This file was generated with the assistance of AI and refined by Brent Ferree.
 * -----------------------------------------------------------------------------------
 */

using System.Text;
using Azure.Identity;
using RedisClientCLI;
using StackExchange.Redis;

internal class Program
{
    private static ConnectionMultiplexer? _redis;
    private static IDatabase? _db;
    private static readonly string historyFilePath = Path.Combine(Path.GetTempPath(), "redis_cli_history.txt");
    private static readonly string redisHostFilePath = Path.Combine(Path.GetTempPath(), "redis_cli_last_host.txt");
    private static List<string> commandHistory = new();
    private static int historyIndex = -1;
    private static string redisHost = "";
    private static string redisHostTerminalPromptName = "";

    private static bool IsConnectionString(string input) => input.Contains("=") && input.Contains(";");
    
    private static void LoadLastUsedRedisHost() => redisHost = FileHelper.ReadFile(redisHostFilePath);
    
    private static void SaveLastUsedRedisHost() => FileHelper.WriteFile(redisHostFilePath, redisHost);

    private static async Task LoadLastUsedRedisHostAsync() =>
        redisHost = File.Exists(redisHostFilePath) ? await File.ReadAllTextAsync(redisHostFilePath) : string.Empty;

    
    
    private static async Task SaveLastUsedRedisHostAsync() =>
        await File.WriteAllTextAsync(redisHostFilePath, redisHost);
    
    
    private static async Task Main()
    {
        Console.WriteLine("Azure Redis CLI with Entra ID Authentication Support");
        LoadLastUsedRedisHost();

        Console.Write($"Enter Redis Hostname [{redisHost}]: ");
        var inputHost = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(inputHost))
        {
            redisHost = inputHost;
            SaveLastUsedRedisHost();
        }

        if (string.IsNullOrWhiteSpace(redisHost))
        {
            Console.WriteLine("Invalid hostname. Exiting.");
            return;
        }

        LoadCommandHistory();

        try
        {
            await ConnectToRedis();
            await CommandLoop();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error occurred in Redis Command Execution: Exception: {ex}");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task ConnectToRedis()
    {
        ConfigurationOptions configurationOptions;

        if (redisHost.Contains("@")) // Assuming this means it's a full connection string
        {
            Console.WriteLine("Connecting to Redis using connection string...");
            configurationOptions = ConfigurationOptions.Parse(redisHost);
            redisHostTerminalPromptName = configurationOptions.EndPoints[0].ToString();
        }
        else
        {
            Console.WriteLine("Connecting to Redis and Authenticating with Azure Entra ID interactively...");
            var credential = new InteractiveBrowserCredential();
            configurationOptions = await ConfigurationOptions
                .Parse(redisHost)
                .ConfigureForAzureWithTokenCredentialAsync(credential)
                .ConfigureAwait(false);
            redisHostTerminalPromptName = redisHost;
        }

        // Common Redis configurations
        configurationOptions.ConnectTimeout = 15000;
        configurationOptions.SyncTimeout = 15000;
        configurationOptions.AbortOnConnectFail = false;
        configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);

        _redis = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
        _db = _redis.GetDatabase();

        Console.WriteLine($"Connected to Redis: {redisHostTerminalPromptName}");
    }

    private static async Task CommandLoop()
    {
        Console.WriteLine("Enter Redis commands (type 'quit' to exit):");
        while (true)
        {
            Console.Write($"[{redisHostTerminalPromptName}]> ");
            var command = ReadCommandWithHistory();
            if (string.IsNullOrWhiteSpace(command)) continue;

            if (command.Trim().ToLower() == "quit")
            {
                SaveCommandHistory();
                break;
            }

            commandHistory.Add(command);
            historyIndex = commandHistory.Count;

            var commandParts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length == 0) continue;

            try
            {
                var cmd = commandParts[0].ToUpper();
                string[] args = commandParts.Skip(1).ToArray();

                if (cmd == "SCAN")
                {
                    var count = args.Length > 0 && int.TryParse(args[0], out var parsedCount) ? parsedCount : 10;
                    await ScanAndPrintKeys(count);
                }
                else if (cmd == "GET")
                {
                    if (args.Length == 1)
                        await GetAndPrintValue(args[0]);
                    else
                        Console.WriteLine("Usage: GET <key>");
                }
                else
                {
                    var result = await _db.ExecuteAsync(cmd, args);
                    FormatAndPrintResult(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        Console.WriteLine("Exiting Redis CLI.");
    }


    private static void LoadCommandHistory()
    {
        if (File.Exists(historyFilePath))
        {
            commandHistory = new List<string>(File.ReadAllLines(historyFilePath));
            historyIndex = commandHistory.Count;
        }
    }

    private static void SaveCommandHistory()
    {
        File.WriteAllLines(historyFilePath, commandHistory);
    }
private static string ReadCommandWithHistory()
{
    StringBuilder input = new();
    int cursorPosition = 0;
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.UpArrow && historyIndex > 0)
        {
            historyIndex--;
            input.Clear().Append(commandHistory[historyIndex]);
            cursorPosition = input.Length;
            RedrawInputLine(input.ToString(), cursorPosition);
        }
        else if (key.Key == ConsoleKey.DownArrow && historyIndex < commandHistory.Count - 1)
        {
            historyIndex++;
            input.Clear().Append(commandHistory[historyIndex]);
            cursorPosition = input.Length;
            RedrawInputLine(input.ToString(), cursorPosition);
        }
        else if (key.Key == ConsoleKey.LeftArrow && key.Modifiers.HasFlag(ConsoleModifiers.Alt)) // ALT + Left
        {
            cursorPosition = MoveToPreviousWord(input.ToString(), cursorPosition);
            RedrawInputLine(input.ToString(), cursorPosition);
        }
        else if (key.Key == ConsoleKey.RightArrow && key.Modifiers.HasFlag(ConsoleModifiers.Alt)) // ALT + Right
        {
            cursorPosition = MoveToNextWord(input.ToString(), cursorPosition);
            RedrawInputLine(input.ToString(), cursorPosition);
        }
        else if (key.Key == ConsoleKey.LeftArrow && cursorPosition > 0)
        {
            cursorPosition--;
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
        else if (key.Key == ConsoleKey.RightArrow && cursorPosition < input.Length)
        {
            cursorPosition++;
            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
        }
        else if (key.Key == ConsoleKey.Backspace && cursorPosition > 0)
        {
            input.Remove(cursorPosition - 1, 1);
            cursorPosition--;
            RedrawInputLine(input.ToString(), cursorPosition);
        }
        else if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            input.Insert(cursorPosition, key.KeyChar);
            cursorPosition++;
            RedrawInputLine(input.ToString(), cursorPosition);
        }
    } while (true);

    return input.ToString().Trim();
}

private static int MoveToPreviousWord(string text, int cursorPosition)
{
    if (cursorPosition == 0) return 0;

    // Skip any spaces before the word
    while (cursorPosition > 0 && char.IsWhiteSpace(text[cursorPosition - 1]))
    {
        cursorPosition--;
    }

    // Skip the word itself
    while (cursorPosition > 0 && !char.IsWhiteSpace(text[cursorPosition - 1]))
    {
        cursorPosition--;
    }

    return cursorPosition;
}

private static int MoveToNextWord(string text, int cursorPosition)
{
    if (cursorPosition >= text.Length) return text.Length;

    // Skip the current word if in the middle
    while (cursorPosition < text.Length && !char.IsWhiteSpace(text[cursorPosition]))
    {
        cursorPosition++;
    }

    // Skip any spaces after the word
    while (cursorPosition < text.Length && char.IsWhiteSpace(text[cursorPosition]))
    {
        cursorPosition++;
    }

    return cursorPosition;
}

private static void RedrawInputLine(string text, int cursorPosition)
{
    Console.SetCursorPosition(0, Console.CursorTop);
    Console.Write(new string(' ', Console.WindowWidth - 1)); // Clear line properly
    Console.SetCursorPosition(0, Console.CursorTop);
    Console.Write($"[{redisHostTerminalPromptName}]> {text}");
    Console.SetCursorPosition($"[{redisHostTerminalPromptName}]> ".Length + cursorPosition, Console.CursorTop);
}


    private static async Task ScanAndPrintKeys(int count)
    {
        var scanResult = await _db.ExecuteAsync("SCAN", "0", "MATCH", "*", "COUNT", count.ToString());
        var scanArray = (RedisResult[])scanResult;
        Console.WriteLine($"Cursor: {scanArray[0]}");
        Console.WriteLine($"Keys Found ({count} max):");
        foreach (var key in (RedisResult[])scanArray[1]) Console.WriteLine($" - {key}");
    }

    private static async Task GetAndPrintValue(string key)
    {
        var typeResult = await _db.ExecuteAsync("TYPE", key);
        var keyType = typeResult.ToString();

        switch (keyType)
        {
            case "none":
            case "string":
                var value = await _db.StringGetAsync(key);
                FormatAndPrintValue(value);
                break;

            case "hash":
                var hashEntries = await _db.HashGetAllAsync(key);
                Console.WriteLine($"Hash ({hashEntries.Length} fields):");
                foreach (var entry in hashEntries) Console.WriteLine($"  {entry.Name}: {entry.Value}");

                break;

            case "list":
                var listItems = await _db.ListRangeAsync(key);
                Console.WriteLine($"List ({listItems.Length} items):");
                for (var i = 0; i < listItems.Length; i++) Console.WriteLine($"  [{i}] {listItems[i]}");

                break;

            case "set":
                var setItems = await _db.SetMembersAsync(key);
                Console.WriteLine($"Set ({setItems.Length} members):");
                foreach (var member in setItems) Console.WriteLine($"  - {member}");

                break;

            case "zset":
                var sortedSetItems = await _db.SortedSetRangeByRankWithScoresAsync(key);
                Console.WriteLine($"Sorted Set ({sortedSetItems.Length} elements):");
                foreach (var entry in sortedSetItems) Console.WriteLine($"  {entry.Element} (Score: {entry.Score})");

                break;

            default:
                Console.WriteLine($"Unsupported type: {keyType}");
                break;
        }
    }

    private static void FormatAndPrintValue(RedisValue value)
    {
        if (!value.HasValue)
            Console.WriteLine("(nil)");
        else
            Console.WriteLine(value.ToString());
    }


    private static void FormatAndPrintResult(RedisResult result)
    {
        if (result.IsNull)
        {
            Console.WriteLine("(nil)");
            return;
        }

        if (result.Type == ResultType.Integer)
        {
            Console.WriteLine(result);
        }
        else if (result.Type == ResultType.BulkString || result.Type == ResultType.SimpleString)
        {
            Console.WriteLine(result.ToString());
        }
        else if (result.Type == ResultType.MultiBulk)
        {
            var items = (RedisResult[])result;
            Console.WriteLine($"{items.Length} element(s):");
            for (var i = 0; i < items.Length; i++) Console.WriteLine($"[{i + 1}] {items[i]}");
        }
        else
        {
            Console.WriteLine(result.ToString());
        }
    }
}