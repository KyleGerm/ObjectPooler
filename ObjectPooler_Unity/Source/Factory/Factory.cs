using KylesUnityLib.Pooling;
using System;


namespace KylesUnityLib.Factory
{
    /// <summary>
    /// Delegate type for the method used to create an object used in <see cref="Factory{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public delegate T FactoryCreationMethod<T>();
    /// <summary>
    /// Delegate type for the clean up of objects used in <see cref="Factory{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="disposed"></param>
    public delegate void FactoryCleanupMethod<T>(T disposed);
    /// <summary>
    /// A Generic Factory for defining creation and destruction of objects
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class Factory<T> : IFactory<T> where T : class
    {
        /// <summary>
        /// Delegate for object creation
        /// </summary>
        protected FactoryCreationMethod<T> _creationAction;
        /// <summary>
        /// Delegate for optional cleanup of objects
        /// </summary>
        protected FactoryCleanupMethod<T>? _cleanupAction;
        /// <summary>
        /// New factory for creation of <typeparamref name="T"/>
        /// </summary>
        /// <param name="creationMethod"></param>
        public Factory(FactoryCreationMethod<T> creationMethod)
        {
            _creationAction = creationMethod;
        }

        /// <inheritdoc/>
        public virtual T CreateNewObject() =>_creationAction.Invoke();

       /// <inheritdoc/>
        public virtual void DisposeObject(T disposed) => _cleanupAction?.Invoke(disposed);

       /// <inheritdoc/>
        public virtual void DefineCreation(FactoryCreationMethod<T> creationAction)
        {
            if (creationAction == null) return;
            _creationAction = creationAction;
        }
        
        /// <inheritdoc />
       public virtual void DefineCleanup(FactoryCleanupMethod<T> factoryCleanupMethod) => _cleanupAction = factoryCleanupMethod;

        /// <summary>
        /// Validates the creation action exists and is not null. Throws <see cref="ArgumentNullException"/> if not valid
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void ValidateFactory()
        {
            if (_creationAction == null) throw new ArgumentNullException(GetType().Name, "Factory creation method is not defined. Define a creation instruction by calling IFactory.DefineCreationMethod and supplying a valid instruction");
        }
    }

}
