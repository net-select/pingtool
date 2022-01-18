using URLTester;


Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.White;



string[] cArgs = Environment.GetCommandLineArgs();
bool showHelp = (cArgs.Length < 2 || (cArgs.Length == 2 && string.IsNullOrEmpty(cArgs[1])));
bool foundAParam = false;
List<string> addresses = new List<string>();
int iterations = 5;
int speed = 1000;


for (int i = 1; i < cArgs.Length; i++)
{
    if (cArgs[i].StartsWith('-') || cArgs[i].StartsWith('/'))
    {
        foundAParam = true;

        if (cArgs[i].Substring(1).StartsWith('-'))
        {
            cArgs[i] = cArgs[i].Substring(1);
        }


        if (cArgs[i].Substring(1).StartsWith('h') || cArgs[i].Substring(1).StartsWith('?'))
        {
            showHelp = true;
            break;
        }

        if (cArgs[i].Substring(1).StartsWith('i') && cArgs.Length > i + 1)
        {
            iterations = int.Parse(cArgs[i + 1]);
        }

        if (cArgs[i].Substring(1).StartsWith('t'))
        {
            iterations = int.MaxValue;
        }

        if (cArgs[i].Substring(1).StartsWith('s') && cArgs.Length > i + 1)
        {
            speed = int.Parse(cArgs[i + 1]);
        }

        if (cArgs[i].Substring(1).StartsWith('f') && cArgs.Length > i + 1)
        {
            string fName = cArgs[i + 1].Trim();
            if (System.IO.File.Exists(fName))
            {
                addresses = System.IO.File.ReadAllLines(fName).ToList();
            }
        }


    } else if (!foundAParam)
    {
        addresses.Add(cArgs[i]);
    }
}




if (showHelp)
{
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.WriteLine("    _________________");
    Console.WriteLine("  /                   \\");
    Console.Write(" /  ");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.Write("+--    ---    |    ");
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.WriteLine("\\");
    Console.Write("/   ");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.Write("|  |  +---+  -+-    ");
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.WriteLine("\\");
    Console.Write("\\   ");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.Write("|  |   __     \\__   ");
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.WriteLine("/");
    Console.WriteLine(" \\                       __    ___   |   ___   |   ___    |");
    Console.WriteLine("  \\__________   ______  #__   #---#  |  #---#  |  |      -+-");
    Console.WriteLine("                         __#   __    |   __    |  '___    \\__");
    Console.WriteLine("");
    Console.WriteLine("");


    Console.WriteLine("==============================================================");
    Console.WriteLine("");

    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("            P i n g T o o l        by net-select");
    Console.WriteLine("");
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.WriteLine("==============================================================");
    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine("");


    Console.WriteLine("How to use:");
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.WriteLine("\tpingtool [?address] [?address] [...] [parameters]");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine();
    Console.WriteLine("Address can be ip address or fqdn.");
    Console.WriteLine("You can enter a single address or multiple ones with whitespaces between each other.");
    Console.WriteLine("As soon as you enter a parameter, no more addresses will be parsed.");
    Console.WriteLine("Parameters:");
    Console.WriteLine("-h\t\tshows this help");
    Console.WriteLine("-i [value]\tdefines the ping iterations (default: 5)");
    Console.WriteLine("-t\t\tmakes this ping loop to infinity and beyond (default: off)");
    Console.WriteLine("-s [value]\tdefines the ping interval (speed) in ms (default: 1000)");
    Console.WriteLine("-f [path]\tdefines a file to read addresses from (default: off)");
    Console.WriteLine("\t\t\tThe file must include one address per line");
    Console.WriteLine("\t\t\tPossible addresses entered are ignored if there is a file");

    Console.WriteLine();
    Console.WriteLine("Press any key to exit.");

    Console.ReadKey();

    return;
}





bool redo = true;




while (redo)
{
    ThreadedPing.PingAddresses(addresses.ToArray(), iterations, speed);

    while (ThreadedPing.PingRunning)
    {
        Thread.Sleep(speed);
    }

    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine("Finished. Press 'y' for restarting or any key to exit.");

    ConsoleKeyInfo answer = Console.ReadKey();
    if (answer.KeyChar == 'y')
    {
        Console.WriteLine("es, I want to restart");
        redo = true;
    } else
    {
        redo = false;
    }
}