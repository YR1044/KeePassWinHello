﻿using System;

namespace KeePassWinHello
{
    public enum AuthCacheType
    {
        Persistent,
        Local,
    }

    public interface IAuthProvider
    {
        byte[] Encrypt(byte[] data);
        byte[] PromptToDecrypt(byte[] data);
        void ClaimCurrentCacheType(AuthCacheType authCacheType);
        AuthCacheType CurrentCacheType { get; }
    }

    static class AuthProviderFactory
    {
        public static IAuthProvider GetInstance(IntPtr keePassWindowHandle)
        {
#if DEBUG
            return new XorProvider();
#else
            var provider = WinHelloProvider.CreateInstance(AuthCacheType.Local);

            if (UAC.IsCurrentProcessElevated)
                return new WinHelloProviderForegroundDecorator(provider, keePassWindowHandle);
            else
                return provider;
#endif
        }
    }
}