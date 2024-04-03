using System.Threading.Tasks;

namespace EasyBootstrap
{
    /// <summary>
    /// Object placed in a bootstrap scene.
    /// <see cref="BootstrapLoader"/> will call Initialize() on this object.
    /// Order of Initialize calls is decided by <see cref="BootstrapCallOrder"/>: Lower is called first.
    /// </summary>
    public interface IBootstrappable
    {
        /// <summary>
        /// Order of bootstrapping. Object with the lowest <see cref="BootstrapCallOrder"/> in the scene gets initialized first.
        /// </summary>
        public int BootstrapCallOrder { get; }


        public Task Initialize();
    }
}