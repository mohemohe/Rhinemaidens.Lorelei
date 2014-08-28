using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhinemaidens
{
    public class TwitterServerNotWorkingWellException : Exception
    {
        public TwitterServerNotWorkingWellException() { }

        public TwitterServerNotWorkingWellException(string message) : base(message) { }

        public TwitterServerNotWorkingWellException(string message, Exception inner) : base(message) { }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException() { }

        public BadRequestException(string message) : base(message) { }

        public BadRequestException(string message, Exception inner) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() { }

        public UnauthorizedException(string message) : base(message) { }

        public UnauthorizedException(string message, Exception inner) : base(message) { }
    }

    public class TooLongTweetBodyException : Exception
    {
        public TooLongTweetBodyException() { }

        public TooLongTweetBodyException(string message) : base(message) { }

        public TooLongTweetBodyException(string message, Exception inner) : base(message) { }
    }

    public class DuplicateTweetBodyException : Exception
    {
        public DuplicateTweetBodyException() { }

        public DuplicateTweetBodyException(string message) : base(message) { }

        public DuplicateTweetBodyException(string message, Exception inner) : base(message) { }
    }

    public class DeadOrDisconnectedUserStreamException : Exception
    {
        public DeadOrDisconnectedUserStreamException() { }

        public DeadOrDisconnectedUserStreamException(string message) : base(message) { }

        public DeadOrDisconnectedUserStreamException(string message, Exception inner) : base(message) { }
    }
}
