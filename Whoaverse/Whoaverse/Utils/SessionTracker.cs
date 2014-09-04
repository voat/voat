/*
This source file is subject to version 3 of the GPL license, 
that is bundled with this package in the file LICENSE, and is 
available online at http://www.gnu.org/licenses/gpl.txt; 
you may not use this file except in compliance with the License. 

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

All portions of the code written by Whoaverse are Copyright (c) 2014 Whoaverse
All Rights Reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whoaverse.Utils
{
    public static class SessionTracker
    {
        public static List<Session> States = new List<Session>();

        // add a new session
        public static void Add(Session state)
        {
            if (!SessionExists(state))
            {
                States.Add(state);
            }            
        }

        // remove a session
        public static void Remove(Session state)
        {
            States.Remove(state);
        }

        // check if session exists
        public static bool SessionExists(Session state)
        {
            var result = from stateCollection in States
                         where stateCollection.Subverse.Equals(state.Subverse) && stateCollection.SessionID.Equals(state.SessionID)
                         select stateCollection;

            if (result.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        // count sessions for given subverse
        public static int ActiveSessionsForSubverse(string subverseName)
        {
            var result = from stateCollection in States
                         where stateCollection.Subverse.Equals(subverseName)
                         select stateCollection;

            return result.Count();
        }
    }
 
    public class Session
    {
        private string _sessionId;
        private string _subverse;
 
        public string SessionID { get { return _sessionId; } set { _sessionId = value; } }
        public string Subverse { get { return _subverse; } set { _subverse = value; } }
    }    
}