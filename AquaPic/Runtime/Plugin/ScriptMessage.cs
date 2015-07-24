/***************************************************************************************************/
/*        NOT COMPILED                                                                             */
/***************************************************************************************************/

using System;

namespace AquaPic
{
    public class ScriptMessage
    {
        public string errorLocation;
        public string message;

        public ScriptMessage (string errorLocation, string message) {
            this.errorLocation = errorLocation;
            this.message = message;
        }
    }
}

