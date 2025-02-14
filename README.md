# Redis Command Line Interface (CLI)

## Overview
This project provides an enhanced **console input reader** for connecting to Redis with Entra ID authenticaiton support. 

The implementation includes:

- **Entra ID Authentication** login using just your azure FQDN redis host name.
- **Connection String** is also accepted alternatively, this need more testing.
- **Command history navigation** using Up/Down arrows.
- **Word-by-word navigation** using `CMD + Left/Right Arrow`, similar to Linux shell prompts.
- **Backspace handling** for in-line deletion.
- **Real-time input display** with cursor repositioning.

## Features
- **Navigate command history** (`Up/Down Arrow`)
- **Move cursor character-by-character** (`Left/Right Arrow`)
- **Move cursor word-by-word** (`ALT + Left/Right Arrow`)
- **Delete characters with Backspace**
- **Supports dynamic command editing**

## Usage
To integrate this feature into your C# console application:

1. Copy `ReadCommandWithHistory()` into your project.
2. Ensure you have a `commandHistory` list to store previous commands.
3. Call `ReadCommandWithHistory()` to get user input dynamically.

### Example Usage
```csharp
string command = ReadCommandWithHistory();
Console.WriteLine("You entered: " + command);
```

## Requirements
- .NET 6 or later
- Console application
- Windows/macOS/Linux compatibility

## Installation
1. Clone the repository:
   ```sh
   git clone [https://github.com/yourusername/console-input-enhanced.git](https://github.com/sirforce/RedisClientCLI.git)
   ```
2. Open the project in Visual Studio or any C# IDE.
3. Run the console application.

## License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2025 [Your Name]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## Contribution
Feel free to submit **pull requests** or **report issues** on GitHub. Contributions are welcome!

## Contact
For questions or suggestions, reach out to **[Sir Force]** at [26939529+sirforce@users.noreply.github.com].

