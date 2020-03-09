using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class TabletPlayer
{

    public string ID;
    public string Name;
    public bool Connected;

    public TabletPlayer(string _id, string _name)
    {
        this.ID = _id;
        this.Name = _name;
        this.Connected = true;
    }
}

