using System;

namespace Testing.Exceptions
{
    public class CustomException : Exception
    {
        public CustomException(string message, int libraryId) : base(message)
        {
            Data.Add("libraryId", libraryId);
        }
    }
}
