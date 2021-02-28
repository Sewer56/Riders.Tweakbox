using System;
using System.Collections.Generic;
using System.Text;

namespace Sewer56.Imgui.Controls.Extensions
{
    /// <summary>
    /// Provides automatic disposal to disposable objects.
    /// </summary>
    public class Finalized<T> : IDisposable where T : IDisposable
    {
        public T Instance;

        public Finalized(T instance) => Instance = instance;
        ~Finalized() => this.Dispose();

        /// <inheritdoc />
        public void Dispose()
        {
            Instance.Dispose();
            GC.SuppressFinalize(this);
        }

        public static implicit operator Finalized<T>(T value) => new Finalized<T>(value);
        public static implicit operator T(Finalized<T> value) => value.Instance;
    }

    /// <summary>
    /// Provides automatic disposal to disposable arrays.
    /// </summary>
    public class FinalizedList<TList, TElement> : IDisposable where TElement : IDisposable where TList : IList<TElement>
    {
        public TList Instance;

        public FinalizedList(TList instance) => Instance = instance;
        ~FinalizedList() => this.Dispose();

        /// <inheritdoc />
        public void Dispose()
        {
            if (Instance != null)
            {
                for (int x = 0; x < Instance.Count; x++)
                    Instance[x].Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public static implicit operator FinalizedList<TList, TElement>(TList value) => new FinalizedList<TList, TElement>(value);
        public static implicit operator TList(FinalizedList<TList, TElement> value) => value.Instance;
    }
}
