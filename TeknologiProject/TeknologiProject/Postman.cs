using System;
using System.Collections.Generic;
using System.Text;

namespace TeknologiProject
{
    // BRUGES IKKE SOM DET ER NU - I stedet er postmændene tråde.
    public class Postman
    {
        public string Name; // Navn på postmanden
        public bool IsWorking = false; // Indikator for om postmanden er i gang med at arbejde
        public int ID; // Unikt ID for postmanden, som kan bruges til at identificere ham i logs og UI
    }
}
