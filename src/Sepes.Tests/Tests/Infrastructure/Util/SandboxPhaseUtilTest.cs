using Sepes.Common.Model;
using Sepes.Infrastructure.Model;
using Sepes.Infrastructure.Util;
using System;
using System.Collections.Generic;
using Xunit;

namespace Sepes.Tests.Util
{
    public class SandboxPhaseUtilTest
    {
        //CURRENT PHASE
        [Fact]
        public void CallingCurrentPhase_WithNullArgument_WillThrow()
        {          
            Assert.Throws<ArgumentNullException>(() => SandboxPhaseUtil.GetCurrentPhase(null));
        }

        [Fact]
        public void CallingCurrentPhase_WithPhaseListBeingNull_WillThrow()
        {
            var sandboxWithoutPhase = new Sandbox();
            Assert.Throws<Exception>(() => SandboxPhaseUtil.GetCurrentPhase(sandboxWithoutPhase));
        }

        [Fact]
        public void CallingCurrentPhase_WithEmptyPhaseList_WillReturnOpen()
        {
            var sandboxWithoutPhase = new Sandbox() { PhaseHistory = new List<SandboxPhaseHistory>() {  }  };
            var phaseOfSandbox = SandboxPhaseUtil.GetCurrentPhase(sandboxWithoutPhase);

            Assert.Equal(SandboxPhase.Open, phaseOfSandbox);          
        }

        [Fact]
        public void CallingCurrentPhase_WithLastPhaseBeingOpen_WillReturnOpen()
        {
            var sandboxWithoutPhase = new Sandbox() { PhaseHistory = new List<SandboxPhaseHistory>() { new SandboxPhaseHistory() { Counter = 0, Phase = SandboxPhase.Open } } };
            var phaseOfSandbox = SandboxPhaseUtil.GetCurrentPhase(sandboxWithoutPhase);

            Assert.Equal(SandboxPhase.Open, phaseOfSandbox);
        }

        //NEXT PHASE
        [Fact]
        public void CallingNextPhase_WithNullArgument_WillThrow()
        {
            Assert.Throws<ArgumentNullException>(() => SandboxPhaseUtil.GetNextPhase(null));
        }

        [Fact]
        public void CallingNextPhase_WithPhaseListBeingNull_WillThrow()
        {
            var sandboxWithoutPhase = new Sandbox();
            Assert.Throws<Exception>(() => SandboxPhaseUtil.GetNextPhase(sandboxWithoutPhase));
        }

        [Fact]
        public void CallingNextPhase_WithEmptyPhaseList_WillReturnDataAvailable()
        {
            var sandboxWithoutPhase = new Sandbox() { PhaseHistory = new List<SandboxPhaseHistory>() { } };
            var nextPhaseOfSandbox = SandboxPhaseUtil.GetNextPhase(sandboxWithoutPhase);

            Assert.Equal(SandboxPhase.DataAvailable, nextPhaseOfSandbox);
        }

        [Fact]
        public void CallingNextPhase_WithLastPhaseBegingOpen_WillReturnDataAvailable()
        {
            var sandboxWithoutPhase = new Sandbox() { PhaseHistory = new List<SandboxPhaseHistory>() { new SandboxPhaseHistory() { Counter = 0, Phase = SandboxPhase.Open } } };
            var nextPhaseOfSandbox = SandboxPhaseUtil.GetNextPhase(sandboxWithoutPhase);

            Assert.Equal(SandboxPhase.DataAvailable, nextPhaseOfSandbox);
        }

        [Fact]
        public void CallingNextPhase_WithLastPhaseBegingDataAvailable_WillThrow()
        {
            var sandboxWithoutPhase = new Sandbox() { PhaseHistory = new List<SandboxPhaseHistory>() { new SandboxPhaseHistory() { Counter = 0, Phase = SandboxPhase.DataAvailable } } };
            Assert.Throws<Exception>(() => SandboxPhaseUtil.GetNextPhase(sandboxWithoutPhase));      
        }

        [Fact]
        public void CallingNextPhase_WithSeveralPhases_LastPhaseBegingDataAvailable_WillThrow()
        {
            var sandboxWithoutPhase = new Sandbox() { PhaseHistory = new List<SandboxPhaseHistory>() {
                new SandboxPhaseHistory() { Counter = 0, Phase = SandboxPhase.Open },
                new SandboxPhaseHistory() { Counter = 1, Phase = SandboxPhase.DataAvailable } }
            };
            Assert.Throws<Exception>(() => SandboxPhaseUtil.GetNextPhase(sandboxWithoutPhase));
        }

        [Fact]
        public void CallingNextPhase_WithSeveralPhases_LastPhaseBegingOpen_WillReturnDataAvailable()
        {
            var sandboxWithoutPhase = new Sandbox()
            {
                PhaseHistory = new List<SandboxPhaseHistory>() {
                new SandboxPhaseHistory() { Counter = 0, Phase = SandboxPhase.Open },
                new SandboxPhaseHistory() { Counter = 1, Phase = SandboxPhase.DataAvailable },
                new SandboxPhaseHistory() { Counter = 2, Phase = SandboxPhase.Open },
            }
            };

            var nextPhaseOfSandbox = SandboxPhaseUtil.GetNextPhase(sandboxWithoutPhase);
            Assert.Equal(SandboxPhase.DataAvailable, nextPhaseOfSandbox);
        }
    }
}
