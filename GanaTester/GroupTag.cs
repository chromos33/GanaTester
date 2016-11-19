using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GanaTester
{
    public class GroupTag
    {
        public int iGroupID;
        public char cGroup;
        public GroupTag(int id, char group)
        {
            iGroupID = id;
            cGroup = group;
        }
        public GroupTag()
        {
        }
    }
}
