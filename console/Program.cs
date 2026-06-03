// MIT License
//
// Copyright (c) 2026 FNIRSI-DPS-150-CONTROL Project
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Main launcher for FNIRSI DPS-150 test programs.
    /// Provides a menu to select between different test applications.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     FNIRSI DPS-150 CONTROL - Test Program Launcher          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Select a test program to run:");
            Console.WriteLine();
            Console.WriteLine("  [1] DPS150 Registers Test Program");
            Console.WriteLine("      - Tests register abstraction layer");
            Console.WriteLine("      - Direct register read/write operations");
            Console.WriteLine();
            Console.WriteLine("  [2] DPS150 Comprehensive Test Program");
            Console.WriteLine("      - Complete DPS150Control class testing");
            Console.WriteLine("      - Interactive device control and testing");
            Console.WriteLine();
            Console.WriteLine("  [Q] Quit");
            Console.WriteLine();
            Console.Write("Enter your choice: ");

            string? choice = Console.ReadLine()?.Trim().ToUpperInvariant();

            Console.WriteLine();
            Console.WriteLine(new string('═', 62));
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Launching DPS150 Registers Test Program...\n");
                    DPS150RegistersTestProgram.Main(args);
                    break;

                case "2":
                    Console.WriteLine("Launching DPS150 Comprehensive Test Program...\n");
                    TestProgram.Main(args);
                    break;

                case "Q":
                    Console.WriteLine("Exiting...");
                    return;

                default:
                    Console.WriteLine("Invalid choice. Press any key to exit...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}
