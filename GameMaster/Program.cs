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
        _thalamusClient.SessionWithTablets();
        _thalamusClient.Dispose();
    }
}
