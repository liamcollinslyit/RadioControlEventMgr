using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioLibrary
{
    public class AdminMethods
   {
        // Check Username is between 1-30 and contain no special characters
        public bool CheckUsername(string username)
        {
            bool validated =true;

            if (username.Length < 1 || username.Length > 30 || username.Any(ch => !Char.IsLetterOrDigit(ch)))
            {
                validated = false;
            }

            return validated;
        }

        // Check Password is between 1-30
        public bool CheckPassword(string password)
        {
            bool validated = true;

            if (password.Length < 1 || password.Length > 30)
            {
                validated = false;
            }

            return validated;
        }

        // Check Forename is between 1-30 and contain no special characters/numbers
        public bool CheckForename(String forename)
        {
            bool validated = true;

            if (forename.Length < 1 || forename.Length > 30 || forename.Any(ch => !Char.IsLetter(ch)))
            {
                validated = false;
            }

            return validated;
        }

        // Check Surname is between 1-30 and contain no special characters/numbers
        public bool CheckSurname(String surname)
        {
            bool validated = true;

            if (surname.Length < 1 || surname.Length > 30 || surname.Any(ch => !Char.IsLetter(ch)))
            {
                validated = false;
            }

            return validated;
        }

        // Check item has been selected from combo box
        public bool CheckAccessLevel(int index)
        {
            bool validated = true;

            if (index < 0)
            {
                validated = false;
            }

            return validated;
        }
    }
}
