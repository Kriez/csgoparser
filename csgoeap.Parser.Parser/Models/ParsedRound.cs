using System;
using System.Collections.Generic;
using System.Text;

namespace csgoeap.Parser.Parser.Models
{
    public class ParsedRound
    {
        public int RoundId { get; set; }
        public ParsedPlayer[] Players { get; set; }
    }
}
