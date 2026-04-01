using GameServer.Database;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
   public class Session
    {
        public Character character; 
        public Space space=> character.Space;
        public DbPlayer dbPlayer;
    }
}
