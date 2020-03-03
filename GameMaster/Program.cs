using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Program
{
    static void Main(string[] args)
    {
        GameMasterThalamusClient _thalamusClient = new GameMasterThalamusClient();
        bool stop = false;
        while (!stop)
        {
            string input = Console.ReadLine();

            if (input == "z")
            {
                _thalamusClient.GameMaster.ReceiveZ();
            }
            else if (input == "q")
            {
                stop = true;
            }
        }
        _thalamusClient.Dispose();
    }
}
