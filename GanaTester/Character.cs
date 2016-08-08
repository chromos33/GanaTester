using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GanaTester
{
    public class Character
    {
        public string Romaji;
        public string Gana;
        public DateTime TestTime;
        public int correct;
        public bool bToBeTested;
        public bool isHiragana;
        public bool isActive;
        public Character()
        {
        }
        public Character(string _Gana, string _Romaji,bool _isHirgana)
        {
            Romaji = _Romaji;
            Gana = _Gana;
            correct = 0;
            TestTime = DateTime.Now;
            bToBeTested = true;
            isHiragana = _isHirgana;
        }
        public override string ToString()
        {
            return Gana;
        }
    }
}
