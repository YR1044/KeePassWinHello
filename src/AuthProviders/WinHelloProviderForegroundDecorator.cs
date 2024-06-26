﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeePassWinHello
{
    class WinHelloProviderForegroundDecorator : IAuthProvider
    {
        private readonly IAuthProvider _winHelloProvider;
        private readonly UIContextManager _uiContextManager;

        public WinHelloProviderForegroundDecorator(IAuthProvider provider, UIContextManager uiContextManager)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _winHelloProvider = provider;
            _uiContextManager = uiContextManager;
        }

        public AuthCacheType CurrentCacheType
        {
            get
            {
                return _winHelloProvider.CurrentCacheType;
            }
        }

        public void ClaimCurrentCacheType(AuthCacheType newType)
        {
            _winHelloProvider.ClaimCurrentCacheType(newType);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _winHelloProvider.Encrypt(data);
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                Win32Window.AllowAllSetForeground();
                Task.Factory.StartNew(MakePromptWindowForegroundSafe, tokenSource.Token);

                try
                {
                    var result = _winHelloProvider.PromptToDecrypt(data);
                    BringKeePassMainWindowToFrontSafe();
                    return result;
                }
                catch
                {
                    tokenSource.Cancel();
                    throw;
                } 
            }
        }

        private void BringKeePassMainWindowToFrontSafe()
        {
            try
            {
                var keePassWindowHandle = _uiContextManager.CurrentContext.ParentWindowHandle; //should not be null
                Win32Window.GetOrNull(keePassWindowHandle).EnsureForeground();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        private void MakePromptWindowForegroundSafe()
        {
            try
            {
#if DEBUG
                const string targetWindowClass = null;
#else
                const string targetWindowClass = "Credential Dialog Xaml Host"; 
#endif
                var win = Win32Window.Find(targetWindowClass, "Windows Security", 2000);
                if (win != null)
                    win.EnsureForeground();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }
    }
}
