using System;

namespace Company.ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
        #if (Option1)
        //Option 1 set
        #endif
        #if (Option2)
        //Option 2 set
        #endif
        #if (Option3)
        //Option 3 set
        #endif
        #if (Option4)
        //Option 4 set
        #endif
            Console.WriteLine("Hello from my custom .NET Core template!");
        }
    }
}
