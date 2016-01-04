/*  ************************************************************************
 *  Copyright 2015 Charles Young
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  ************************************************************************/

// When AssessorTest is defined, it adds a specialised test for the case where the DoAssessent method is also a computation.
//#define AssessorTest

namespace Halting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a computation over a natural number.
    /// </summary>
    /// <param name="naturalNumber">The natural number, in the range of Uint32.</param>
    public delegate void Computation(uint naturalNumber);

    /// <summary>
    /// This program demonstrates the Halting problem.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Computations over natural numbers, modelled as delegates.  These represent computations that
        /// either halt or do not halt.  We won't actually invoke these anywhere, so they do not need to
        /// implement any functionality for the purpose of this demonstration.
        /// </summary>
        static Computation computation1 = n => Console.Write(string.Format("Executing computation_1({0})", n));
        static Computation computation2 = n => Console.Write(string.Format("Executing computation_2({0})", n));
        static Computation computation3 = n => Console.Write(string.Format("Executing computation_3({0})", n));
        static Computation computation4 = n => Console.Write(string.Format("Executing computation_4({0})", n));
        static Computation computation5 = n => Console.Write(string.Format("Executing computation_5({0})", n));
        static Computation computation7 = n => Console.Write(string.Format("Executing computation_7({0})", n));
        static Computation computation8 = n => Console.Write(string.Format("Executing computation_8({0})", n));

        /// <summary>
        /// A computation over a natural number that happens also to be an assessor method.
        /// </summary>
        static Computation assessorAsComputation = DoAssessment;

        /// <summary>
        /// A dictionary of computations.  This represents an ordered set of computations over natuaral numbers.
        /// </summary>
        static Dictionary<uint, Computation> Computations = new Dictionary<uint, Computation>
            {
                {1, computation1},
                {2, computation2},
                {3, computation3},
                {4, computation4},
                {5, computation5},
                {6, assessorAsComputation},
                {7, computation7},
                {8, computation8}
           };

        /// <summary>
        /// Entry point.  Searches through a subset of the total theoretical problem space of computations
        /// over a single natural number.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.BufferWidth = 100;

            // Simuate search through a small sub-set of the problem space where we know to contain an 
            // example of a Gödel sentance.
            for (var computationIndex = 1U; computationIndex <= 8U; computationIndex++)
            {
                for (var naturalNumber = 1U; naturalNumber <= 8U; naturalNumber++)
                {
                    DoAssessment(computationIndex, naturalNumber);
                }
            }

            Console.WriteLine("Press any key to exit.");
            var keyInfo = Console.ReadKey();
        }

        /// <summary>
        /// Represents the assessment function to assesses computations that take a natural
        /// number as an argument.  This method will always halt if it determines that the
        /// computation does not halt.  If it cannot determine if the computation halts, it
        /// continues in an edless loop.  NB. the user is prompted to break the loop, for
        /// convenience.
        /// </summary>
        /// <param name="computationIndex">The computation index.</param>
        /// <param name="naturalNumber">The natural number.</param>
        static void DoAssessment(uint computationIndex, uint naturalNumber)
        {
            if (computationIndex == naturalNumber)
            {
                // The assessor logically takes a single argument only, because the 
                // computation index and the naturalNumber are identical values.  We will
                // represent this by dispatching to an alternative DoAssessment method 
                // which takes a single argument.
                DoAssessment(naturalNumber);
                return;
            }

            // Set the text used to identify the assessor in the output.
            var assessor = string.Format("Assessor({0}, {1})", computationIndex, naturalNumber);

            // Perform the test to see if the computation halts.
            DoAssessmentTest(assessor, computationIndex, naturalNumber);
        }

        /// <summary>
        /// Represents the assessment function where the computation index is the same as the 
        /// natural number.  In this case, the assessment function takes a single argument.
        /// It has the same signature as a computation and is therefore part of the set of
        /// all computations over a natural number.  Hence, a delegate for this assessor has
        /// been added to the Computations dictionary.
        /// </summary>
        /// <param name="naturalNumber">The natural number.</param>
        static void DoAssessment(uint naturalNumber)
        {
            // Set the text used to identify the assessor in the output.
            var assessor = string.Format("Assessor({0}, {0})", naturalNumber);

            // If the natural number is the same as the comutation index for the 
            // delegate that represents this assessor as a computation in the 
            // Computation dictionary, we reflect this by changing the text used 
            // to identify the assessor in the output.
            if (Computations[naturalNumber].Method == GetCurrentMethod())
            {
                assessor = string.Format("Computation_{0}({0})", naturalNumber);
            }

            // Perform the test to see if the computation halts.
            DoAssessmentTest(assessor, naturalNumber, naturalNumber);
        }

        /// <summary>
        /// Tests if a given computation halts.
        /// </summary>
        /// <param name="assessor">Text representation of the asessor.</param>
        /// <param name="computationIndex">The cmputation index.</param>
        /// <param name="naturalNumber">The natural number.</param>
        static void DoAssessmentTest(string assessor, uint computationIndex, uint naturalNumber)
        {
            if (IsComputationKnownToNeverHalt(assessor, computationIndex, naturalNumber))
            {
                // Halt the assessment on determining that the computation does not halt
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(string.Format("{0} halts, therefore the program knows that Computation_{1}({2}) does not halt.", assessor, computationIndex, naturalNumber));
                Console.ResetColor();
                return;
            }
        }

        /// <summary>
        /// Tests to see if we can determine if a comutatin over a single natural number is known to
        /// never halt.
        /// </summary>
        /// <param name="assessor"></param>
        /// <param name="computationIndex">The computation index.</param>
        /// <param name="naturalNumber">The natural number.</param>
        /// <returns>True, if the computation is known to never halt; otherwise, false.</returns>
        /// <remarks>
        /// This method returns false if it is not known if a computation halts.  False is returned
        /// when the method detects it has entered a loop (i.e., will never halt).  Representing a 
        /// non-halting assessor in this way is useful for demonstration purposes, but adds additional
        /// semantics that could serve to obfuscate the true natur eof the halting problem.  The 
        /// assessor logically only determines one thing - does the computation never halt.  If the 
        /// assessor 'halts' (i.e., returns 'true') we know the computation never halts.  If it returns
        /// 'false', we do not know that the computation never halts.
        /// </remarks>
        static bool IsComputationKnownToNeverHalt(string assessor, uint computationIndex, uint naturalNumber)
        {
            // Function to handle assessment tests that never halt.  
            Action loopForever = () =>
                {
                    var inLoop = false;

                    // The assessment test does not know that the computation never halts.  It may or may not halt.  The 
                    // assessor does not halt in this case.  We enter a never-ending loop.  For demonstration purposes,
                    // however, we will detect and break out of the loop.
                    while (true)
                    {
                        if (inLoop)
                        {
                            return;
                        }

                        inLoop = true;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(string.Format("{0} does not halt, therefore the program does not know if Computation_{1}({2}) halts.", assessor, computationIndex, naturalNumber));
                        Console.ResetColor();
                    }
                };

            // For demonstration purposes, this single default test is used.
            Func<uint, bool> test = n =>
                {
                    if (naturalNumber % 3 != 0)
                    {
                        loopForever();
                        return false;
                    }

                    return true;
                };

            // For demonstration purposes, this code merely simulates testing of a few computations using
            // a single test to determine if they are know to never halt.  The code could, in 
            // principle, be re-written to dispatch to every possible logical test for determining 
            // non-halting bahaviour of any computation over a natural number.  We don't know how large this 
            // method would be, and a real-world implementation would probably be infeasible.  Neverthless, the 
            // simulation is sufficient to demonstrate the halting problem.  In effect, we are constraining the 
            // simulation to range over a tiny area of the full problem space that we know to contain an 
            // example of a Gödel sentance.
            switch (computationIndex)
            {
#if AssessorTest
                case 6U:
                    // We will simulate correct behaviour here when testing 
                    // the case where the assessor is also the computation.
                    if (naturalNumber == 6U)
                    {
                        loopForever();
                        return false;
                    }

                    goto default;
#endif
                default:
                    return test(naturalNumber);
            }
        }

        /// <summary>
        /// Get the current method in which this method is invoked.
        /// </summary>
        /// <returns>The method base containing redlection data about the method.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static MethodBase GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod();
        }
    }
}
