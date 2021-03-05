﻿using System;

namespace Extension
{
    public class Extension : Interface.Extension
    {
        public string Name { get => "Extension"; }
        public string Description { get => "This is a sample extension."; }

        public int Execute(String Data)
        {
            Console.WriteLine("Hi " + Data + "!");
            Console.WriteLine();
            Console.WriteLine("The current time is " + DateTime.Now.ToString() + ".");
            Console.WriteLine();
            return 0;
        }
    }
}
