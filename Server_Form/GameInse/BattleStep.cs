using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_Form.GameInse
{
    public class BattleStep: Step
    {
        private Team pTeamA;
        private Team pTeamB;

        public Team PTeamA
        {
            get { return pTeamA; }
            set { pTeamA = value; }
        }
        public Team PTeamB
        {
            get { return pTeamB; }
            set { pTeamB = value; }
        }
    }
}
