using System;
using System.IO;

namespace ConsoleApp1
{
    public class FileGenerator
    {
        private const int NUM_FILES = 200;
        
        /**
         * our process to generate the files. First it picks a random size for
         * the file we are on, and then picks a random number for each line
         */
        public static void run()
        {
            Random gen = new Random();

            for (int i = 1; i <= NUM_FILES; i++)
            {
                int fileSize = gen.Next(1_000, 10_000);

                using (StreamWriter sw = File.CreateText("Files/" + i + ".txt"))
                {
                    for (int j = 0; j < fileSize; j++)
                    {
                        sw.WriteLine(gen.Next().ToString());
                    }
                }
            }
        }
    }
}