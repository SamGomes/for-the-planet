using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class TabletPlayer
{

    public int ID;
    public string Name;
    public bool Connected;
    public List<int> EnvInvestments;

    public TabletPlayer(int _id)
    {
        this.ID = _id;
        this.Connected = false;
        this.EnvInvestments = new List<int>();
    }
}

