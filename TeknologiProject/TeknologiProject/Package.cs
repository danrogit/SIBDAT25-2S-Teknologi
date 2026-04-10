using System;
using System.Collections.Generic;
using System.Text;

namespace TeknologiProject
{
    public class Package
    {
        public Sender? Sender;
        public Receiver? Receiver;
        public PackageSize? Size;

        public Package(Sender sender, Receiver receiver, PackageSize size) 
        {
            Sender = sender;
            Receiver = receiver;
            Size = size;
        }

        public Package()
        {

        }

    }

    public enum PackageSize
    {
        Small,
        Medium,
        Large
    }
}
