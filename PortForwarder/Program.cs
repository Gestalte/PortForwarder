using NATTraversal;

namespace PortForwarder
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            MonoTraverser traverser = new();

            while (true)
            {
                int port = 0;
                string mappingName = "";
                bool enumerateMaps = false;

                if (args.Length == 0)
                {
                    Console.WriteLine("Enter \"e\" to enumerate forwarded ports. Enter \"port number\", \"mapping name\"");

                    var input = Console.ReadLine();

                    if (input == "e")
                    {
                        enumerateMaps = true;
                    }
                    else
                    {
                        if (input == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("");
                            Console.WriteLine("Incorrect input.");
                            Console.WriteLine("");
                            Console.ResetColor();
                            continue;
                        }

                        var splits = input.Split(",");

                        if (splits.Length == 2 && int.TryParse(splits[0], out port))
                        {
                            mappingName = splits[1];
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("");
                            Console.WriteLine("Incorrect input.");
                            Console.WriteLine("");
                            Console.ResetColor();
                            continue;
                        }
                    }
                }
                else if (args.Length == 1 && args[0] == "e")
                {
                    enumerateMaps = true;
                }
                else if (args.Length == 2 && int.TryParse(args[0], out port))
                {
                    mappingName = args[1];
                }

                if (enumerateMaps)
                {
                    string[] s = traverser.TryGetForwardedPorts();
                    Console.WriteLine("");
                    foreach (var item in s)
                    {
                        Console.WriteLine(item);
                    }
                    Console.WriteLine("");
                    continue;
                }

                Console.WriteLine("");

                if (traverser.TryForwardPort(port, mappingName))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Port {port} forwarded successfully.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Port {port} not forwarded successfully.");
                }

                Console.ResetColor();
                Console.WriteLine("");
            }
        }
    }
}