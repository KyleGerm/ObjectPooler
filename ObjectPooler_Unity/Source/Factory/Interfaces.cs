
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
            ///<summary>
            /// Validates the Factory is in a usable state.
            /// </summary>
            void ValidateFactory();

        }
    
}
