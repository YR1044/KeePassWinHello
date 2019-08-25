﻿using System;

namespace KeePassWinHello
{
    [Serializable]
    public class AuthProviderInvalidKeyException : AuthProviderException
    {
        public AuthProviderInvalidKeyException() { }
        public AuthProviderInvalidKeyException(string message) : base(message) { }
        public AuthProviderInvalidKeyException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderInvalidKeyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}