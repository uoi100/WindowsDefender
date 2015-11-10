using System;

namespace WDClient
{
    [Serializable()]
    public class Instruction
    {
        public enum Type { JOIN, LEAVE, CMD, JOINED };
        public Type Command { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
        public string Arg3 { get; set; }
        public string Arg4 { get; set; }
    }
}
