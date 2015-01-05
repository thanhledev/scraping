using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes.Enums
{
    public sealed class SystemMessage
    {
        private readonly string value;
        private readonly int key;

        public static readonly SystemMessage CloseWarning = new SystemMessage(1, "Any unsaved values will be lost! Proceed?");
        public static readonly SystemMessage Warning = new SystemMessage(2, "Are you sure?");

        private SystemMessage(int key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
