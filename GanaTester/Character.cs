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
        public int mode2correct;
        public bool bToBeTested;
        public bool isHiragana;
        public bool isActive;
        public int strokeCount;
        public int GroupID;
        public Character()
        {
        }
        public Character(string _Gana, string _Romaji,bool _isHirgana)
        {
            Romaji = _Romaji;
            Gana = _Gana;
            correct = 0;
            TestTime = DateTime.Now;
            bToBeTested = false;
            isHiragana = _isHirgana;
        }
        public override string ToString()
        {
            return Gana;
        }
        public bool Check(int mode,string Character,bool practice = false)
        {
            // mode = 1 romaji -> gana/kana
            if(mode == 1)
            {
                if(Character.ToLower() == Gana)
                {
                    if(!practice)
                    {
                        correct++;
                    }
                    return true;
                }
            }
            // mode = 2 romaji <- gana/kana
            if (mode == 2)
            {
                if (Character.ToLower() == Romaji)
                {
                    if (!practice)
                    {
                        mode2correct++;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
