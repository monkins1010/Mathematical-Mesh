﻿using Goedel.Mesh.Test;

using System.Collections.Generic;

namespace ExampleGenerator {
    public class ShellGroup : ExampleSet {



        public List<ExampleResult> GroupCreate;
        public List<ExampleResult> GroupDecryptAlice;
        public List<ExampleResult> GroupAdd;
        public List<ExampleResult> GroupDecryptBob2;
        public List<ExampleResult> GroupList1;
        public List<ExampleResult> GroupDelete;
        public List<ExampleResult> GroupDecryptBob3;
        public List<ExampleResult> GroupList2;
        public List<ExampleResult> GroupEncrypt;



        public ShellGroup(CreateExamples createExamples) :
            base(createExamples) {

            //GroupCreate = testCLIAlice1.Example($"group create {GroupService}");
            //GroupEncrypt = testCLIBob1.Example(
            //            $"dare encode{TestFile1} {TestFile1Group} /encrypt={GroupService}",
            //            $"dare decode  {TestFile1Group}");
            //GroupDecryptAlice = testCLIAlice1.Example($"dare decode  {TestFile1Group}");
            //GroupAdd = testCLIAlice1.Example($"group add {GroupService} {BobService}");
            //GroupDecryptBob2 = testCLIAlice1.Example($"dare decode  {TestFile1Group}");
            //GroupList1 = testCLIAlice1.Example($"group list {GroupService}");
            //GroupDelete = testCLIAlice1.Example($"group delete {GroupService} {BobService}");
            //GroupDecryptBob3 = testCLIAlice1.Example($"dare decode  {TestFile1Group}");
            //GroupList2 = testCLIAlice1.Example($"group list {GroupService}");

            }


        }
    }
