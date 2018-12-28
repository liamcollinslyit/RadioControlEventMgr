using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RadioLibrary.Tests
{
    public class AdminMethodsTest
    {
        // Unit test username must be between 1-30 characters and contain no special characters
        [Theory]
        [InlineData ("username", true)]
        [InlineData ("", false)]
        [InlineData ("a", true)]
        [InlineData ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", false)]
        [InlineData ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", true)]
        [InlineData ("user name", false)]
        [InlineData ("username ", false)]
        [InlineData ("user!name", false)]
        [InlineData ("$username", false)]
        [InlineData ("username^", false)]
        [InlineData ("user&name", false)]
        [InlineData ("_username", false)]
        [InlineData ("username*", false)]
        [InlineData (" ", false)]
        [InlineData ("username1", true)]
        public void CheckUsername_VerifyValidation(string username, bool expected)
        {
            // Arrange
            AdminMethods adminMethods = new AdminMethods();

            // Act
            bool actual = adminMethods.CheckUsername(username);

            // Assert
            Assert.Equal(expected, actual);
        }

        // Unit test password must be between 1-30 characters and allow special characters
        [Theory]
        [InlineData ("password", true)]
        [InlineData ("", false)]
        [InlineData ("a", true)]
        [InlineData ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", false)]
        [InlineData ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", true)]
        [InlineData("pass word", true)]
        [InlineData("password ", true)]
        [InlineData("pass!word", true)]
        [InlineData("$password", true)]
        [InlineData("password^", true)]
        [InlineData("pass&word", true)]
        [InlineData("_password", true)]
        [InlineData("password*", true)]
        [InlineData(" ", true)]
        [InlineData("password1", true)]
        public void CheckPassword_VerifyValidation(string password, bool expected)
        {
            // Arrange
            AdminMethods adminMethods = new AdminMethods();

            // Act
            bool actual = adminMethods.CheckPassword(password);

            // Assert
            Assert.Equal(expected, actual);
        }

        // Unit test forename must be between 1-30 characters and contain no special characters/numbers
        [Theory]
        [InlineData ("forename", true)]
        [InlineData ("", false)]
        [InlineData ("a", true)]
        [InlineData ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", false)]
        [InlineData ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", true)]
        [InlineData("fore name", false)]
        [InlineData("forename ", false)]
        [InlineData("fore!name", false)]
        [InlineData("$forename", false)]
        [InlineData("forename^", false)]
        [InlineData("fore-name", false)]
        [InlineData("_forename", false)]
        [InlineData("forename*", false)]
        [InlineData(" ", false)]
        [InlineData("forename1", false)]
        public void CheckForename_VerifyValidation(string forename, bool expected)
        {
            // Arrange
            AdminMethods adminMethods = new AdminMethods();

            // Act
            bool actual = adminMethods.CheckForename(forename);

            // Assert
            Assert.Equal(expected, actual);
        }

        // Unit test surname must be between 1-30 characters and contain no special characters/numbers
        [Theory]
        [InlineData("surname", true)]
        [InlineData("", false)]
        [InlineData("a", true)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", false)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", true)]
        [InlineData("sur name", false)]
        [InlineData("surname ", false)]
        [InlineData("sur!name", false)]
        [InlineData("$surname", false)]
        [InlineData("surname^", false)]
        [InlineData("sur-name", false)]
        [InlineData("_surname", false)]
        [InlineData("sur'name", false)]
        [InlineData(" ", false)]
        [InlineData("surname1", false)]
        public void CheckSurname_VerifyValidation(string surname, bool expected)
        {
            // Arrange
            AdminMethods adminMethods = new AdminMethods();

            // Act
            bool actual = adminMethods.CheckForename(surname);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData (0, true)]
        [InlineData (1, true)]
        [InlineData(int.MaxValue, true)]
        [InlineData (-1, false)]
        [InlineData(int.MinValue, false)]
        public void CheckAccessLevel_VerifyValidation(int index, bool expected)
        {
            // Arrange
            AdminMethods adminMethods = new AdminMethods();

            // Act
            bool actual = adminMethods.CheckAccessLevel(index);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
