
namespace KylesUnityLib.Factory
{

        /// <summary>
        /// Interface for the use of a <see cref="Factory{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IFactory<T>
        {

            /// <summary>
            /// Construct a new object with the creation method given
            /// </summary>
            /// <returns></returns>
            T CreateNewObject();

            /// <summary>
            /// Run clean up delegate of a <typeparamref name="T"/> object
            /// </summary>
            /// <param name="disposed"></param>
            void DisposeObject(T disposed);
        }

        /// <summary>
        /// Interface for the setup of a <see cref="Factory{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IFactorySetup<T>
        {
            ///<summary>
            /// Validates the Factory is in a usable state.
            /// </summary>
            void ValidateFactory();

            /// <summary>
            /// Define the creation of a new <typeparamref name="T"/> object.<br/>
            /// null values will be ignored.
            /// </summary>
            /// <param name="creationAction"></param>
            void DefineCreation(FactoryCreationMethod<T> creationAction);
            /// <summary>
            /// Defines a delegate which will clean up objects, typically before GC finalizes the object.<br/>
            /// Use this to clear references contained within the object.<br/>
            /// Clean Up Action is optional, and calling this when it is not set will do nothing
            /// </summary>
            /// <param name="factoryCleanupMethod"></param>
            void DefineCleanupAction(FactoryCleanupMethod<T> factoryCleanupMethod);

        }
    
}
