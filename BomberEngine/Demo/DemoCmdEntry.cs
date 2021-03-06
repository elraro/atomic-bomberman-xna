﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BomberEngine.Core.IO;

namespace BomberEngine.Demo
{
    public class DemoCmdEntry : CommandLineEntry
    {
        public String m_fileName;

        public DemoCmdEntry()
            : base("d", "demo")
        {
        }

        public override void Parse(ArgsIterator iter)
        {
            if (!iter.HasNext())
            {
                throw new CommandLineException("Filename expected");
            }

            m_fileName = iter.Next();
        }

        public String FileName
        {
            get { return m_fileName; }
        }
    }
}
