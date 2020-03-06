using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Program
{
    static void Main(string[] args)
    {

        if (args.Length != 2)
        {
            Console.WriteLine("Usage: " + Environment.GetCommandLineArgs()[0] + " <ClientName> <CharacterName>");
            return;
        }
        else
        {
            string clientName = args[0];
            string character = args[1];

            GameMasterThalamusClient _thalamusClient = new GameMasterThalamusClient(clientName, character);
            _thalamusClient.SessionWithTablets();
            _thalamusClient.Dispose();

            Console.ReadLine();
            _thalamusClient.Dispose();
        }
    }
}
