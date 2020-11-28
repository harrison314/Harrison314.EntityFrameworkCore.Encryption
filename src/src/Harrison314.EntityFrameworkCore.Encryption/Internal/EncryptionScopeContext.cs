using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal
{
    public static class EncryptionScopeContext
    {
        sealed class ContextStackBookmark : IDisposable
        {
            readonly ImmutableStack<IEncryptionContext> bookmark;

            public ContextStackBookmark(ImmutableStack<IEncryptionContext> bookmark)
            {
                this.bookmark = bookmark;
            }

            public void Dispose()
            {
                CurrentStack = this.bookmark;
            }
        }

        static readonly AsyncLocal<ImmutableStack<IEncryptionContext>> Data = new AsyncLocal<ImmutableStack<IEncryptionContext>>();

        private static ImmutableStack<IEncryptionContext> CurrentStack
        {
            get => Data.Value;
            set => Data.Value = value;
        }

        public static IEncryptionContext Current
        {
            get
            {
                ImmutableStack<IEncryptionContext> stack = GetOrCreateEnricherStack();
                if (stack.IsEmpty)
                {
                    return new NullEncryptionContext();
                }
                else
                {
                    return stack.Peek();
                }
            }
        }

        public static bool IsInScope
        {
            get
            {
                ImmutableStack<IEncryptionContext> enrichers = CurrentStack;
                return !(enrichers == null || enrichers.IsEmpty);
            }
        }

        public static IDisposable Push(IEncryptionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            ImmutableStack<IEncryptionContext> stack = GetOrCreateEnricherStack();

            ContextStackBookmark bookmark = new ContextStackBookmark(stack);

            CurrentStack = stack.Push(context);
            return bookmark;
        }

        public static IDisposable Suspend()
        {
            ImmutableStack<IEncryptionContext> stack = GetOrCreateEnricherStack();
            ContextStackBookmark bookmark = new ContextStackBookmark(stack);

            CurrentStack = ImmutableStack<IEncryptionContext>.Empty;

            return bookmark;
        }

        public static void Reset()
        {
            if (CurrentStack != null && CurrentStack != ImmutableStack<IEncryptionContext>.Empty)
            {
                CurrentStack = ImmutableStack<IEncryptionContext>.Empty;
            }
        }

        private static ImmutableStack<IEncryptionContext> GetOrCreateEnricherStack()
        {
            ImmutableStack<IEncryptionContext> enrichers = CurrentStack;
            if (enrichers == null)
            {
                enrichers = ImmutableStack<IEncryptionContext>.Empty;
                CurrentStack = enrichers;
            }

            return enrichers;
        }
    }
}
